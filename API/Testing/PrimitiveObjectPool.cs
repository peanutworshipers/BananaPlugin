namespace BananaPlugin.API.Testing;

using AdminToys;
using BananaPlugin.API.Main;
using BananaPlugin.Extensions;
using MEC;
using Mirror;
using NorthwoodLib.Pools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A primitive object pool responsible for managing temporary primtive objects.
/// </summary>
public sealed class PrimitiveObjectPool : IPool<PrimitiveObjectToy>
{
    static PrimitiveObjectPool()
    {
        SceneManager.sceneUnloaded += SceneUnloaded;
    }

    /// <summary>
    /// Gets the shared primitive object pool.
    /// </summary>
    public static PrimitiveObjectPool Shared { get; } = new ();

    private static Queue<PrimitiveObjectToy> Pool { get; set; } = new ();

    /// <summary>
    /// Rents a primitive object from the queue.
    /// </summary>
    /// <returns>The primitive object that was rented.</returns>
    public PrimitiveObjectToy Rent()
    {
        if (!PrefabLoader.ObjectsLoaded)
        {
            // Null checking responsiblity
            // should be left to the caller
            // and not us.
            return null!;
        }

        RETRY:
        if (!Pool.TryDequeue(out PrimitiveObjectToy toy))
        {
            toy = PrefabLoader.SpawnPrimitive(PrimitiveType.Cube, new Vector2(0, 6000), Vector3.zero, Vector3.one, Color.white);
            toy.NetworkMovementSmoothing = 0;
        }
        else if (!toy)
        {
            goto RETRY;
        }

        toy.gameObject.SetActive(true);
        return toy;
    }

    /// <summary>
    /// Returns a primitive object to the queue.
    /// </summary>
    /// <param name="toy">The primitive object to return.</param>
    public void Return(PrimitiveObjectToy toy)
    {
        if (!toy)
        {
            return;
        }

        toy.transform.SetParent(null);
        toy.SetProperties(new Vector3(0, 6000f), Quaternion.identity, Vector3.one);

        MECExtensions.RunAfterFrames(2, Segment.Update, this.ReturnSafe, toy);
    }

    private static void SceneUnloaded(Scene scene)
    {
        if (scene.name != "Facility")
        {
            return;
        }

        Pool.Clear();
    }

    private void ReturnSafe(PrimitiveObjectToy toy)
    {
        toy.gameObject.SetActive(false);
        Pool.Enqueue(toy);
    }
}
