namespace BananaPlugin.API.Utils;

using BananaPlugin.Extensions;
using MEC;
#if LOCAL
using Mirror;
#endif
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// A class responsible for ensuring prefabs have been loaded.
/// </summary>
public static class PrefabHelper
{
    private static bool prefabsLoaded;

    static PrefabHelper()
    {
        MECExtensions.Run(CheckPrefabsLoaded, Segment.Update);
    }

    /// <summary>
    /// Gets a value indicating whether prefabs have been initialized on the server.
    /// </summary>
    public static bool PrefabsLoaded => prefabsLoaded;

    /// <summary>
    /// Runs a specified delegate when prefabs have been loaded.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    /// <param name="action">The delegate to run.</param>
    /// <param name="args">The args to pass to the specified delegate.</param>
    public static void RunWhenReady<T>(T action, params object[] args)
        where T : Delegate
    {
        if (prefabsLoaded)
        {
            action.DynamicInvoke(args);
        }
        else
        {
            MECExtensions.Run(RunWhenReadyCoroutine<T>, Segment.Update, action, args);
        }
    }

    private static IEnumerator<float> CheckPrefabsLoaded()
    {
        BPLogger.IdentifyMethodAs(nameof(PrefabHelper), nameof(CheckPrefabsLoaded));

        while (SceneManager.GetActiveScene().name != "Facility")
        {
            yield return Timing.WaitForOneFrame;
        }

        yield return Timing.WaitForOneFrame;

        prefabsLoaded = true;

#if LOCAL
        foreach (KeyValuePair<uint, UnityEngine.GameObject> prefab in NetworkClient.prefabs)
        {
            BPLogger.Info($"{prefab.Value.name} ({prefab.Key})");

            foreach (NetworkBehaviour comp in prefab.Value.GetComponents<NetworkBehaviour>())
            {
                BPLogger.Info($"- {comp.GetType().Name}");
            }

            BPLogger.Info("\n");
            BPLogger.Info("\n");
        }
#endif
    }

    private static IEnumerator<float> RunWhenReadyCoroutine<T>(T action, object[] args)
        where T : Delegate
    {
        BPLogger.IdentifyMethodAs(nameof(PrefabHelper), nameof(RunWhenReady) + "<" + typeof(T).Name + ">");

        while (!prefabsLoaded)
        {
            yield return Timing.WaitForOneFrame;
        }

        action.DynamicInvoke(args);
    }
}
