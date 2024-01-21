namespace BananaPlugin.API.Main;

using Extensions;
using RemoteAdmin;
using System.Linq;

/// <inheritdoc/>
internal sealed class DeveloperCommandSender : PlayerCommandSender
{
    private bool developerModeActive;
    private ExPlayer player;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeveloperCommandSender"/> class.
    /// </summary>
    /// <param name="original">The original player command sender.</param>
    public DeveloperCommandSender(PlayerCommandSender original)
        : base(original.ReferenceHub)
    {
        this.OriginalSender = original;
        this.player = ExPlayer.Get(original.ReferenceHub);
    }

    /// <summary>
    /// Gets full permissions with permissions not allowed for developers.
    /// </summary>
    public static ulong RevokedPermissions
    {
        get
        {
            PlayerPermissions toRevoke =
                  PlayerPermissions.KickingAndShortTermBanning
                | PlayerPermissions.BanningUpToDay
                | PlayerPermissions.LongTermBanning
                | PlayerPermissions.ViewHiddenGlobalBadges
                | PlayerPermissions.PlayerSensitiveDataAccess
                | PlayerPermissions.ServerConsoleCommands
                | PlayerPermissions.SetGroup
                | PlayerPermissions.PermissionsManagement
                | PlayerPermissions.ServerConfigs;

            return ServerStatic.PermissionsHandler.FullPerm & ~(ulong)toRevoke;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this player has developer mode active.
    /// </summary>
    public bool DeveloperModeActive
    {
        get => this.developerModeActive;
        set
        {
            if (value == this.developerModeActive)
            {
                return;
            }

            this.ApplyDeveloperMode(value);
        }
    }

    /// <inheritdoc/>
    public override ulong Permissions => this.developerModeActive ? RevokedPermissions : this.player.Group.Permissions;

    /// <summary>
    /// Gets the original player command sender.
    /// </summary>
    internal PlayerCommandSender OriginalSender { get; }

    private static UserGroup DeveloperModeGroup { get; } = new UserGroup()
    {
        BadgeColor = "brown",
        BadgeText = "ploogin dev (DEVELOPER MODE)",
        Permissions = RevokedPermissions,
        Cover = true,
        HiddenByDefault = false,
        Shared = false,
        KickPower = 0,
        RequiredKickPower = 1,
    };

    private void ApplyDeveloperMode(bool value)
    {
        ServerStatic.GetPermissionsHandler()._groups["DEV_OVERRIDE"] = DeveloperModeGroup;

        if (value)
        {
            this.player.Group = DeveloperModeGroup;
        }
        else
        {
            this.ServerRoles.SetGroup(null, ovr: false);
            this.ServerRoles.RefreshPermissions();
        }

        string message = $"@[SYSTEM BROADCAST]\n<size=35>Player ({this.PlayerId}) {this.Nickname} has {(value ? "enabled" : "disabled")} Developer Mode.</size>";

        this.RaReply(message, true, false, string.Empty);

        foreach (ExPlayer ply in ExPlayer.List.Where(x => x.Sender.CheckPermission(BRank.SeniorModerator, out _)))
        {
            if (ply.ReferenceHub == this.ReferenceHub)
            {
                continue;
            }

            ply.Sender.RaReply(message, true, false, string.Empty);
        }

        this.developerModeActive = value;
    }
}
