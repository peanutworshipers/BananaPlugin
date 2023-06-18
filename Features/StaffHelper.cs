namespace BananaPlugin.Features;

using BananaPlugin.API;
using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Disarming;
using MEC;
using PlayerRoles;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The main feature responsible for assisting staff in their duties.
/// </summary>
public sealed class StaffHelper : BananaFeature
{
    private HashSet<int>? cuffedPlayersDying;
    private CoroutineHandle lateUpdateHandle;

    private StaffHelper()
    {
    }

    /// <inheritdoc/>
    public override string Name => "Staff Helper";

    /// <inheritdoc/>
    public override string Prefix => "staff";

    /// <inheritdoc/>
    protected override void Enable()
    {
        this.cuffedPlayersDying = new();

        ExHandlers.Server.WaitingForPlayers += this.WaitingForPlayers;
        ExHandlers.Player.Dying += this.Dying;
        ExHandlers.Player.Died += this.Died;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        this.cuffedPlayersDying = null;

        ExHandlers.Server.WaitingForPlayers -= this.WaitingForPlayers;
        ExHandlers.Player.Dying -= this.Dying;
        ExHandlers.Player.Died -= this.Died;
    }

    private void WaitingForPlayers()
    {
        this.lateUpdateHandle.KillAssignNew(this.LateUpdateHandler, Segment.LateUpdate);
    }

    private void Dying(DyingEventArgs ev)
    {
        if (ev.Player.Inventory.IsDisarmed())
        {
            this.cuffedPlayersDying!.Add(ev.Player.Id);
        }
    }

    private void Died(DiedEventArgs ev)
    {
        if (ev.Attacker is null || ev.Player == ev.Attacker)
        {
            return;
        }

        if (this.cuffedPlayersDying!.Contains(ev.Player.Id))
        {
            bool isKOS = ev.TargetOldRole switch
            {
                RoleTypeId.ClassD => true,
                RoleTypeId.Scientist => true,
                _ => false,
            };

            if (isKOS)
            {
                string message = $"<size=50>Player [{ev.Attacker?.UserId}] {ev.Attacker?.Nickname}\nkilled a cuffed {ev.TargetOldRole}!</size>";

                BRank.JuniorModerator.AdminBroadcastRankedPlayers(message);
            }
        }
    }

    private IEnumerator<float> LateUpdateHandler()
    {
        while (this.cuffedPlayersDying is not null)
        {
            this.cuffedPlayersDying.Clear();

            yield return Timing.WaitForOneFrame;
        }
    }
}
