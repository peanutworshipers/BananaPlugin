namespace BananaPlugin.API.Interfaces;

/// <summary>
/// Defines a command that has a rank requirement to execute.
/// </summary>
public interface IRequiresRank
{
    /// <summary>
    /// Gets the rank requried to execute the command.
    /// </summary>
    public BRank RankRequirement { get; }
}
