using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace RansomSim.Core;

/// <summary>
/// The simulation engine.
///
/// SAFETY CONTRACT (this is what makes it a simulation and not ransomware):
///   1. Original files are opened read-only and are NEVER modified, renamed or deleted.
///   2. Ciphertext is written to a NEW file with the .locked extension next to the original.
///   3. A manifest records every file plus the key, so <see cref="Restore"/> can undo everything.
///   4. Built-in exclusions skip system/VM files, and a percentage guard aborts runaway runs.
/// </summary>
public sealed class RansomSimulator
{
    /// <summary>Raised for each processed file so a GUI can show live progress.</summary>
    public event Action<FileResult>? FileProcessed;

    /// <summary>Raised with human readable status text.</summary>
    public event Action<string>? Status;

    /// <summary>Set to cancel a long run between files.</summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Runs an encryption pass. Returns a report; never throws for per-file problems
    /// (those are captured in the report) but throws <see cref="SimulationException"/> for setup errors.
    /// </summary>
    public SimulationReport Run(SimulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        var target = SimulationTarget.Create(options.Scope, options.RootPath);
        var runId = $"run-{DateTime.Now:yyyyMMdd-HHmmss}-{RandomNumberGenerator.GetHexString(4).ToLowerInvariant()}";

        var report = new SimulationReport
        {
            RunId = runId,
            Scope = target.Scope.ToString(),
            RootPath = target.RootPath,
            DryRun = options.DryRun,
            StartedAt = DateTimeOffset.Now
        };

        var manifest = new SimulationManifest
        {
            RunId = runId,
            StartedAt = report.StartedAt,
            Scope = target.Scope.ToString(),
            RootPath = target.RootPath,
            DryRun = options.DryRun
        };

        Status?.Invoke($"Scanning {target.RootPath} ({target.Scope}) ...");
        var candidates = FileEnumerator.Collect(target, options);

        if (candidates.Count == 0)
        {
            report.CompletedAt = DateTimeOffset.Now;
            throw new SimulationException($"No eligible files found under {target.RootPath}. " +
                "Add a few test files (txt, docx, jpg, csv) and try again.");
        }

        if (options.MaxPercentOfFiles > 0 && candidates.Count >
            (options.MaxPercentOfFiles / 100.0) * CountAllFiles(target.RootPath))
        {
            throw new SimulationException(
                $"Safety guard: the run would touch {candidates.Count} files, more than " +
                $"{options.MaxPercentOfFiles:0.#}% of the folder. Narrow the root or raise MaxPercentOfFiles.");
        }

        if (options.MaxFiles > 0 && candidates.Count > options.MaxFiles)
        {
            candidates = candidates.Take(options.MaxFiles).ToList();
            report.SkippedReasons.Add($"Limited to the first {options.MaxFiles} files (MaxFiles).");
        }

        Status?.Invoke($"Found {candidates.Count} file(s). Mode={(options.DryRun ? "DRY-RUN" : "SIMULATE")}");

        var key = SimulationCipher.CreateKey(options.Passphrase, runId);
        if (string.IsNullOrEmpty(options.Passphrase))
            manifest.KeyBase64 = Convert.ToBase64String(key);
        else
            manifest.KeyFromPassphrase = true;

        foreach (var file in candidates)
        {
            if (CancellationToken.IsCancellationRequested)
            {
                report.SkippedReasons.Add("Cancelled by user.");
                break;
            }

            var sw = Stopwatch.StartNew();
            var relPath = Path.GetRelativePath(target.RootPath, file);

            if (SimulationCipher.IsLockedFile(file))
            {
                report.Files.Add(new FileResult { Path = relPath, Outcome = FileOutcome.Skipped, Message = "already simulated" });
                continue;
            }

            if (options.DryRun)
            {
                var size = SafeLength(file);
                report.Files.Add(new FileResult { Path = relPath, Outcome = FileOutcome.Skipped, SizeBytes = size, Message = "dry-run" });
                OnFile(new FileResult { Path = relPath, Outcome = FileOutcome.Skipped, SizeBytes = size, Message = "dry-run" });
                continue;
            }

            try
            {
                var lockedPath = SimulationCipher.LockedPathFor(file);
                var written = SimulationCipher.EncryptFile(file, lockedPath, key);

                var entry = new ManifestEntry
                {
                    OriginalPath = file,
                    LockedPath = lockedPath,
                    SizeBytes = written,
                    Sha256Original = HashFile(file),
                    Sha256Locked = HashFile(lockedPath)
                };
                manifest.Entries.Add(entry);

                // The original must still be there, untouched. Verify rather than assume.
                if (!File.Exists(file))
                    throw new SimulationException($"Original file disappeared: {file}");

                var result = new FileResult
                {
                    Path = relPath,
                    Outcome = FileOutcome.Encrypted,
                    SizeBytes = written,
                    ElapsedMs = sw.Elapsed.TotalMilliseconds
                };
                report.Files.Add(result);
                OnFile(result);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SimulationException)
            {
                var result = new FileResult
                {
                    Path = relPath,
                    Outcome = FileOutcome.Failed,
                    ElapsedMs = sw.Elapsed.TotalMilliseconds,
                    Message = ex.Message
                };
                report.Files.Add(result);
                OnFile(result);
            }

            if (options.DelayBetweenFilesMs > 0)
                Thread.Sleep(options.DelayBetweenFilesMs);
        }

        if (!options.DryRun && options.WriteRansomNote && manifest.Entries.Count > 0)
        {
            var notePath = Path.Combine(target.RootPath, options.RansomNoteFileName);
            File.WriteAllText(notePath, BuildRansomNote(runId, manifest.Entries.Count, target));
            report.RansomNotePath = notePath;
            Status?.Invoke($"Ransom note written: {notePath}");
        }

        if (!options.DryRun)
        {
            manifest.CompletedAt = DateTimeOffset.Now;
            var manifestPath = SimulationManifest.PathForRun(target.RootPath, runId);
            manifest.Save(manifestPath);
            report.ManifestPath = manifestPath;
            Status?.Invoke($"Manifest written: {manifestPath}");
        }

        report.CompletedAt = DateTimeOffset.Now;
        return report;
    }

