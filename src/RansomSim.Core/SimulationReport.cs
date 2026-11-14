namespace RansomSim.Core;

/// <summary>Outcome of a single file.</summary>
public enum FileOutcome
{
    Encrypted,
    Restored,
    Skipped,
    Failed
}

public sealed class FileResult
{
    public string Path { get; init; } = string.Empty;
    public FileOutcome Outcome { get; init; }
    public long SizeBytes { get; init; }
    public double ElapsedMs { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Full result of a run, exposed to the CLI and GUI for logging and reporting.
/// </summary>
public sealed class SimulationReport
{
    public string RunId { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public string RootPath { get; init; } = string.Empty;
    public bool DryRun { get; init; }
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.Now;
    public DateTimeOffset CompletedAt { get; set; } = DateTimeOffset.Now;

    public List<FileResult> Files { get; } = new();
    public List<string> SkippedReasons { get; } = new();
    public string? ManifestPath { get; set; }
    public string? RansomNotePath { get; set; }
    public string? Error { get; set; }

    public int EncryptedCount => Files.Count(f => f.Outcome == FileOutcome.Encrypted);
    public int RestoredCount => Files.Count(f => f.Outcome == FileOutcome.Restored);
    public int FailedCount => Files.Count(f => f.Outcome == FileOutcome.Failed);
    public int SkippedCount => Files.Count(f => f.Outcome == FileOutcome.Skipped);
    public long BytesProcessed => Files.Where(f => f.Outcome is FileOutcome.Encrypted or FileOutcome.Restored).Sum(f => f.SizeBytes);

    public TimeSpan Duration => CompletedAt - StartedAt;

    public double FilesPerSecond =>
        Duration.TotalSeconds <= 0 ? 0 : Files.Count(f => f.Outcome is FileOutcome.Encrypted or FileOutcome.Restored) / Duration.TotalSeconds;

    public double MegabytesPerSecond =>
        Duration.TotalSeconds <= 0 ? 0 : BytesProcessed / 1024d / 1024d / Duration.TotalSeconds;

    public bool Success => Error is null && FailedCount == 0;
}
