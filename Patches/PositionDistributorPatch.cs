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
    private static readonly List<VisibilityCheck> Handlers = new(16);

    /// <summary>
    /// The delegate used for checking visibility of players.
    /// </summary>
    /// <param name="data">The data of the visibility check.</param>
    public delegate void VisibilityCheck(ref VisibilityData data);

    /// <summary>
    /// Event called when checking the visibility for players.
    /// </summary>
    public static event VisibilityCheck CheckingVisibility
    {
        add => Handlers.Add(value);
        remove => Handlers.Remove(value);
    }

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

        for (int z = 0; z < newInstructions.Count; z++)
        {
            yield return newInstructions[z];
        }

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }

    private static void CallSafely(ref bool invisible, ReferenceHub receiver, ReferenceHub player)
    {
        try
        {
            VisibilityData data = new(invisible, receiver, player);

            for (int i = 0; i < Handlers.Count; i++)
            {
                try
                {
                    Handlers[i].Invoke(ref data);
                }
                catch (Exception e)
                {
                    BPLogger.Error($"Failed to invoke event: {e}");
                }
            }

            invisible = data.Invisible;
        }
        catch (Exception e)
        {
            BPLogger.Error(e.ToString());
        }
    }

    /// <summary>
    /// Struct responsible for holding visiblity check data.
    /// </summary>
    public struct VisibilityData
    {
        /// <summary>
        /// Value indicating whether the player should be invisible to the receiver.
        /// </summary>
        public bool Invisible;

        /// <summary>
        /// The current event change priority.
        /// </summary>
        public EventChangePriority Priority;

        /// <summary>
        /// The receiver of the visibility information.
        /// </summary>
        public ReferenceHub Receiver;

        /// <summary>
        /// The player being viewed by the receiver.
        /// </summary>
        public ReferenceHub Player;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityData"/> struct.
        /// </summary>
        /// <param name="invisible">Indicates whether the player should be invisible to the receiver.</param>
        /// <param name="priority">The event change priority.</param>
        /// <param name="receiver">The receiver of the visibility information.</param>
        /// <param name="player">The player being viewed by the receiver.</param>
        public VisibilityData(bool invisible, ReferenceHub receiver, ReferenceHub player)
        {
            this.Invisible = invisible;
            this.Priority = EventChangePriority.None;
            this.Receiver = receiver;
            this.Player = player;
        }
    }
}