    /// <summary>Undoes a previous run: decrypts every .locked file back and removes it.</summary>
    /// <param name="manifestPath">The run-*.ransomsim.json produced by <see cref="Run"/>.</param>
    /// <param name="keepLockedFiles">Keep the .locked copies after a successful decrypt.</param>
    /// <param name="passphrase">Required only when the run derived its key from a passphrase.</param>
    public SimulationReport Restore(string manifestPath, bool keepLockedFiles = false, string? passphrase = null)
    {
        var manifest = SimulationManifest.Load(manifestPath);

        var report = new SimulationReport
        {
            RunId = manifest.RunId,
            Scope = manifest.Scope,
            RootPath = manifest.RootPath,
            StartedAt = DateTimeOffset.Now
        };

        byte[] key;
        if (!string.IsNullOrEmpty(manifest.KeyBase64))
        {
            key = Convert.FromBase64String(manifest.KeyBase64);
        }
        else if (manifest.KeyFromPassphrase)
        {
            if (string.IsNullOrEmpty(passphrase))
                throw new SimulationException(
                    "This run used a passphrase-derived key. Restore again with the same passphrase.");

            key = SimulationCipher.CreateKey(passphrase, manifest.RunId);
        }
        else
        {
            throw new SimulationException("Manifest contains no key and no passphrase flag; cannot restore.");
        }

        var authenticationFailures = 0;

        foreach (var entry in manifest.Entries)
        {
            if (CancellationToken.IsCancellationRequested)
            {
                report.SkippedReasons.Add("Cancelled by user.");
                break;
            }

            var rel = Path.GetRelativePath(manifest.RootPath, entry.OriginalPath);
            if (!File.Exists(entry.LockedPath))
            {
                report.Files.Add(new FileResult { Path = rel, Outcome = FileOutcome.Skipped, Message = "locked file missing" });
                continue;
            }

            try
            {
                SimulationCipher.DecryptFile(entry.LockedPath, entry.OriginalPath, key);

                // Integrity check against the manifest before we declare success.
                var restoredHash = HashFile(entry.OriginalPath);
                if (!string.Equals(restoredHash, entry.Sha256Original, StringComparison.OrdinalIgnoreCase))
                    throw new SimulationException("Restored file hash does not match the manifest.");

                if (!keepLockedFiles)
                    File.Delete(entry.LockedPath);

                var result = new FileResult
                {
                    Path = rel,
                    Outcome = FileOutcome.Restored,
                    SizeBytes = entry.SizeBytes
                };
                report.Files.Add(result);
                OnFile(result);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or SimulationException or CryptographicException)
            {
                // A tag mismatch means the key does not belong to these .locked files. Nothing was
                // written for that entry, so the original is untouched; count it for the summary below.
                if (ex is CryptographicException)
                    authenticationFailures++;

                var result = new FileResult { Path = rel, Outcome = FileOutcome.Failed, Message = ex.Message };
                report.Files.Add(result);
                OnFile(result);
            }
        }

        // Every file failing authentication almost always means a wrong passphrase, which deserves
        // a clear message instead of a wall of per-file errors.
        if (report.RestoredCount == 0 && authenticationFailures > 0)
            throw new SimulationException(
                "Nothing could be restored: authentication failed for every locked file. The passphrase is " +
                "probably wrong, or these .locked files belong to a different run. Original files were not touched.");

        // Clean up the note we dropped, if it is ours.
        try
        {
            var note = Directory.EnumerateFiles(manifest.RootPath, "READ_ME_SIMULATION*.txt").FirstOrDefault();
            if (note is not null)
                File.Delete(note);
        }
        catch (Exception)
        {
            // Non-fatal.
        }

        report.CompletedAt = DateTimeOffset.Now;
        return report;
    }

