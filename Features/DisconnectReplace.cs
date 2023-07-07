namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using HarmonyLib;
using NorthwoodLib.Pools;
using System.Collections.Generic;
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

    // [HarmonyPatch(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnServerDisconnect))]
    private static class RemoveDisonnectDropPatch
    {
        // This patch removes default disconnectDrop implementation.
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            BPLogger.IdentifyMethodAs(nameof(RemoveDisonnectDropPatch), nameof(Transpiler));

            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            FieldInfo disconnectDropField = Field(typeof(CustomNetworkManager), nameof(CustomNetworkManager._disconnectDrop));

            int index = newInstructions.FindIndex(x => x.LoadsField(disconnectDropField));
            Label notDisconnectDropLabel = (Label)newInstructions[index + 1].operand;
            int labelIndex = newInstructions.FindIndex(x => x.labels.Contains(notDisconnectDropLabel));
            newInstructions.RemoveRange(index - 1, labelIndex - index + 1);

            BPLogger.Debug($"Removed {index - 1} - {labelIndex}");

            for (int z = 0; z < newInstructions.Count; z++)
            {
                yield return newInstructions[z];
            }

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}
