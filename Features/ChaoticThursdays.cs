namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

#pragma warning disable SA1118 // Parameter should not span multiple lines

/// <summary>
/// The main feature responsible for chaotic thursdays.
/// </summary>
public sealed class ChaoticThursday : BananaFeature
{
    private ChaoticThursday()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the chaotic thursday instance.
    /// </summary>
    public static ChaoticThursday? Instance { get; private set; }

    /// <summary>
    /// Gets a value indicating whether chaotic thursday is active.
    /// </summary>
    public static bool IsActive { get; private set; } = false;

    /// <inheritdoc/>
    public override string Name => "Chaotic Thursdays";

    /// <inheritdoc/>
    public override string Prefix => "ct";

    /// <inheritdoc/>
    protected override void Enable()
    {
        ExHandlers.Server.WaitingForPlayers += this.WaitingForPlayers;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ExHandlers.Server.WaitingForPlayers -= this.WaitingForPlayers;

        IsActive = false;
    }

    private void WaitingForPlayers()
    {
        // We only apply chaotic thursday
        // once a round is ready, so it
        // doesn't enable mid-round.
        IsActive = Versioning.LocalBuild || System.DayOfWeek.Thursday.IsDayOfWeek();
    }

    [HarmonyPatch]
    private static class PushForcePatch
    {
        [HarmonyPrepare]
        private static bool Init()
        {
            return TypeByName("Push.Config") is not null;
        }

        [HarmonyPatch("Push.Config", nameof(Push.Config.PushForce), MethodType.Getter)]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            int index = newInstructions.FindLastIndex(x => x.opcode == OpCodes.Ret);

            Label skipLabel = generator.DefineLabel();

            newInstructions[index].labels.Add(skipLabel);

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                // if (ChaoticThursday.IsActive)
                // {
                //     __result *= 3f;
                // }
                new(OpCodes.Call, PropertyGetter(typeof(ChaoticThursday), nameof(IsActive))),
                new(OpCodes.Brfalse_S, skipLabel),
                new(OpCodes.Ldc_R4, 3f),
                new(OpCodes.Mul),
            });

            return newInstructions.FinishTranspiler();
        }
    }
}
