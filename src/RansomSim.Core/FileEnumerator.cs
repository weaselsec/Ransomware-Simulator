namespace RansomSim.Core;

/// <summary>
/// Walks the target folder depth-first and yields the files that are in scope,
/// applying the built-in safe-mode exclusions before anything is touched.
/// </summary>
internal static class FileEnumerator
{
    /// <summary>Extensions that are always skipped: never re-encrypt our own output, never touch VM backing files.</summary>
    internal static readonly string[] AlwaysSkipExtensions =
    {
        ".locked",          // our own ciphertext
        ".ransomsim",       // manifest / state
        ".vhd", ".vhdx", ".vmdk", ".iso", ".vdi" // virtual disk images
    };

    /// <summary>Partial file names that are always skipped (system-critical files).</summary>
    internal static readonly string[] AlwaysSkipNameFragments =
    {
        "~$",               // Office lock files
        "desktop.ini",
        "thumbs.db"
    };

    /// <summary>Folder names that are never entered.</summary>
    internal static readonly string[] AlwaysSkipFolders =
    {
        ".git", ".svn", ".hg", "$RECYCLE.BIN", "System Volume Information", "node_modules"
    };

    public static IReadOnlyList<string> Collect(SimulationTarget target, SimulationOptions options)
    {
        var results = new List<string>();
        var stack = new Stack<string>();
        stack.Push(target.RootPath);

        // Build the matcher once; it combines built-in rules with user exclusions.
        var filter = new ExclusionFilter(target.RootPath, options.Exclusions);

        // Enumeration is manual rather than SearchOption.AllDirectories so protected
        // folders can be pruned instead of walked and filtered afterwards.
        while (stack.Count > 0)
        {
            var dir = stack.Pop();

            IEnumerable<string> subDirs;
            try
            {
                subDirs = Directory.EnumerateDirectories(dir);
            }
            catch (Exception)
            {
                continue; // unreadable directory: skip, do not fail the whole run
            }

            foreach (var sub in subDirs)
            {
                if (IsProtectedFolder(sub))
                    continue;
                if (filter.IsExcludedPath(sub))
                    continue;
                if (options.Recurse)
                    stack.Push(sub);
            }

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(dir);
            }
            catch (Exception)
            {
                continue;
            }

            foreach (var file in files)
            {
                if (filter.IsExcludedPath(file))
                    continue;
                if (IsProtectedFile(file))
                    continue;

                // Skip anything that looks like a reparse point / symlink to avoid escaping the root.
                try
                {
                    var info = new FileInfo(file);
                    if (info.Attributes.HasFlag(FileAttributes.ReparsePoint))
                        continue;
                    if (info.Length <= 0)
                        continue;
                    if (info.Length > options.MaxFileSizeBytes)
                        continue;
                }
                catch (Exception)
                {
                    continue;
                }

                results.Add(file);
            }
        }

        results.Sort(StringComparer.OrdinalIgnoreCase);
        return results;
    }

    private static bool IsProtectedFolder(string path)
    {
        var name = Path.GetFileName(path);
        return AlwaysSkipFolders.Any(f => string.Equals(f, name, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsProtectedFile(string path)
    {
        var name = Path.GetFileName(path);

        if (name.StartsWith("~$", StringComparison.Ordinal))
            return true;

        foreach (var fragment in AlwaysSkipNameFragments)
        {
            if (name.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        var ext = Path.GetExtension(path);
        return AlwaysSkipExtensions.Any(e => string.Equals(e, ext, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Applies user exclusions (relative paths, folder prefixes, and * / ? glob patterns).</summary>
    private sealed class ExclusionFilter
    {
        private readonly string _root;
        private readonly List<string> _relativePaths = new();
        private readonly List<string> _patterns = new();
        private readonly List<string> _folders = new();

        public ExclusionFilter(string root, IEnumerable<string> exclusions)
        {
            _root = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            foreach (var raw in exclusions)
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                var value = raw.Trim();
                if (value.IndexOfAny(new[] { '*', '?' }) >= 0)
                    _patterns.Add(value);
                else if (value.EndsWith(Path.DirectorySeparatorChar) || value.EndsWith('/'))
                    _folders.Add(value.TrimEnd(Path.DirectorySeparatorChar, '/'));
                else
                    _relativePaths.Add(value.TrimStart('.', Path.DirectorySeparatorChar, '/'));
            }
        }

        public bool IsExcludedPath(string fullPath)
        {
            string rel;
            try
            {
                rel = Path.GetRelativePath(_root, fullPath);
            }
            catch (Exception)
            {
                return true; // cannot reason about it -> do not touch it
            }

            var relNorm = rel.Replace('\\', '/');

            foreach (var folder in _folders)
            {
                var f = folder.Replace('\\', '/');
                if (relNorm.Equals(f, StringComparison.OrdinalIgnoreCase) ||
                    relNorm.StartsWith(f + "/", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            foreach (var item in _relativePaths)
            {
                var normalized = item.Replace('\\', '/');
                if (relNorm.Equals(normalized, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            var fileName = Path.GetFileName(relNorm);
            foreach (var pattern in _patterns)
            {
                if (GlobMatch(fileName, pattern) || GlobMatch(relNorm, pattern.Replace('\\', '/')))
                    return true;
            }

            return false;
        }

        /// <summary>Minimal glob matcher supporting * and ?.</summary>
        private static bool GlobMatch(string input, string pattern)
        {
            int i = 0, p = 0, star = -1, match = 0;

            while (i < input.Length)
            {
                if (p < pattern.Length && (pattern[p] == '?' || char.ToLowerInvariant(pattern[p]) == char.ToLowerInvariant(input[i])))
                {
                    i++; p++;
                }
                else if (p < pattern.Length && pattern[p] == '*')
                {
                    star = p++;
                    match = i;
                }
                else if (star >= 0)
                {
                    p = star + 1;
                    i = ++match;
                }
                else
                {
                    return false;
                }
            }

            while (p < pattern.Length && pattern[p] == '*')
                p++;

            return p == pattern.Length;
        }
    }
}
