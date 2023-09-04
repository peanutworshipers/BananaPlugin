namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using PlayerRoles.Spectating;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

/// <summary>
/// The main feature for handling the replacement of disconnecting players.
/// </summary>
[Obsolete]
public sealed class DisconnectReplace : BananaFeature
{
    private DisconnectReplace()
    {
    }

    /// <inheritdoc/>
    public override string Name => "Disconnect Replace";

    /// <inheritdoc/>
    public override string Prefix => "dcr";

    /// <inheritdoc/>
    protected override void Enable()
    {
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
    }

    private static bool FilterHubs(ReferenceHub toSelect)
    {
        return toSelect.roleManager.CurrentRole is SpectatorRole;
    }

    private static float GetDeathTime(ReferenceHub hub)
    {
        return hub.roleManager.CurrentRole is SpectatorRole spec
            ? spec.ActiveTime
            : 0f;
    }

    private void Left(LeftEventArgs ev)
    {
        // Add proper role checking here.
        // Ex: tutorials should not be replaced.
        //
        // However, custom roles should specify
        // if they can be replaced.

#warning CustomRoles look here.

        if (ev.Player.ReferenceHub.roleManager.CurrentRole.RoleTypeId == PlayerRoles.RoleTypeId.Tutorial)
        {
            return;
        }

        ev.Player.ReferenceHub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown));
        return;

        ReferenceHub left = ev.Player.ReferenceHub;
        ReferenceHub? available =
            PlayerListUtils.VerifiedHubs
            .OrderByDescending(GetDeathTime)
            .FirstOrDefault(FilterHubs);

        // Nobody available to replace
        // the disconnected player.
        if (available is null)
        {
            left.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown));
            return;
        }

        // Apply replacement here.
    }

    [HarmonyPatch(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnServerDisconnect))]
    private static class RemoveDisonnectDropPatch
    {
        // This patch removes default disconnectDrop implementation.
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            FieldInfo disconnectDropField = Field(typeof(CustomNetworkManager), nameof(CustomNetworkManager._disconnectDrop));

            int index = newInstructions.FindIndex(x => x.LoadsField(disconnectDropField));
            Label notDisconnectDropLabel = (Label)newInstructions[index + 1].operand;
            int labelIndex = newInstructions.FindIndex(x => x.labels.Contains(notDisconnectDropLabel));
            newInstructions.RemoveRange(index - 1, labelIndex - index + 1);

            return newInstructions.FinishTranspiler();
        }
    }
}
