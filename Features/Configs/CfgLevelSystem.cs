namespace BananaPlugin.Features.Configs;

using BananaPlugin.Features.LevelSystemFeature.Configs;

using System.ComponentModel;

/// <summary>
/// The main config for level system.
/// </summary>
public sealed class CfgLevelSystem
{
    /// <summary>
    /// Gets or sets where and how the database file is saved.
    /// </summary>
    public DatabaseConfig DatabaseConfig { get; set; } = new DatabaseConfig();

    /// <summary>
    /// Gets or sets the hint configuration.
    /// </summary>
    [Description("Configure The Hints shown for Leveling")]
    public HintConfig HintConfiguration { get; set; } = new();

    /// <summary>
    /// Gets or sets how much XP is required to reach level 1.
    /// </summary>
    [Description("How much XP a player must get to reach the first level")]
    public int StartXPForLevelUp { get; set; } = 1000;

    /// <summary>
    /// Gets or sets how much XP is required adding up to each level.
    /// </summary>
    [Description("How much XP will be added to the level up requirement per level")]
    public int XPForLevelUpGrowth { get; set; } = 200;

    /// <summary>
    /// Gets or sets the XP sources configuration.
    /// </summary>
    [Description("Configure The different sources of XP")]
    public XPSourceConfig XPSources { get; set; } = new();

    /// <summary>
    /// Gets or sets the level badge configuration.
    /// </summary>
    [Description("Configure Badge Rewards for Leveling")]
    public BadgeConfig BadgeRewards { get; set; } = new();
}