namespace BananaPlugin.Patches.Fixes;

using HarmonyLib;
using Mirror;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

/// <summary>
/// The main patch responsible for fixing a disconnect issue when overriding damage handler classes.
/// </summary>
[HarmonyPatch(typeof(DamageHandlerReaderWriter), nameof(DamageHandlerReaderWriter.WriteDamageHandler))]
public static class DamageHandlerWriterPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        yield return new(OpCodes.Ldarg_0);
        yield return new(OpCodes.Ldarg_1);
        yield return new(OpCodes.Call, Method(typeof(DamageHandlerWriterPatch), nameof(WriteDamageHandler)));
        yield return new(OpCodes.Ret);
    }

    private static void WriteDamageHandler(this NetworkWriter writer, DamageHandlerBase info)
    {
        Type type = info.GetType();

        if (DamageHandlers.IdsByTypeHash.TryGetValue(type.FullName.GetStableHashCode(), out byte hash))
        {
            writer.WriteByte(hash);
            info.WriteAdditionalData(writer);
            return;
        }

        while ((type = type.BaseType) != typeof(DamageHandlerBase))
        {
            if (DamageHandlers.IdsByTypeHash.TryGetValue(type.FullName.GetStableHashCode(), out hash))
            {
                writer.WriteByte(hash);
                info.WriteAdditionalData(writer);
                return;
            }

            type = type.BaseType;
        }

        CustomReasonDamageHandler custom = new CustomReasonDamageHandler("[ERROR] Server failed to acquire damage handler hashcode.");

        if (DamageHandlers.IdsByTypeHash.TryGetValue(typeof(CustomReasonDamageHandler).FullName.GetStableHashCode(), out hash))
        {
            writer.WriteByte(hash);
            custom.WriteAdditionalData(writer);
        }
    }
}
