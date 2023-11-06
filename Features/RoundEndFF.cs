namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using ExFeatures;
using Exiled.Events.EventArgs.Server;
using GameCore;

/// <summary>
/// The feature responsible for enabling friendly fire when the round ends.
/// </summary>
public sealed class RoundEndFF : PluginFeature
{
    private RoundEndFF()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the base game friendly fire config is enabled.
    /// </summary>
    public static bool GetFFConfig => ConfigFile.ServerConfig.GetBool("friendly_fire");

    /// <inheritdoc/>
    public override string Name => "Round End FF";

    /// <inheritdoc/>
    public override string Prefix => "end_ff";

    /// <inheritdoc/>
    protected override void Enable()
    {
        ExHandlers.Server.WaitingForPlayers += this.ResetFFConfig;
        ExHandlers.Server.RoundEnded += this.EnableFF;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ExHandlers.Server.WaitingForPlayers -= this.ResetFFConfig;
        ExHandlers.Server.RoundEnded -= this.EnableFF;

        if (Round.IsEnded)
        {
            this.ResetFFConfig();
        }
    }

    private void ResetFFConfig()
    {
        Server.FriendlyFire = GetFFConfig;

        BPLogger.Debug("Round end FF was reset.");
    }

    private void EnableFF(RoundEndedEventArgs ev)
    {
        Server.FriendlyFire = true;

        BPLogger.Debug("Round end FF was enabled.");
    }
}
