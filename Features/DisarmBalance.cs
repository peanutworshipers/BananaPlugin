namespace BananaPlugin.Features;

using BananaPlugin.API.Attributes;
using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using HarmonyLib;
using InventorySystem.Disarming;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;

/// <summary>
/// The main feature responsible for balancing disarming.
/// </summary>
[AllowedPorts(ServerPorts.TestServer)]
public sealed class DisarmBalance : BananaFeature
{
    /// <summary>
    /// The distance at which players are uncuffed inside the facility.
    /// </summary>
    public const float FacilityUncuffDistance = 50f;

    /// <summary>
    /// The distance at which players are uncuffed on surface.
    /// </summary>
    public const float SurfaceUncuffDistance = 70f;

    private DisarmBalance()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the Disarm Balance instance.
    /// </summary>
    public static DisarmBalance? Instance { get; private set; }

    /// <inheritdoc/>
    public override string Name => "Disarm Balance";

    /// <inheritdoc/>
    public override string Prefix => "disarm";

    /// <inheritdoc/>
    protected override void Enable()
    {
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
    }

    [HarmonyPatch(typeof(DisarmedPlayers), nameof(DisarmedPlayers.ValidateEntry))]
    private static class DisarmValidateEntryPatch
    {
        private const float SqrFacilityDist = FacilityUncuffDistance * FacilityUncuffDistance;

        private const float SqrSurfaceDist = SurfaceUncuffDistance * SurfaceUncuffDistance;

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
#pragma warning disable SA1118 // Parameter should not span multiple lines
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            Label originalLabel = generator.DefineLabel();
            Label skipLabel = generator.DefineLabel();
            Label surfaceLabel = generator.DefineLabel();

            MethodInfo getSqrMagnitude = PropertyGetter(typeof(Vector3), nameof(Vector3.sqrMagnitude));

            int index = newInstructions.FindLastIndex(x => x.Calls(getSqrMagnitude)) + 1;
            int skipIndex = newInstructions.FindIndex(index, x => x.opcode == OpCodes.Ret) + 1;

            newInstructions[index].labels.Add(originalLabel);
            newInstructions[skipIndex].labels.Add(skipLabel);

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                // if (DisarmBalance.Instance is null || !DisarmBalance.Instance.Enabled)
                // {
                //     goto originalLabel;
                // }
                new(OpCodes.Call, PropertyGetter(typeof(DisarmBalance), nameof(Instance))),
                new(OpCodes.Brfalse_S, originalLabel),
                new(OpCodes.Call, PropertyGetter(typeof(DisarmBalance), nameof(Instance))),
                new(OpCodes.Call, PropertyGetter(typeof(BananaFeature), nameof(Enabled))),
                new(OpCodes.Brfalse_S, originalLabel),

                // float [onstack] sqrDist;
                //
                // if ((disarmerPos.y > 500 && sqrDist <= SqrSurfaceDist)
                //     || sqrDist <= SqrFacilityDist)
                // {
                //     return false;
                // }
                //
                // goto skipLabel;
                new(OpCodes.Ldloca_S, 2), // [2] Vector3 disarmerPos
                new(OpCodes.Ldfld, Field(typeof(Vector3), nameof(Vector3.y))),
                new(OpCodes.Ldc_R4, 500f),
                new(OpCodes.Bge_Un_S, surfaceLabel),

                new(OpCodes.Ldc_R4, SqrFacilityDist),
                new(OpCodes.Ble_Un_S, skipLabel),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ret),

                new CodeInstruction(OpCodes.Ldc_R4, SqrSurfaceDist)
                    .WithLabels(surfaceLabel),
                new(OpCodes.Ble_Un_S, skipLabel),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ret),
            });

            return newInstructions.FinishTranspiler();
#pragma warning restore SA1118 // Parameter should not span multiple lines
        }
    }
}
