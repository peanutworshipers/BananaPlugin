namespace BananaPlugin.Patches;

using BananaPlugin.API;
using BananaPlugin.API.Utils;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

#pragma warning disable SA1118 // Parameter should not span multiple lines
#pragma warning disable SA1120 // Comments should contain text
/// <summary>
/// The main patch for overriding visibility of players.
/// </summary>
[HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.WriteAll))]
public static class PositionDistributorPatch
{
    /// <summary>
    /// The delegate used for checking visibility of players.
    /// </summary>
    /// <param name="invisible">Indicates whether the player should be considered invisible.</param>
    /// <param name="priority">The current change priority.</param>
    /// <param name="receiver">The player that is receiving the position information.</param>
    /// <param name="player">The player whose position is being sent to the receiver.</param>
    public delegate void VisibilityCheck(ref bool invisible, ref EventChangePriority priority, ReferenceHub receiver, ReferenceHub player);

    /// <summary>
    /// Event called when checking the visibility for players.
    /// </summary>
    public static event VisibilityCheck? CheckingVisibility;

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

        // FpcSyncData newSyncData = GetNewSyncData(receiver, allHub, fpcRole.FpcModule, flag2);
        //
        // <------- Patch goes here.
        //
        // if (!invisible)
        // {
        //     // apply data
        // }
        int index = newInstructions.FindLastIndex(x => x.opcode == OpCodes.Ldloc_S && x.operand is LocalBuilder local && local.LocalIndex == 7);

        newInstructions.InsertRange(index, new CodeInstruction[]
        {
            //
            // PositionDistributorPatch.CallSafely(ref bool invisible, receiver, player);
            //
            new(OpCodes.Ldloca_S, 7), // ref bool invisible
            new(OpCodes.Ldarg_0), // ReferenceHub receiver
            new(OpCodes.Ldloc_S, 5), // ReferenceHub player
            new(OpCodes.Call, Method(typeof(PositionDistributorPatch), nameof(CallSafely))),
        });

        for (int z = 0; z < newInstructions.Count;  z++)
        {
            yield return newInstructions[z];
        }

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }

    private static void CallSafely(ref bool invisible, ReferenceHub receiver, ReferenceHub player)
    {
        try
        {
            Delegate[] delegates = CheckingVisibility?.GetInvocationList() ?? Array.Empty<VisibilityCheck>();

            EventChangePriority currentPriority = EventChangePriority.None;

            for (int i = 0; i < delegates.Length; i++)
            {
                try
                {
                    ((VisibilityCheck)delegates[i]).Invoke(ref invisible, ref currentPriority, receiver, player);
                }
                catch (Exception e)
                {
                    BPLogger.Error($"Failed to invoke event: {e}");
                }
            }
        }
        catch (Exception e)
        {
            BPLogger.Error(e.ToString());
        }
    }
}