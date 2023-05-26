namespace BananaPlugin.Extensions;

using AdminToys;
using Mirror;
using System;
using UnityEngine;
using Utils.Networking;

/// <summary>
/// Consists of primitive object extensions for changing their properties on clients.
/// </summary>
[Obsolete("This is currently not needed.", true)]
public static class PrimitiveObjectExtensions
{
    /// <summary>
    /// Sets the scale of a primitive object.
    /// </summary>
    /// <param name="toy">The primitive object to change the scale of.</param>
    /// <param name="newScale">The new scale of the object.</param>
    public static void SetScale(this PrimitiveObjectToy toy, Vector3 newScale)
    {
        toy.transform.localScale = newScale;

        if (!NetworkServer.spawned.ContainsKey(toy.netId))
        {
            NetworkServer.Spawn(toy.gameObject);
        }
        else
        {
            SpawnMessage spawn = new ()
            {
                netId = toy.netId,
                isLocalPlayer = false,
                isOwner = false,
                sceneId = toy.netIdentity.sceneId,
                assetId = toy.netIdentity.assetId,
                position = toy.transform.position,
                rotation = toy.transform.rotation,
                scale = newScale,
                payload = new (Array.Empty<byte>()),
            };

            spawn.SendToAuthenticated();
        }

        toy.NetworkScale = newScale;
    }

    /// <summary>
    /// Sets the rotation of a primitive object.
    /// </summary>
    /// <param name="toy">The primitive object to change the rotation of.</param>
    /// <param name="newRotation">The new rotation of the object.</param>
    public static void SetRotation(this PrimitiveObjectToy toy, Quaternion newRotation)
    {
        toy.transform.rotation = newRotation;

        if (!NetworkServer.spawned.ContainsKey(toy.netId))
        {
            NetworkServer.Spawn(toy.gameObject);
        }
        else
        {
            SpawnMessage spawn = new ()
            {
                netId = toy.netId,
                isLocalPlayer = false,
                isOwner = false,
                sceneId = toy.netIdentity.sceneId,
                assetId = toy.netIdentity.assetId,
                position = toy.transform.position,
                rotation = newRotation,
                scale = toy.transform.localScale,
                payload = new (Array.Empty<byte>()),
            };

            spawn.SendToAuthenticated();
        }

        toy.NetworkRotation = new LowPrecisionQuaternion(newRotation);
    }

    /// <summary>
    /// Sets the position of a primitive object.
    /// </summary>
    /// <param name="toy">The primitive object to change the position of.</param>
    /// <param name="newPosition">The new position of the object.</param>
    public static void SetPosition(this PrimitiveObjectToy toy, Vector3 newPosition)
    {
        toy.transform.position = newPosition;

        if (!NetworkServer.spawned.ContainsKey(toy.netId))
        {
            NetworkServer.Spawn(toy.gameObject);
        }
        else
        {
            /*SpawnMessage spawn = new ()
            {
                netId = toy.netId,
                isLocalPlayer = false,
                isOwner = false,
                sceneId = toy.netIdentity.sceneId,
                assetId = toy.netIdentity.assetId,
                position = newPosition,
                rotation = toy.transform.rotation,
                scale = toy.transform.localScale,
                payload = new (Array.Empty<byte>()),
            };

            spawn.SendToAuthenticated();*/
        }

        toy.NetworkPosition = newPosition;
    }

    /// <summary>
    /// Sets the properties of a primitive object.
    /// </summary>
    /// <param name="toy">The primitive object to change the properties of.</param>
    /// <param name="newPosition">The new position of the primitive object.</param>
    /// <param name="newRotation">The new rotation of the primitive object.</param>
    /// <param name="newScale">The new scale of the primitive object.</param>
    public static void SetProperties(this PrimitiveObjectToy toy, Vector3? newPosition = null, Quaternion? newRotation = null, Vector3? newScale = null)
    {
        if (newPosition is not null)
        {
            toy.transform.position = newPosition.Value;
            toy.NetworkPosition = newPosition.Value;
        }

        if (newRotation is not null)
        {
            toy.transform.rotation = newRotation.Value;
            toy.NetworkRotation = new LowPrecisionQuaternion(newRotation.Value);
        }

        if (newScale is not null)
        {
            toy.transform.localScale = newScale.Value;
            toy.NetworkScale = newScale.Value;
        }

        if (!NetworkServer.spawned.ContainsKey(toy.netId))
        {
            NetworkServer.Spawn(toy.gameObject);
        }
        else
        {
            /*SpawnMessage spawn = new ()
            {
                netId = toy.netId,
                isLocalPlayer = false,
                isOwner = false,
                sceneId = toy.netIdentity.sceneId,
                assetId = toy.netIdentity.assetId,
                position = newPosition ?? toy.transform.position,
                rotation = newRotation ?? toy.transform.rotation,
                scale = newScale ?? toy.transform.localScale,
                payload = new (Array.Empty<byte>()),
            };

            spawn.SendToAuthenticated();*/
        }
    }
}
