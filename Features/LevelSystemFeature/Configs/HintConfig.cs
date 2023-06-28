namespace BananaPlugin.Features.LevelSystemFeature.Configs;

using System.ComponentModel;

/// <summary>
/// The class used to represent hint configurations for the level system.
/// </summary>
/// <remarks>Credits to <see href="https://github.com/creepycats"/> for original source.</remarks>
public sealed class HintConfig
{
    /// <summary>
    /// Gets or sets how big should the hint be.
    /// </summary>
    [Description("How big should the Hint be?")]
    public int HintSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets how far down on the screen the hint should be.
    /// </summary>
    [Description("How far down on the screen should the hint be?")]
    public int HintYOffset { get; set; } = 440;

    /// <summary>
    /// Gets or sets a value indicating whether the player should receive a hint when gaining XP.
    /// </summary>
    [Description("Should the Player receive a Hint for XP Gain?")]
    public bool ShowXPGain { get; set; } = true;

    /// <summary>
    /// Gets or sets the message for gaining XP.
    /// </summary>
    /// <remarks>Variables: <b>%xp% %xp-gained% %xp-next% %level% %levels-gained%</b>.</remarks>
    [Description("Message for the XP Gain - Variables: %xp% %xp-gained% %xp-next% %level% %levels-gained%")]
    public string XPGainMessage { get; set; } = "<color=#0F0>+ %xp-gained% </color>XP";

    /// <summary>
    /// Gets or sets a value indicating whether the player should receive a hint when leveling up.
    /// </summary>
    [Description("Should the Player receive a Hint for Leveling Up?")]
    public bool ShowLevelUp { get; set; } = true;

    /// <summary>
    /// Gets or sets the message for leveling up.
    /// </summary>
    /// <remarks>Variables: <b>%xp% %xp-gained% %xp-next% %level% %levels-gained%</b>.</remarks>
    [Description("Message for the Level Up - Variables: %xp% %xp-gained% %xp-next% %level% %levels-gained%")]
    public string LevelUpMessage { get; set; } = "Level Up! You are now Level <color=#0F0>%level%</color> (<color=#0F0>+%levels-gained% </color>Levels)";

    /// <summary>
    /// Gets or sets a value indicating whether an XP bar should be displayed when receiving a hint.
    /// </summary>
    [Description("Should the Player receive a Hint with a XP Bar in it?")]
    public bool ShowXPBar { get; set; } = true;

    /// <summary>
    /// Gets or sets the XP bar configuration associated with this instance.
    /// </summary>
    [Description("Configure the XP Bar here")]
    public XPBarConfig XPBarConfig { get; set; } = new XPBarConfig { };
}
