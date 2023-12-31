﻿namespace BananaPlugin.Features;

using BananaPlugin.API;
using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.Patches;
using CustomPlayerEffects;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp173;
using System.Collections.Generic;
using System.Reflection.Emit;
using static BananaPlugin.Patches.PositionDistributorPatch;
using static HarmonyLib.AccessTools;

/// <summary>
/// The main feature responsible for buffing the SCP-268 item.
/// </summary>
public sealed class Scp268Buff : BananaFeature
{
    private Scp268Buff()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the Scp268Buff instance.
    /// </summary>
    public static Scp268Buff? Instance { get; private set; }

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

    private void CheckingVisibility(ref VisibilityData data)
    {
        if (data.Invisible)
        {
            return;
        }

        if (data.Priority >= EventChangePriority.Feature)
        {
            return;
        }

        if (data.Receiver.roleManager.CurrentRole.RoleTypeId != PlayerRoles.RoleTypeId.Scp079)
        {
            return;
        }

        if (!data.Player.playerEffectsController.GetEffect<Invisible>().IsEnabled)
        {
            return;
        }

        data.Priority = EventChangePriority.Feature;
        data.Invisible = true;
    }

    [HarmonyPatch(typeof(Scp173ObserversTracker), nameof(Scp173ObserversTracker.IsObservedBy))]
    private static class Scp173ObserverPatch
    {
#pragma warning disable SA1118 // Parameter should not span multiple lines
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            Label skipLabel = generator.DefineLabel();
            Label effectNotNullLabel = generator.DefineLabel();

            newInstructions[0].WithLabels(skipLabel);

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                // if (Scp268Buff.Instance is null || !Scp268Buff.Instance.Enabled)
                // {
                //     goto SKIP;
                // }
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(Scp268Buff), nameof(Instance))),
                new(OpCodes.Brfalse_S, skipLabel),
                new(OpCodes.Call, PropertyGetter(typeof(Scp268Buff), nameof(Instance))),
                new(OpCodes.Call, PropertyGetter(typeof(BananaFeature), nameof(Enabled))),
                new(OpCodes.Brfalse_S, skipLabel),

                // if (target.playerEffectsController.GetEffect<Invisible>()?.IsEnabled)
                // {
                //     return false;
                // }
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldfld, Field(typeof(ReferenceHub), nameof(ReferenceHub.playerEffectsController))),
                new(OpCodes.Call, Method(typeof(PlayerEffectsController), nameof(PlayerEffectsController.GetEffect)).MakeGenericMethod(typeof(Invisible))),
                new(OpCodes.Dup),
                new(OpCodes.Brtrue_S, effectNotNullLabel),
                new(OpCodes.Pop),
                new(OpCodes.Br_S, skipLabel),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(StatusEffectBase), nameof(StatusEffectBase.IsEnabled))).WithLabels(effectNotNullLabel),
                new(OpCodes.Brfalse_S, skipLabel),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ret),
            });

            return newInstructions.FinishTranspiler();
        }
#pragma warning restore SA1118 // Parameter should not span multiple lines
    }
}
