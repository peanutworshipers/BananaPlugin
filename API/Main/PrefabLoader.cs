namespace BananaPlugin.API.Main;

using AdminToys;
using Utils;
using Interactables.Interobjects.DoorUtils;
using Mirror;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

/// <summary>
/// The class responsible for loading network prefab objects.
/// </summary>
public static class PrefabLoader
{
    private const uint PrimitiveToyKey = 1321952889u;
    private const uint LightToyKey = 3956448839u;
    private const uint EzDoorKey = 1883254029u;
    private const uint HczDoorKey = 2295511789u;
    private const uint LczDoorKey = 3038351124u;

    private static PrimitiveObjectToy? primitiveToy;
    private static LightSourceToy? lightToy;
    private static DoorVariant? ezDoor;
    private static DoorVariant? hczDoor;
    private static DoorVariant? lczDoor;

    static PrefabLoader()
    {
        PrefabHelper.RunWhenReady(InitializeFields);
    }

    /// <summary>
    /// Gets a value indicating whether prefab objects have been loaded.
    /// </summary>
    [MemberNotNullWhen(
        true,
        nameof(PrimitiveToy),
        nameof(primitiveToy),
        nameof(LightToy),
        nameof(lightToy),
        nameof(EzDoor),
        nameof(ezDoor),
        nameof(HczDoor),
        nameof(hczDoor),
        nameof(LczDoor),
        nameof(lczDoor))]
    public static bool ObjectsLoaded { get; private set; }

    /// <summary>
    /// Gets the primitive toy network prefab.
    /// </summary>
    public static PrimitiveObjectToy? PrimitiveToy => primitiveToy;

    /// <summary>
    /// Gets the light toy network prefab.
    /// </summary>
    private static LightSourceToy? LightToy => lightToy;

    /// <summary>
    /// Gets the entrance door network prefab.
    /// </summary>
    private static DoorVariant? EzDoor => ezDoor;

    /// <summary>
    /// Gets the heavy door network prefab.
    /// </summary>
    private static DoorVariant? HczDoor => hczDoor;

    /// <summary>
    /// Gets the light door network prefab.
    /// </summary>
    private static DoorVariant? LczDoor => lczDoor;

    /// <summary>
    /// Spawns a primitive object.
    /// </summary>
    /// <param name="type">The primitive type to spawn.</param>
    /// <param name="pos">The position to spawn.</param>
    /// <param name="rot">The rotation to spawn.</param>
    /// <param name="scale">The scale to spawn.</param>
    /// <param name="color">The color to spawn.</param>
    /// <returns>A primitive object spawned with the specified properties.</returns>
    public static PrimitiveObjectToy SpawnPrimitive(PrimitiveType type, Vector3 pos, Vector3 rot, Vector3 scale, Color color)
    {
        // Null checking responsiblity
        // should be left to the caller
        // and not us.
        PrimitiveObjectToy prim = UObject.Instantiate(primitiveToy) !;

        // ReSharper disable Unity.InefficientPropertyAccess
        prim.transform.position = pos;
        prim.transform.rotation = Quaternion.Euler(rot);
        prim.transform.localScale = scale;

        NetworkServer.Spawn(prim.gameObject);

        prim.NetworkMaterialColor = color;
        prim.NetworkPrimitiveType = type;

        return prim;
    }

    private static void InitializeFields()
    {
        BPLogger.Info("Initializing schematic fields...");

        Dictionary<uint, GameObject> prefabs = NetworkClient.prefabs;

        if (!prefabs.TryGetValue(PrimitiveToyKey, out GameObject gameObj) || !gameObj.TryGetComponent(out PrimitiveObjectToy primToy))
        {
            BPLogger.Error("Could not find Primitive Object Prefab! Please contact the developer.");
        }
        else
        {
            primitiveToy = primToy;
            BPLogger.Info("Primitive Object Prefab loaded!");
        }

        if (!prefabs.TryGetValue(LightToyKey, out gameObj) || !gameObj.TryGetComponent(out LightSourceToy lightsrcToy))
        {
            BPLogger.Error("Could not find Light Source Prefab! Please contact the developer.");
        }
        else
        {
            lightToy = lightsrcToy;
            BPLogger.Info("Light Source Prefab loaded!");
        }

        if (!prefabs.TryGetValue(EzDoorKey, out gameObj) || !gameObj.TryGetComponent(out DoorVariant door))
        {
            BPLogger.Error("Could not find Entrance Door Prefab! Please contact the developer.");
        }
        else
        {
            ezDoor = door;
            BPLogger.Info("Entrance Door Prefab loaded!");
        }

        if (!prefabs.TryGetValue(HczDoorKey, out gameObj) || !gameObj.TryGetComponent(out door))
        {
            BPLogger.Error("Could not find Heavy Door Prefab! Please contact the developer.");
        }
        else
        {
            hczDoor = door;
            BPLogger.Info("Heavy Door Prefab loaded!");
        }

        if (!prefabs.TryGetValue(LczDoorKey, out gameObj) || !gameObj.TryGetComponent(out door))
        {
            BPLogger.Error("Could not find Light Door Prefab! Please contact the developer.");
        }
        else
        {
            lczDoor = door;
            BPLogger.Info("Light Door Prefab loaded!");
        }

        ObjectsLoaded = true;

        BPLogger.Info("Schematic fields initialized.");
    }
}
