namespace BananaPlugin.Patches.Fixes;

using BananaPlugin.API.Utils;
using HarmonyLib;
using SCP294.Commands;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

#pragma warning disable SA1118 // Parameter should not span multiple lines

/// <summary>
/// This patch allows obtaining custom cola feature drinks from SCP-294.
/// </summary>
[HarmonyPatch]
public static class Scp294RedBullPatch
{
    private const string NestedTypeName = "<>c__DisplayClass10_4";
    private const string MethodName = "<Execute>b__5";

    private static Type? nestedType;

    [HarmonyPrepare]
    private static bool Init()
    {
        return DependencyChecker.CheckDependencies(DependencyChecker.Dependency.SCP294);
    }

    private static MethodInfo TargetMethod()
    {
        nestedType = typeof(SCP294Command).GetNestedType(NestedTypeName, all);

        return nestedType.GetMethod(MethodName, all);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

        newInstructions.InsertRange(newInstructions.Count - 1, new CodeInstruction[]
        {
            // Scp294RedBullPatch.OnReceivedCola(item, this.drinkName);
            new(OpCodes.Ldloc_0),
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldfld, Field(nestedType!, "drinkName")),
            new(OpCodes.Call, Method(typeof(Scp294RedBullPatch), nameof(OnReceivedCola))),
        });

        return newInstructions.FinishTranspiler();
    }

    private static void OnReceivedCola(ExItem item, string drinkName)
    {
        if (!Features.CustomCola.Instance || drinkName.ToLower() != "red bull")
        {
            return;
        }

        Scp294Plugin.Instance.CustomDrinkItems.Remove(item.Serial);
        Features.CustomCola.Instance.ChangeColaType(item.Serial, Features.CustomCola.ColaType.RedBull);
    }
}
