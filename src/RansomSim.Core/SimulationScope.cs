namespace RansomSim.Core;

/// <summary>
/// Where the simulation should read files from.
/// </summary>
public enum SimulationScope
{
    /// <summary>A folder on this computer (for example C:\RansomSimLab).</summary>
    LocalComputer = 0,

    /// <summary>A UNC path pointing at a network share (for example \\fileserver\lab-share).</summary>
    RemoteShare = 1
}

/// <summary>
/// A validated target: the scope plus the resolved root directory.
/// </summary>
public sealed class SimulationTarget
{
    public SimulationScope Scope { get; }

    /// <summary>Full path to the folder that will be simulated against.</summary>
    public string RootPath { get; }

    /// <summary>True when the root is reached over SMB/UNC rather than a local volume.</summary>
    public bool IsRemote => RootPath.StartsWith(@"\\", StringComparison.Ordinal) || RootPath.StartsWith("//", StringComparison.Ordinal);

    private SimulationTarget(SimulationScope scope, string rootPath)
    {
        Scope = scope;
        RootPath = rootPath;
    }

    /// <summary>
    /// Validates the user supplied path against the chosen scope and returns a target.
    /// Throws <see cref="SimulationException"/> with an actionable message when the input is unusable.
    /// </summary>
    public static SimulationTarget Create(SimulationScope scope, string? rawPath)
    {
        if (string.IsNullOrWhiteSpace(rawPath))
            throw new SimulationException("No folder selected. Pick a " +
                (scope == SimulationScope.LocalComputer ? "local folder" : @"UNC share such as \\server\share") + ".");

        var path = rawPath!.Trim().Trim('"');

        if (scope == SimulationScope.RemoteShare && !IsUncPath(path))
            throw new SimulationException(@"Remote share mode needs a UNC path (\\server\share[\folder]). " +
                "Mapped drive letters resolve to local paths and are rejected on purpose.");

        string full;
        try
        {
            full = Path.GetFullPath(path);
        }
        catch (Exception ex)
        {
            throw new SimulationException($"'{path}' is not a usable path: {ex.Message}", ex);
        }

        if (!Directory.Exists(full))
            throw new SimulationException($"Folder not found or not reachable: {full}");

        if (IsProtectedSystemPath(full))
            throw new SimulationException($"Refusing to simulate inside an operating-system directory ({full}). " +
                "Pick a normal data folder, for example C:\\RansomSimLab.");

        // A drive root or filesystem root is too broad for a simulation.
        if (string.Equals(full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                          Path.GetPathRoot(full)?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                          StringComparison.OrdinalIgnoreCase))
            throw new SimulationException($"Refusing to use a filesystem root ({full}) as the simulation root. " +
                "Create a dedicated lab folder instead.");

        return new SimulationTarget(scope, full);
    }

    private static bool IsUncPath(string path) =>
        path.StartsWith(@"\\", StringComparison.Ordinal) || path.StartsWith("//", StringComparison.Ordinal);

    /// <summary>Directory names that must never become a simulation root.</summary>
    private static readonly string[] ProtectedSystemFolders =
    {
        "windows", "program files", "program files (x86)", "programdata",
        "system volume information", "$recycle.bin",
        "usr", "etc", "bin", "sbin", "boot", "lib", "lib64", "proc", "sys", "dev"
    };

    /// <summary>True when the path is, or sits inside, an operating-system directory.</summary>
    public static bool IsProtectedSystemPath(string path)
    {
        var normalized = path.Replace('\\', '/').ToLowerInvariant();
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Ignore the drive/volume prefix when comparing segment names.
        foreach (var segment in segments)
        {
            if (ProtectedSystemFolders.Contains(segment, StringComparer.Ordinal))
                return true;
        }

        return false;
    }

    public override string ToString() => $"{Scope}: {RootPath}";
}
