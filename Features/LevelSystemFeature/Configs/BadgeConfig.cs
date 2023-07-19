namespace BananaPlugin.Features.LevelSystemFeature.Configs;

using System.ComponentModel;

/// <summary>
/// The class used to represent badge configurations for the level system.
/// </summary>
/// <remarks>Credits to <see href="https://github.com/creepycats"/> for original source.</remarks>
public sealed class BadgeConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether players should be given badges based on their level.
    /// </summary>
    [Description("Should players be given badges based on their level?")]
    public bool EnableBadges { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether level badges with colors should override server badge colors.
    /// </summary>
    [Description("Should badges with colors override Server badge colors?")]
    public bool BadgesOverrideColor { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether level badges are shown by default.
    /// </summary>
    [Description("Do players by default show their level badge?")]
    public bool DefaultBadgeVisibility { get; set; } = true;

    /// <summary>
    /// Gets or sets the level badge format.
    /// </summary>
    /// <remarks>Variables: <b>%xp% %xp-next% %level% %badge</b>.</remarks>
    [Description("Badge Format - Variables: %xp% %xp-next% %level% %badge%")]
    public string BadgeFormat { get; set; } = "- LVL %level% | %badge% -";

#warning implement this later plis
#if false
    /// <summary>
    /// Gets or sets the badges than can be unlocked.
    /// </summary>
    public Dictionary<string, BadgeInfo> UnlockableBadges { get; set; } = new Dictionary<string, BadgeInfo>()
    {
        ["default"] = new BadgeInfo("Visitor", "none", 0),
        ["beginner"] = new BadgeInfo("Beginner", "cyan", 5),
        ["amateur"] = new BadgeInfo("Amateur", "aqua", 10),
        ["apprentice"] = new BadgeInfo("Apprentice", "blue_green", 15),
        ["regular"] = new BadgeInfo("Regular", "mint", 25),
    };
#endif
}
