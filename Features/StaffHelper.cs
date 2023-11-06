namespace BananaPlugin.Features;

using BananaPlugin.API;
using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.Extensions;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp330;
using InventorySystem.Disarming;
using MEC;
using PlayerRoles;
using System.Collections.Generic;

/// <summary>
/// The main feature responsible for assisting staff in their duties.
/// </summary>
public sealed class StaffHelper : PluginFeature
{
    private HashSet<int>? cuffedPlayersDying;
    private HashSet<int>? usingPinkCandy;
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
        this.usingPinkCandy = new();

        ExHandlers.Server.WaitingForPlayers += this.WaitingForPlayers;
        ExHandlers.Player.Dying += this.Dying;
        ExHandlers.Player.Died += this.Died;
        ExHandlers.Scp330.EatingScp330 += this.EatingScp330;
        ExHandlers.Player.Handcuffing += this.Handcuffing;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        this.cuffedPlayersDying = null;
        this.usingPinkCandy = null;

        ExHandlers.Server.WaitingForPlayers -= this.WaitingForPlayers;
        ExHandlers.Player.Dying -= this.Dying;
        ExHandlers.Player.Died -= this.Died;
        ExHandlers.Scp330.EatingScp330 -= this.EatingScp330;
        ExHandlers.Player.Handcuffing -= this.Handcuffing;
    }

    private void WaitingForPlayers()
    {
        this.lateUpdateHandle.KillAssignNew(this.LateUpdateHandler, Segment.LateUpdate);
    }

    private void EatingScp330(EatingScp330EventArgs ev)
    {
        if (ev.Candy.Kind == InventorySystem.Items.Usables.Scp330.CandyKindID.Pink)
        {
            this.usingPinkCandy!.Add(ev.Player.Id);
        }
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

        // Dont check SCP kills.
        if (ev.Attacker.LeadingTeam == ExEnums.LeadingTeam.Anomalies)
        {
            return;
        }

        // Dont check pink candy kills.
        if (this.usingPinkCandy!.Contains(ev.Attacker.Id))
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

    private void Handcuffing(HandcuffingEventArgs ev)
    {
        if (ev.Target.Role != RoleTypeId.Tutorial)
        {
            return;
        }

        ev.IsAllowed = false;
    }

    private IEnumerator<float> LateUpdateHandler()
    {
        while (this.Enabled)
        {
            this.cuffedPlayersDying!.Clear();
            this.usingPinkCandy!.Clear();

            yield return Timing.WaitForOneFrame;
        }
    }
}
