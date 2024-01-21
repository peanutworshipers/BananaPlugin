namespace BananaPlugin.Extensions;

using BananaPlugin.API.Utils;
using MEC;
using System;
using System.Collections.Generic;

/// <summary>
/// A utility class designed to make managing coroutines easier.
/// </summary>
/// <remarks>Some of the defined methods arent extensions.</remarks>
public static class MECExtensions
{
    /// <summary>
    /// Runs the specified coroutine delegate.
    /// </summary>
    /// <typeparam name="T">The type of the delegate.</typeparam>
    /// <param name="coroutine">The coroutine delegate.</param>
    /// <param name="segment">The segment to run on.</param>
    /// <param name="args">The arguments to pass to the delegate.</param>
    /// <returns>A coroutine handle representing the newly run coroutine instance.</returns>
    public static CoroutineHandle Run<T>(T coroutine, Segment segment, params object[] args)
        where T : Delegate
    {
        IEnumerator<float> enumerator = MakeSafeCoroutine(coroutine, args);

        return Timing.RunCoroutine(enumerator, segment);
    }

    /// <summary>
    /// Runs a specified delegate after the number of frames.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    /// <param name="frameCount">The number of frames to wait.</param>
    /// <param name="segment">The segment to run on.</param>
    /// <param name="coroutine">The coroutine delegate.</param>
    /// <param name="args">The arguments to pass to the delegate.</param>
    public static void RunAfterFrames<T>(int frameCount, Segment segment, T coroutine, params object[] args)
        where T : Delegate
    {
        IEnumerator<float> safeCoroutine = MakeSafeCoroutine(RunAfterFramesCoroutine<T>, frameCount, coroutine, args);

        Timing.RunCoroutine(safeCoroutine, segment);
    }

    /// <summary>
    /// Makes the specifies coroutine delegate safe, that is, it handles exception logging.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    /// <param name="coroutine">The coroutine delegate.</param>
    /// <param name="args">The arguments to pass to the delegate.</param>
    /// <returns>A safe version of the specified coroutine delegate.</returns>
    /// <exception cref="ArgumentException">ArgumentException.</exception>
    public static IEnumerator<float> MakeSafeCoroutine<T>(T coroutine, params object[] args)
        where T : Delegate
    {
        if (coroutine.Method.ReturnType != typeof(IEnumerator<float>))
        {
            throw new ArgumentException("Coroutine must have a return type of 'IEnumerator<float>'", nameof(coroutine));
        }

        IEnumerator<float> enumerator = (IEnumerator<float>)coroutine.DynamicInvoke(args);

        while (true)
        {
            try
            {
                if (!enumerator.MoveNext())
                {
                    yield break;
                }
            }
            catch (Exception e)
            {
                // ReSharper disable PossibleNullReferenceException
                string message = string.Concat(
                    "Error in coroutine [",
                    coroutine.Method.DeclaringType.FullName,
                    "::",
                    coroutine.Method.Name,
                    "]\n",
                    e.ToString());

                BPLogger.Error(message);

                yield break;
            }

            yield return enumerator.Current;
        }
    }

    /// <summary>
    /// Kills a specified coroutine handle reference, then assigns a newly run coroutine delegate.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    /// <param name="handle">The handle to kill and replace.</param>
    /// <param name="newCoroutine">The coroutine to run and assign.</param>
    /// <param name="segment">The segment to run on.</param>
    /// <param name="args">The arguments to pass to the coroutine delegate.</param>
    public static void KillAssignNew<T>(ref this CoroutineHandle handle, T newCoroutine, Segment segment, params object[] args)
        where T : Delegate
    {
        Timing.KillCoroutines(handle);

        AssignNew(ref handle, newCoroutine, segment, args);
    }

    /// <summary>
    /// Assigns a newly run coroutine delegate to the specified coroutine handle reference.
    /// </summary>
    /// <typeparam name="T">The delegate type.</typeparam>
    /// <param name="handle">The handle to kill and replace.</param>
    /// <param name="newCoroutine">The coroutine to run and assign.</param>
    /// <param name="segment">The segment to run on.</param>
    /// <param name="args">The arguments to pass to the coroutine delegate.</param>
    public static void AssignNew<T>(ref this CoroutineHandle handle, T newCoroutine, Segment segment, params object[] args)
        where T : Delegate => handle = Run(newCoroutine, segment, args);

    private static IEnumerator<float> RunAfterFramesCoroutine<T>(int frameCount, T coroutine, params object[] args)
        where T : Delegate
    {
        for (int i = 0; i < frameCount; i++)
        {
            yield return Timing.WaitForOneFrame;
        }

        coroutine.DynamicInvoke(args);
    }
}