    /// <summary>Restore with a caller supplied passphrase when the manifest did not store a key.</summary>
    public SimulationReport RestoreWithPassphrase(string manifestPath, string passphrase, bool keepLockedFiles = false)
        => Restore(manifestPath, keepLockedFiles, passphrase);

    private void OnFile(FileResult result)
    {
        try { FileProcessed?.Invoke(result); }
        catch (Exception) { /* a bad subscriber must not break the run */ }
    }

    private static long SafeLength(string path)
    {
        try { return new FileInfo(path).Length; }
        catch (Exception) { return 0; }
    }

    private static int CountAllFiles(string root)
    {
        try
        {
            return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Count();
        }
        catch (Exception)
        {
            return int.MaxValue; // unknown -> let the guard stay permissive but don't crash
        }
    }

    private static string HashFile(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string BuildRansomNote(string runId, int fileCount, SimulationTarget target)
    {
        var sb = new StringBuilder();
        sb.AppendLine("================================================================");
        sb.AppendLine("            *** TRAINING SIMULATION - NOT REAL RANSOMWARE ***");
        sb.AppendLine("================================================================");
        sb.AppendLine();
        sb.AppendLine("This file was created by RansomSim, a defensive training tool.");
        sb.AppendLine("NO ORIGINAL FILE WAS MODIFIED, RENAMED OR DELETED.");
        sb.AppendLine();
        sb.AppendLine($"Run ID      : {runId}");
        sb.AppendLine($"Scope       : {target.Scope}");
        sb.AppendLine($"Root        : {target.RootPath}");
        sb.AppendLine($"Simulated   : {fileCount} file(s) got a .locked copy");
        sb.AppendLine();
        sb.AppendLine("What actually happened");
        sb.AppendLine("----------------------");
        sb.AppendLine("Every processed file still exists with its original name and content.");
        sb.AppendLine("An AES-256-GCM encrypted copy was written next to it with a .locked");
        sb.AppendLine("extension, exactly like a real ransomware note would claim.");
        sb.AppendLine();
        sb.AppendLine("How to undo it");
        sb.AppendLine("--------------");
        sb.AppendLine("Open RansomSim.Gui, click Restore, and select this run's manifest:");
        sb.AppendLine($"  {SimulationManifest.PathForRun(target.RootPath, runId)}");
        sb.AppendLine();
        sb.AppendLine("Why this is safe to run");
        sb.AppendLine("-----------------------");
        sb.AppendLine("* Files are opened read-only; the encoder only ever writes .locked");
        sb.AppendLine("* A JSON manifest lists every file and stores the key");
        sb.AppendLine("* Restore verifies SHA-256 of each decrypted file");
        sb.AppendLine();
        sb.AppendLine("Detection telemetry this run should generate");
        sb.AppendLine("-------------------------------------------");
        sb.AppendLine("* Hundreds of file writes with a single new extension in one folder");
        sb.AppendLine("* AES-256 encryption of user documents at high rate");
        sb.AppendLine("* Creation of a ransom note text file in the root");
        sb.AppendLine("* If remote: SMB write burst from one client to one share");
        sb.AppendLine();
        return sb.ToString();
    }
}
