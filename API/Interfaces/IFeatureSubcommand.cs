namespace BananaPlugin.API.Interfaces;

using Main;

/// <summary>
/// Implements a feature subcommand.
/// </summary>
/// <typeparam name="T">The feature associated with this subcommand.</typeparam>
public interface IFeatureSubcommand<T> : IFullCommand
    where T : BananaFeature
{
    /// <summary>
    /// Gets the parent feature of this subcommand.
    /// </summary>
    public T Parent { get; }
}
