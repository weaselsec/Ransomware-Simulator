using System.Text.Json;
using System.Text.Json.Serialization;

namespace RansomSim.Core;

/// <summary>One processed file.</summary>
public sealed class ManifestEntry
{
    public string OriginalPath { get; set; } = string.Empty;
    public string LockedPath { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Sha256Original { get; set; } = string.Empty;
    public string Sha256Locked { get; set; } = string.Empty;
}

/// <summary>
/// Persisted state for one simulation run: what was touched and the key needed to undo it.
/// This is the "operator's key" in a real incident; in the lab it is stored next to the run.
/// </summary>
public sealed class SimulationManifest
{
    public const string FileExtension = ".ransomsim.json";

    public string Tool { get; set; } = "RansomSim";
    public string ToolVersion { get; set; } = "1.0.0";
    public string Mode { get; set; } = "SIMULATION";
    public string RunId { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    public string Scope { get; set; } = string.Empty;
    public string RootPath { get; set; } = string.Empty;
    public bool DryRun { get; set; }

    /// <summary>Base64 AES-256 key. Present only when a random key was generated.</summary>
    public string? KeyBase64 { get; set; }

    /// <summary>True when the key was derived from a passphrase instead of stored.</summary>
    public bool KeyFromPassphrase { get; set; }

    public List<ManifestEntry> Entries { get; set; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string PathForRun(string rootPath, string runId) =>
        Path.Combine(rootPath, $"{runId}{FileExtension}");

    public void Save(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(this, JsonOptions);
        File.WriteAllText(path, json);
    }

    public static SimulationManifest Load(string path)
    {
        if (!File.Exists(path))
            throw new SimulationException($"Manifest not found: {path}");

        try
        {
            var manifest = JsonSerializer.Deserialize<SimulationManifest>(File.ReadAllText(path), JsonOptions);
            if (manifest is null)
                throw new SimulationException($"Manifest is empty or unreadable: {path}");
            return manifest;
        }
        catch (JsonException ex)
        {
            throw new SimulationException($"Manifest is not valid JSON: {path} ({ex.Message})", ex);
        }
    }

    /// <summary>Finds the newest manifest in a folder (used by restore when no run id is given).</summary>
    public static string? FindLatest(string rootPath)
    {
        if (!Directory.Exists(rootPath))
            return null;

        return Directory.EnumerateFiles(rootPath, "*" + FileExtension, SearchOption.TopDirectoryOnly)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }
}
