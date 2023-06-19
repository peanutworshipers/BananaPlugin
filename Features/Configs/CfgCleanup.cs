namespace BananaPlugin.Features.Configs;

using System.ComponentModel;

/// <summary>
/// The main config for cleanup.
/// </summary>
public sealed class CfgCleanup
{
#warning add softrestart stuff later
    /// <summary>
    /// The default ragdoll cleanup config value.
    /// </summary>
    public const int DefaultRagdollCleanup = 40;

    /// <summary>
    /// Gets or sets the maximum number of allowed ragdolls.
    /// </summary>
    [Description("The maximum number of allowed ragdolls.")]
    public int RagdollCleanup { get; set; } = DefaultRagdollCleanup;
}
