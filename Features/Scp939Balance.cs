namespace BananaPlugin.Features;

using BananaPlugin.API;
using BananaPlugin.API.Main;
using BananaPlugin.Extensions;
using CustomPlayerEffects;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using System.Collections.Generic;
using static BananaPlugin.Patches.PositionDistributorPatch;

/// <summary>
/// The main feature responsible for balancing SCP-939.
/// </summary>
public class Scp939Balance : BananaFeature
{
    /// <summary>
    /// The maximum amount of players SCP-939 is allowed to hit within one frame.
    /// </summary>
    public const int Max939HitCount = 3;

    private CoroutineHandle endOfFrameHandle;

    private Scp939Balance()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the Scp939Balance instance.
    /// </summary>
    public static Scp939Balance? Instance { get; private set; }

    /// <inheritdoc/>
    public override string Name => "SCP-939 Balance";

    /// <inheritdoc/>
    public override string Prefix => "939";

    private Dictionary<int, int> Scp939ClawedCount { get; } = new();

    private HashSet<ReferenceHub> BeingFlashed { get; } = new();

    /// <inheritdoc/>
    protected override void Enable()
    {
        ExHandlers.Player.Hurting += this.Hurting;
        ExHandlers.Map.ExplodingGrenade += this.ExplodingGrenade;
        StatusEffectBase.OnIntensityChanged += this.EffectIntensityChanged;
        CheckingVisibility += this.CheckingVisibilityDeafened;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ExHandlers.Player.Hurting -= this.Hurting;
        ExHandlers.Map.ExplodingGrenade -= this.ExplodingGrenade;
        StatusEffectBase.OnIntensityChanged -= this.EffectIntensityChanged;
        CheckingVisibility -= this.CheckingVisibilityDeafened;
    }

    private void CheckingVisibilityDeafened(ref VisibilityData data)
    {
        if (data.Invisible)
        {
            return;
        }

        if (data.Priority >= EventChangePriority.Feature)
        {
            return;
        }

        if (data.Receiver.GetRoleId() != RoleTypeId.Scp939)
        {
            return;
        }

        if (!data.Receiver.playerEffectsController.GetEffect<Deafened>().IsEnabled)
        {
            return;
        }

        data.Priority = EventChangePriority.Feature;
        data.Invisible = true;
    }

    private void EnsureEndOfFrameCoroutine()
    {
        if (!this.endOfFrameHandle.IsRunning)
        {
            this.endOfFrameHandle.AssignNew(this.OnEndOfFrame, Segment.EndOfFrame);
        }
    }

    private void ExplodingGrenade(ExplodingGrenadeEventArgs ev)
    {
        if (ev.Projectile.Type != ItemType.GrenadeFlash)
        {
            return;
        }

        this.EnsureEndOfFrameCoroutine();

        foreach (ExPlayer player in ev.TargetsToAffect)
        {
            this.BeingFlashed.Add(player.ReferenceHub);
        }
    }

    private void EffectIntensityChanged(StatusEffectBase effect, byte oldVal, byte newVal)
    {
        if (effect is not Deafened deafened)
        {
            return;
        }

        if (newVal == 0)
        {
            return;
        }

        if (!this.BeingFlashed.Contains(effect.Hub))
        {
            return;
        }

        if (effect.Hub.GetRoleId() != RoleTypeId.Scp939)
        {
            return;
        }

        effect.ServerChangeDuration(3f, true);
    }

    private void Hurting(HurtingEventArgs ev)
    {
        if (ev.DamageHandler.Type != ExEnums.DamageType.Scp939 || ev.Attacker is null)
        {
            return;
        }

        this.EnsureEndOfFrameCoroutine();

        this.IncrementAttackCounter(ev.Attacker.ReferenceHub);

        if (this.CheckAttackCount(ev.Attacker.ReferenceHub) > Max939HitCount)
        {
            ev.IsAllowed = false;
        }
    }

    private int CheckAttackCount(ReferenceHub scp939)
#pragma warning disable IDE0046 // Convert to conditional expression (for readability)
    {
        if (scp939.roleManager.CurrentRole.RoleTypeId != PlayerRoles.RoleTypeId.Scp939)
        {
            return 0;
        }

        if (!this.Scp939ClawedCount.TryGetValue(scp939.PlayerId, out int count))
        {
            return 0;
        }

        return count;
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    private void IncrementAttackCounter(ReferenceHub scp939)
    {
        if (scp939.roleManager.CurrentRole.RoleTypeId != PlayerRoles.RoleTypeId.Scp939)
        {
            return;
        }

        if (!this.Scp939ClawedCount.TryGetValue(scp939.PlayerId, out int count))
        {
            this.Scp939ClawedCount[scp939.PlayerId] = 1;
            return;
        }

        this.Scp939ClawedCount[scp939.PlayerId] = ++count;
    }

    private IEnumerator<float> OnEndOfFrame()
    {
        while (true)
        {
            yield return Timing.WaitForOneFrame;

            this.Scp939ClawedCount.Clear();
            this.BeingFlashed.Clear();
        }
    }
}
