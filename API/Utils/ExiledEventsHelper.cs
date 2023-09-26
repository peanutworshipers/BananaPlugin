namespace BananaPlugin.API.Utils;

using BananaPlugin.Extensions;
using MEC;
using System;
using System.Collections.Generic;

/// <summary>
/// A utility class designed to assist in exiled events management.
/// </summary>
public static class ExiledEventsHelper
{
    private static bool exiledEventsReady = false;
    private static CoroutineHandle eventsReadyCoroutine;
    private static Queue<(Delegate, object[])> actions = new();

    /// <summary>
    /// Runs a specified delegate when the exiled events instance is not null.
    /// </summary>
    /// <typeparam name="T">The type of the delegate to invoke.</typeparam>
    /// <param name="action">The delegate to invoke.</param>
    /// <param name="args">The arguments to pass onto the delegate when invoked.</param>
    public static void RunWhenExiledEventsReady<T>(this T action, params object[] args)
        where T : Delegate
    {
        if (exiledEventsReady)
        {
            action.DynamicInvoke(args);
            return;
        }

        actions.Enqueue((action, args));

        if (!eventsReadyCoroutine.IsRunning)
        {
            MECExtensions.Run(ExiledEventsReadyCoroutine, Segment.Update);
        }
    }

    private static IEnumerator<float> ExiledEventsReadyCoroutine()
    {
        while (Exiled.Events.Events.Instance is null)
        {
            yield return Timing.WaitForOneFrame;
        }

        exiledEventsReady = true;

        Delegate action;
        object[] args;

        while (actions.Count > 0)
        {
            (action, args) = actions.Dequeue();

            action.DynamicInvoke(args);
        }
    }
}
