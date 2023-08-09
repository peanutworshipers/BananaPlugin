namespace BananaPlugin.Features.Configs;

using System.ComponentModel;

/// <summary>
/// The main config for guard escape.
/// </summary>
public sealed class CfgBetterEscape
{
    /// <summary>
    /// Gets or sets a value indicating the number of tokens granted to C.I. when a cuffed guard escapes.
    /// </summary>
    [Description("Indicates the number of tokens granted to C.I. when a cuffed guard escapes.")]
    public int EscapeTokens { get; set; } = 1;
}
