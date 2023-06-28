namespace BananaPlugin.Features.LevelSystemFeature.Configs;

using System.ComponentModel;

/// <summary>
/// The class used to represent xp bar configurations for the level system.
/// </summary>
/// <remarks>Credits to <see href="https://github.com/creepycats"/> for original source.</remarks>
public sealed class XPBarConfig
{
    /// <summary>
    /// Gets or sets how long the xp bar should be in characters.
    /// </summary>
    [Description("How long should the XP bar be?")]
    public int XPBarLength { get; set; } = 20;

    /// <summary>
    /// Gets or sets the hex color used when displaying XP the player has.
    /// </summary>
    [Description("Color that represents the XP the player has")]
    public string XPColor { get; set; } = "#00ff00";

    /// <summary>
    /// Gets or sets the hex color used when displaying XP the player is missing.
    /// </summary>
    [Description("Color that represents the XP the player is missing.")]
    public string XPMissingColor { get; set; } = "#ff0000";
}
