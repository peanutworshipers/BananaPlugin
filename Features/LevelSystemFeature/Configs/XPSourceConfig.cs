namespace BananaPlugin.Features.LevelSystemFeature.Configs;

using ExEnums;
using PlayerRoles;
using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// The class used to represent XP source configurations in the level system.
/// </summary>
/// <remarks>Credits to <see href="https://github.com/creepycats"/> for original source.</remarks>
public sealed class XPSourceConfig
{
    /// <summary>
    /// Gets or sets how much XP should be gained when interacting with certain doors.
    /// </summary>
    [Description("How Much XP Should be Gained when interacting with X type of Door?")]
    public Dictionary<DoorType, int> DoorXP { get; set; } = new()
    {
        [DoorType.Intercom] = 10,
        [DoorType.Scp914Gate] = 20,
        [DoorType.Scp096] = 20,
        [DoorType.CheckpointGate] = 20,
        [DoorType.HczArmory] = 20,
        [DoorType.HID] = 50,
        [DoorType.LczArmory] = 20,
    };

    /// <summary>
    /// Gets or sets how much XP should be gained when spawning as a certain class.
    /// </summary>
    /// <remarks><see cref="RoleTypeId.None"/> should be used as the default value.</remarks>
    [Description("How Much XP Should be Gained when Spawning as a Class? Use roletype none as the Default value")]
    public Dictionary<RoleTypeId, int> SpawnXP { get; set; } = new()
    {
        [RoleTypeId.None] = 0,
        [RoleTypeId.ClassD] = 10,
    };

    /// <summary>
    /// Gets or sets a value indicating whether XP gain should be shown when killing players.
    /// </summary>
    [Description("Show XP gained from Kills? Turn this off if you want the aspect of killing players to remain a mystery")]
    public bool ShowKillXPGain { get; set; } = true;

    /// <summary>
    /// Gets or sets how much XP should be gained when certain classes kill other classes.
    /// </summary>
    [Description("How Much XP Should be Gained When Class 1 Kills Class 2")]
    public Dictionary<RoleTypeId, Dictionary<RoleTypeId, int>> KillXP { get; set; } = new()
    {
        [RoleTypeId.None] = new()
        {
            [RoleTypeId.None] = 0,
        },
        [RoleTypeId.ClassD] = new()
        {
            [RoleTypeId.Scientist] = 50,
            [RoleTypeId.FacilityGuard] = 150,
            [RoleTypeId.NtfPrivate] = 200,
            [RoleTypeId.NtfSergeant] = 250,
            [RoleTypeId.NtfCaptain] = 300,
            [RoleTypeId.Scp049] = 500,
            [RoleTypeId.Scp0492] = 100,
            [RoleTypeId.Scp106] = 500,
            [RoleTypeId.Scp173] = 500,
            [RoleTypeId.Scp096] = 500,
            [RoleTypeId.Scp939] = 500,
        },
        [RoleTypeId.Scientist] = new()
        {
            [RoleTypeId.ClassD] = 50,
            [RoleTypeId.ChaosConscript] = 200,
            [RoleTypeId.ChaosRifleman] = 200,
            [RoleTypeId.ChaosRepressor] = 250,
            [RoleTypeId.ChaosMarauder] = 300,
            [RoleTypeId.Scp049] = 500,
            [RoleTypeId.Scp0492] = 100,
            [RoleTypeId.Scp106] = 500,
            [RoleTypeId.Scp173] = 500,
            [RoleTypeId.Scp096] = 500,
            [RoleTypeId.Scp939] = 500,
        },
    };

    /// <summary>
    /// Gets or sets how much XP should be gained when escaping.
    /// </summary>
    [Description("How much XP should be Granted when Escaping")]
    public Dictionary<RoleTypeId, int> EscapeXP { get; set; } = new()
    {
        [RoleTypeId.ClassD] = 500,
        [RoleTypeId.Scientist] = 300,
    };

    /// <summary>
    /// Gets or sets how much XP should be gained when escaping the pocket dimension.
    /// </summary>
    /// <remarks><see cref="RoleTypeId.None"/> should be used as the default value.</remarks>
    [Description("How much XP should be Granted when Escaping the Pocket Dimension. Use RoleTypeId None as a fallback")]
    public Dictionary<RoleTypeId, int> EscapePocketDimensionXP { get; set; } = new()
    {
        [RoleTypeId.None] = 50,
        [RoleTypeId.ClassD] = 75,
        [RoleTypeId.Scientist] = 65,
    };

    /// <summary>
    /// Gets or sets how much XP should be gained when reviving a player as SCP-049.
    /// </summary>
    [Description("How much XP should be gained when reviving a player as SCP-049?")]
    public int DoctorRevivePlayerXP { get; set; } = 25;

    /// <summary>
    /// Gets or sets how much XP should be gained when a player survives the whole round as a certain class.
    /// </summary>
    /// <remarks><see cref="RoleTypeId.None"/> should be used as the default value.</remarks>
    [Description("How Much XP Should be Gained When a Player Survives the whole round as a certain Class. Use RoleTypeId None as a fallback")]
    public Dictionary<RoleTypeId, int> SurvivedRoundXP { get; set; } = new()
    {
        [RoleTypeId.None] = 75,
        [RoleTypeId.ClassD] = 125,
        [RoleTypeId.Scientist] = 115,
    };

    /// <summary>
    /// Gets or sets how much XP should be rewarded to the winning team players who are alive.
    /// </summary>
    [Description("How Much XP Should be rewarded to the Winning Team's Players who are Alive?")]
    public Dictionary<LeadingTeam, int> WinningTeamXP { get; set; } = new()
    {
        [LeadingTeam.Anomalies] = 75,
        [LeadingTeam.FacilityForces] = 100,
        [LeadingTeam.ChaosInsurgency] = 100,
    };

    /// <summary>
    /// Gets or sets how much XP should be gained when upgrading an item of a certain category using SCP-914.
    /// </summary>
    [Description("How much XP should be gained when a Player upgrades an item of each category in SCP-914")]
    public Dictionary<ItemCategory, int> UpgradeXP { get; set; } = new()
    {
        [ItemCategory.Ammo] = 10,
        [ItemCategory.Armor] = 10,
        [ItemCategory.Firearm] = 10,
        [ItemCategory.Grenade] = 10,
        [ItemCategory.Keycard] = 10,
        [ItemCategory.Medical] = 10,
        [ItemCategory.Radio] = 10,
        [ItemCategory.MicroHID] = 10,
        [ItemCategory.SCPItem] = 10,
    };
}
