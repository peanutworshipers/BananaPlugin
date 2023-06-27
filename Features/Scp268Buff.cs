namespace BananaPlugin.Features;

using BananaPlugin.API;
using BananaPlugin.API.Main;
using BananaPlugin.Patches;
using CustomPlayerEffects;
using HarmonyLib;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.FirstPersonControl.Thirdperson;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp939.Ripples;
using System.Collections.Generic;
using System.Reflection.Emit;

/// <summary>
/// The main feature responsible for buffing the SCP-268 item.
/// </summary>
public sealed class Scp268Buff : BananaFeature
{
    /// <inheritdoc/>
    public override string Name => "SCP-268 Buff";

    /// <inheritdoc/>
    public override string Prefix => "268";

    /// <inheritdoc/>
    protected override void Enable()
    {
        PositionDistributorPatch.CheckingVisibility += this.CheckingVisibility;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        PositionDistributorPatch.CheckingVisibility -= this.CheckingVisibility;
    }

    private void CheckingVisibility(ref bool invisible, ref EventChangePriority priority, ReferenceHub receiver, ReferenceHub player)
    {
        if (priority >= EventChangePriority.Feature)
        {
            return;
        }

        if (receiver.roleManager.CurrentRole.RoleTypeId != PlayerRoles.RoleTypeId.Scp079)
        {
            return;
        }

        if (!player.playerEffectsController.GetEffect<Invisible>().IsEnabled)
        {
            return;
        }

        priority = EventChangePriority.Feature;
        invisible = true;
    }

    [HarmonyPatch(typeof(FootstepRippleTrigger), nameof(FootstepRippleTrigger.OnFootstepPlayed))]
    private static class FootstepRipplePatch
    {
#warning convert to transpiler when you feel like it kekw
        private static bool Prefix(AnimatedCharacterModel model)
        {
            return !Features["268"].Enabled || !model.OwnerHub.playerEffectsController.GetEffect<Invisible>().IsEnabled;
        }
    }
}
