namespace BananaPlugin.Patches;

using BananaPlugin.API.Utils;
using GameObjectPools;
using HarmonyLib;
using NorthwoodLib.Pools;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.SceneManagement;
using static HarmonyLib.AccessTools;

/// <summary>
/// The main patch responsible for intercepting object pool returns.
/// </summary>
[HarmonyPatch(typeof(Pool), nameof(Pool.ReturnObject))]
public static class ReturnPoolObjectPatch
{
#pragma warning disable SA1118 // Parameter should not span multiple lines
#pragma warning disable SA1120 // Comments should contain text
    static ReturnPoolObjectPatch()
    {
        SceneManager.sceneLoaded += SceneLoaded;
    }

    /// <summary>
    /// Gets a dictionary used to check whether returning of a specified pool object is disallowed.
    /// </summary>
    public static Dictionary<PoolObject, bool> DestroyOnReturn { get; } = new();

    /// <summary>
    /// Tells the patch that the specified poolable object should not be allowed to return to any pool.
    /// </summary>
    /// <param name="poolObject">The pool object to prevent pool returns for.</param>
    /// <param name="destroyOnReturn">Specifies whether the object should be destroyed on attempting to return it.</param>
    public static void PreventReturn(PoolObject poolObject, bool destroyOnReturn = true)
    {
        DestroyOnReturn[poolObject] = destroyOnReturn;
    }

    /// <summary>
    /// Tells the patch that the specified poolable objects should be allowed to return to any pool.
    /// </summary>
    /// <param name="poolObject">The pool object to allow pool returns for.</param>
    public static void AllowReturn(PoolObject poolObject)
    {
        DestroyOnReturn.Remove(poolObject);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

        LocalBuilder destroy = generator.DeclareLocal(typeof(bool));
        Label allowReturnLabel = generator.DefineLabel();
        Label elseLabel = generator.DefineLabel();

        newInstructions.InsertRange(0, new CodeInstruction[]
        {
            //
            // if (ReturnPoolObjectPatch.CheckStatus(poolableObject, out bool destroy))
            // {
            //     if (destroy)
            //     {
            //         this.ResetObject(poolableObject);
            //         UnityEngine.Object.Destroy(poolableObject);
            //     }
            //
            //     BPLogger.Debug("Prevented return of an object.");
            //     return;
            // }
            //
            new(OpCodes.Ldarg_1), // [1] PoolObject
            new(OpCodes.Ldloca_S, destroy),
            new(OpCodes.Call, Method(typeof(ReturnPoolObjectPatch), nameof(CheckStatus))),
            new(OpCodes.Brfalse_S, allowReturnLabel),
            new(OpCodes.Ldloc_S, destroy),
            new(OpCodes.Brfalse_S, elseLabel),
            new(OpCodes.Ldarg_0), // [0] Pool (this)
            new(OpCodes.Ldarg_1), // [1] PoolObject
            new(OpCodes.Call, Method("GameObjectPools.Pool:ResetObject")),
            new(OpCodes.Ldarg_1), // [1] PoolObject
            new(OpCodes.Call, Method(typeof(Object), nameof(Object.Destroy), new System.Type[] { typeof(Object) })),

            new CodeInstruction(OpCodes.Ldstr, "Prevented return of an object.").WithLabels(elseLabel),
            new(OpCodes.Call, Method(typeof(BPLogger), nameof(BPLogger.Debug))),
            new(OpCodes.Ret),

            // Returning to pool is allowed.
            new CodeInstruction(OpCodes.Nop).WithLabels(allowReturnLabel),
        });

        return newInstructions.FinishTranspiler();
    }

    private static void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DestroyOnReturn.Clear();
    }

    private static bool CheckStatus(PoolObject poolObject, out bool destroy)
    {
        return poolObject is null
            ? (destroy = false)
            : DestroyOnReturn.TryGetValue(poolObject, out destroy);
    }
}
