namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;

/// <summary>
/// The main feature responsible for chaotic thursdays.
/// </summary>
public sealed class ChaoticThursday : BananaFeature
{
    private ChaoticThursday()
    {
    }

    /// <summary>
    /// Gets a value indicating whether chaotic thursday is active.
    /// </summary>
    public static bool IsActive { get; private set; }

    /// <inheritdoc/>
    public override string Name => "Chaotic Thursdays";

    /// <inheritdoc/>
    public override string Prefix => "ct";

    /// <inheritdoc/>
    protected override void Enable()
    {
        ExHandlers.Server.WaitingForPlayers += this.WaitingForPlayers;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ExHandlers.Server.WaitingForPlayers -= this.WaitingForPlayers;
    }

    private void WaitingForPlayers()
    {
        // We only apply chaotic thursday
        // once a round is ready, so it
        // doesn't enable mid-round.
        IsActive = Versioning.LocalBuild || System.DayOfWeek.Thursday.IsDayOfWeek();
    }
}
