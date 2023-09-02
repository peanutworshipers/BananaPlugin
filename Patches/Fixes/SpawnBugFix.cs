namespace BananaPlugin.Patches.Fixes;

using BananaPlugin.API.Utils;
using HarmonyLib;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.Spawnpoints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using static HarmonyLib.AccessTools;

/// <summary>
/// A patch that fixes players not spawning where they are supposed to.
/// </summary>
[HarmonyPatch]
public static class SpawnBugFix
{
    private static MethodInfo TargetMethod()
    {
        Type[] types = typeof(RoleSpawnpointManager).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance);
        IEnumerable<Type> eventParams = typeof(PlayerRoleManager.RoleChanged).GetMethod("Invoke").GetParameters().Select(x => x.ParameterType);

        for (int i = 0; i < types.Length; i++)
        {
            Type type = types[i];

            if (type.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) is null)
            {
                continue;
            }

            MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);

            for (int j = 0; j < methods.Length; j++)
            {
                MethodInfo method = methods[j];

                IEnumerable<Type> methodParams = method.GetParameters().Select(x => x.ParameterType);

                if (!methodParams.SequenceEqual(eventParams))
                {
                    continue;
                }

                return method;
            }
        }

        throw new Exception("Could not find method.");
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

#pragma warning disable SA1118 // Parameter should not span multiple lines
        newInstructions.InsertRange(newInstructions.Count - 1, new CodeInstruction[]
        {
            // fpcRole.FpcModule.ServerOverridePosition(position, Vector3.up * horizontalRot);
            new(OpCodes.Ldloc_0),
            new(OpCodes.Callvirt, PropertyGetter(typeof(IFpcRole), nameof(IFpcRole.FpcModule))),
            new(OpCodes.Ldloc_1),
            new(OpCodes.Call, PropertyGetter(typeof(Vector3), nameof(Vector3.up))),
            new(OpCodes.Ldloc_2),
            new(OpCodes.Call, Method(typeof(Vector3), "op_Multiply", new Type[] { typeof(Vector3), typeof(float) })),
            new(OpCodes.Call, Method(typeof(FirstPersonMovementModule), nameof(FirstPersonMovementModule.ServerOverridePosition))),
        });
#pragma warning restore SA1118 // Parameter should not span multiple lines

        return newInstructions.FinishTranspiler();
    }
}
