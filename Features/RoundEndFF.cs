namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using ExFeatures;
using Exiled.Events.EventArgs.Server;
using GameCore;

/// <summary>
/// The feature responsible for enabling friendly fire when the round ends.
/// </summary>
public sealed class RoundEndFF : BananaFeature
{
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
        ServerHandlers.WaitingForPlayers += this.ResetFFConfig;
        ServerHandlers.RoundEnded += this.EnableFF;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ServerHandlers.WaitingForPlayers -= this.ResetFFConfig;
        ServerHandlers.RoundEnded -= this.EnableFF;
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
