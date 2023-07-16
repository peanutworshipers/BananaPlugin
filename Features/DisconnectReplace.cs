namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.Spectating;
using PlayerStatsSystem;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

/// <summary>
/// The main feature for handling the replacement of disconnecting players.
/// </summary>
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

    private static bool FilterHubs(ReferenceHub left, ReferenceHub toSelect)
    {
        return toSelect.roleManager.CurrentRole is SpectatorRole spec

            // Northwood skill issue. https://trello.com/c/Q7ziLNP3/4660-player-3rd-person-view
            && spec.SyncedSpectatedNetId != left.netId;
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
        return;

        ReferenceHub left = ev.Player.ReferenceHub;
        ReferenceHub? available =
            ReferenceHub.AllHubs
            .OrderByDescending(GetDeathTime)
            .FirstOrDefault(x => FilterHubs(left, x));

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
