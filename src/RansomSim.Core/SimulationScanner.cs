namespace RansomSim.Core;

/// <summary>
/// Read-only planning helper: answers "which files would this run touch?" without writing anything.
/// The GUI uses it to preview a selection before the operator commits.
/// </summary>
public static class SimulationScanner
{
    /// <summary>Validates the target and returns the files that would be processed, sorted.</summary>
    public static IReadOnlyList<string> Collect(SimulationTarget target, SimulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(options);

        // Copy the options so planning never mutates the caller's configuration.
        options.Validate();
        return FileEnumerator.Collect(target, options);
    }

    /// <summary>Convenience overload that builds the target from scope + path.</summary>
    public static IReadOnlyList<string> Collect(SimulationScope scope, string path, SimulationOptions? options = null)
    {
        var target = SimulationTarget.Create(scope, path);
        var opts = options ?? new SimulationOptions { Scope = scope, RootPath = target.RootPath };
        opts.Scope = scope;
        opts.RootPath = target.RootPath;
        return Collect(target, opts);
    }
}
