namespace RansomSim.Core;

/// <summary>
/// Raised for expected, user-fixable simulation problems (bad path, guard tripped, nothing to do).
/// </summary>
public sealed class SimulationException : Exception
{
    public SimulationException(string message) : base(message) { }

    public SimulationException(string message, Exception inner) : base(message, inner) { }
}
