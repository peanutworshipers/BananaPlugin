namespace BananaPlugin.Features.Configs;

using System.ComponentModel;

/// <summary>
/// The main config for pink candy bowl.
/// </summary>
public sealed class CfgPinkCandyBowl
{
    /// <summary>
    /// The default pink candy weight config value.
    /// </summary>
    public const float DefaultPinkCandyWeight = 0.12244897f; // ~2% chance

    /// <summary>
    /// Gets or sets the pink candy weight.
    /// </summary>
    [Description("The pink candy weight.")]
    public float PinkCandyWeight { get; set; } = DefaultPinkCandyWeight;
}
