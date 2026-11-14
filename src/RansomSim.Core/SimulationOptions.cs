namespace RansomSim.Core;

/// <summary>
/// Everything the engine needs. Defaults are deliberately harmless.
/// </summary>
public sealed class SimulationOptions
{
    /// <summary>Local or remote share.</summary>
    public SimulationScope Scope { get; set; } = SimulationScope.LocalComputer;

    /// <summary>Folder to simulate against. Required.</summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>Recurse into subdirectories.</summary>
    public bool Recurse { get; set; } = true;

    /// <summary>
    /// When true nothing is written to the target at all; the run only reports what it would have done.
    /// </summary>
    public bool DryRun { get; set; }

    /// <summary>Skip files larger than this many bytes (default 50 MB) to keep lab runs quick.</summary>
    public long MaxFileSizeBytes { get; set; } = 50L * 1024 * 1024;

    /// <summary>Stop after this many files (0 = no limit).</summary>
    public int MaxFiles { get; set; }

    /// <summary>Milliseconds to pause between files, simulating throttled encryption. 0 = flat out.</summary>
    public int DelayBetweenFilesMs { get; set; }

    /// <summary>
    /// Relative paths (or simple file-name patterns such as *.exe) that must never be touched.
    /// The engine always adds its own safety exclusions on top of these.
    /// </summary>
    public List<string> Exclusions { get; } = new();

    /// <summary>When false the engine writes the .locked outputs and does not add the ransom note.</summary>
    public bool WriteRansomNote { get; set; } = true;

    /// <summary>Name of the note dropped in the root folder.</summary>
    public string RansomNoteFileName { get; set; } = "READ_ME_SIMULATION.txt";

    /// <summary>
    /// Optional shared secret used to derive the key. When empty a random key is generated and stored
    /// in the manifest - the lab equivalent of "the key the operator keeps".
    /// </summary>
    public string? Passphrase { get; set; }

    /// <summary>Abort if more than this fraction of the folder would be processed. 0 disables the guard.</summary>
    public double MaxPercentOfFiles { get; set; }

    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(RootPath))
            throw new SimulationException("RootPath is required.");

        if (MaxFileSizeBytes <= 0)
            throw new SimulationException("MaxFileSizeBytes must be greater than zero.");

        if (MaxFiles < 0)
            throw new SimulationException("MaxFiles cannot be negative.");

        if (DelayBetweenFilesMs < 0)
            throw new SimulationException("DelayBetweenFilesMs cannot be negative.");

        if (MaxPercentOfFiles is < 0 or > 100)
            throw new SimulationException("MaxPercentOfFiles must be between 0 and 100.");

        if (string.IsNullOrWhiteSpace(RansomNoteFileName) || RansomNoteFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new SimulationException("RansomNoteFileName is not a valid file name.");
    }

    public SimulationOptions Clone() => new()
    {
        Scope = Scope,
        RootPath = RootPath,
        Recurse = Recurse,
        DryRun = DryRun,
        MaxFileSizeBytes = MaxFileSizeBytes,
        MaxFiles = MaxFiles,
        DelayBetweenFilesMs = DelayBetweenFilesMs,
        WriteRansomNote = WriteRansomNote,
        RansomNoteFileName = RansomNoteFileName,
        Passphrase = Passphrase,
        MaxPercentOfFiles = MaxPercentOfFiles
    };
}
