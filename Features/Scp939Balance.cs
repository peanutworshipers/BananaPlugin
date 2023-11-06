namespace BananaPlugin.Features;

using BananaPlugin.API;
using BananaPlugin.API.Main;
using BananaPlugin.Extensions;
using CustomPlayerEffects;
using Exiled.Events.EventArgs.Map;
using MEC;
using PlayerRoles;
using System.Collections.Generic;
using static BananaPlugin.Patches.PositionDistributorPatch;

/// <summary>
/// The main feature responsible for balancing SCP-939.
/// </summary>
public class Scp939Balance : PluginFeature
{
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

    private HashSet<ReferenceHub> BeingFlashed { get; } = new();

    /// <inheritdoc/>
    protected override void Enable()
    {
        ExHandlers.Map.ExplodingGrenade += this.ExplodingGrenade;
        StatusEffectBase.OnIntensityChanged += this.EffectIntensityChanged;
        CheckingVisibility += this.CheckingVisibilityDeafened;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
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

    private void ExplodingGrenade(ExplodingGrenadeEventArgs ev)
    {
        if (ev.Projectile.Type != ItemType.GrenadeFlash)
        {
            return;
        }

        foreach (ExPlayer player in ev.TargetsToAffect)
        {
            this.BeingFlashed.Add(player.ReferenceHub);
        }

        MECExtensions.RunAfterFrames(0, Segment.EndOfFrame, this.BeingFlashed.Clear);
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

        MECExtensions.RunAfterFrames(0, Segment.EndOfFrame, effect.ServerChangeDuration, 8f, false);
    }
}
