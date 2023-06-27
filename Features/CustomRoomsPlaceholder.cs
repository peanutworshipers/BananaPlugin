namespace BananaPlugin.Features;

using AdminToys;
using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using Exiled.API.Features.Items;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using MapGeneration;
using Mirror;
using System.Linq;
using UnityEngine;

#pragma warning disable
#nullable disable
/// <summary>
/// The main feature responsible for custom rooms within the facility.
/// </summary>
public sealed class CustomRoomsPlaceholder : BananaFeature
{
    /// <inheritdoc/>
    public override string Name => "Custom Rooms";

    /// <inheritdoc/>
    public override string Prefix => "crooms";

    /// <inheritdoc/>
    protected override void Enable()
    {
        ExHandlers.Server.WaitingForPlayers += WaitingForPlayers;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ExHandlers.Server.WaitingForPlayers -= WaitingForPlayers;
    }

    private void WaitingForPlayers()
    {
        if (!Schematic.ObjectsLoaded)
            return;

        var EzRedroom = RoomIdentifier.AllRoomIdentifiers.FirstOrDefault(x => x.Name == RoomName.EzRedroom)?.ApiRoom;
        var OutsideRoom = RoomIdentifier.AllRoomIdentifiers.FirstOrDefault(x => x.Name == RoomName.Outside)?.ApiRoom;

        {
            // particle disruptor room
            if (EzRedroom is not null)
            {
                var schem = new Schematic(EzRedroom.Transform);

                var keyDoor = schem.SpawnDoor(DoorType.Entrance, new(0f, 2.68f, 0.577f), new(0f, 180f, 0f)) as BreakableDoor; // EZ Door (Items)
                keyDoor.RequiredPermissions.RequiredPermissions = KeycardPermissions.ContainmentLevelThree;
                keyDoor._ignoredDamageSources |= DoorDamageType.Weapon | DoorDamageType.Scp096 | DoorDamageType.Grenade;

                schem.SpawnDoor(DoorType.Entrance, new(0f, -0.04f, 0.577f), new(0f, 180f, 0f)); // EZ Door (Lower)

                var light = schem.SpawnLight(new(-0.007f, 3.89f, 4.018f), new(0.69f, 0.7f, 1f, 1f), 30f, 3); // LightSource
                light.NetworkLightShadows = true;
                schem.SpawnPickup(ItemType.KeycardFacilityManager, new(-0.537f, 3.414f, 3.8f), new(359.992f, 149.997f, 0f), new(1f, 1f, 1f)); // RegularKeycardPickup
                schem.SpawnPickup(ItemType.ParticleDisruptor, new(0.374f, 3.363f, 3.868f), new(0f, 105f, 90f), new(1f, 1f, 1f)); // DisruptorPickup

                schem.SpawnPrimitive(PrimitiveType.Cube, new(0f, 2.901f, 3.903f), new(0f, 90f, 0f), new(1f, 0.378f, 2f), new(0.4f, 0.4f, 0.4f, 1f)); // Cube (Pedestal)
                schem.SpawnPrimitive(PrimitiveType.Cube, new(0f, 2.557f, 4.2f), new(0f, 90f, 0f), new(0.3f, 0.1f, 0.2f), new(1f, 1f, 0.67f, 1f)); // Cube (Yellow Light)
                schem.SpawnPrimitive(PrimitiveType.Cube, new(0f, 2.65f, 3f), new(0f, 90f, 0f), new(9f, 0.2f, 11f), new(1f, 1f, 1f, 1f)); // Cube
                schem.SpawnPrimitive(PrimitiveType.Cube, new(-1.18f, 2.522f, 0.594f), new(0f, 90f, 0f), new(0.257f, 5.427f, 1.05f), new(1f, 1f, 1f, 1f)); // Cube
                schem.SpawnPrimitive(PrimitiveType.Cube, new(-4.247f, 1.271f, -3.387f), new(0f, 270f, 35.6f), new(4.8f, 0.2f, 2.507f), new(1f, 1f, 1f, 1f)); // Cube
                schem.SpawnPrimitive(PrimitiveType.Cube, new(1.18f, 2.527f, 0.594f), new(0f, 90f, 0f), new(0.257f, 5.416f, 1.05f), new(1f, 1f, 1f, 1f)); // Cube (1)
                schem.SpawnPrimitive(PrimitiveType.Cube, new(0f, 5.125f, 0.593f), new(0f, 90f, 0f), new(0.257f, 0.221f, 1.4f), new(1f, 1f, 1f, 1f)); // Cube (2)
                schem.SpawnPrimitive(PrimitiveType.Cube, new(-0.012f, 3.957f, 5.219f), new(0f, 90f, 0f), new(0.257f, 2.555f, 3.435f), new(1f, 1f, 1f, 1f)); // Cube (3)
                schem.SpawnPrimitive(PrimitiveType.Cube, new(-0.013f, 5.14f, 4.03f), new(0f, 90f, 0f), new(6.7f, 0.2f, 3.435f), new(1f, 1f, 1f, 1f)); // Cube (4)
                schem.SpawnPrimitive(PrimitiveType.Cube, new(-1.7f, 3.871f, 4.03f), new(0f, 90f, 0f), new(6.7f, 2.555f, 0.2f), new(1f, 1f, 1f, 1f)); // Cube (5)
                schem.SpawnPrimitive(PrimitiveType.Cube, new(1.7f, 3.871f, 4.03f), new(0f, 90f, 0f), new(6.7f, 2.555f, 0.2f), new(1f, 1f, 1f, 1f)); // Cube (6)
                schem.SpawnPrimitive(PrimitiveType.Cube, new(4.247f, 1.271f, -3.387f), new(0f, 270f, 35.6f), new(4.8f, 0.2f, 2.507f), new(1f, 1f, 1f, 1f)); // Cube (7)
                schem.SpawnPrimitive(PrimitiveType.Cube, new(0f, 2.462f, 0.594f), new(0f, 90f, 0f), new(0.257f, 0.253f, 1.4f), new(1f, 1f, 1f, 1f)); // Cube (8)
                light = schem.SpawnLight(new(0f, 2f, 4.2f), new(1f, 1f, 0.67f, 1f), 20f, 3); // Light Source
                light.NetworkLightShadows = true;

                BPLogger.Info("Spawned particle disruptor room.");
            }
            else
            {
                BPLogger.Error("Redroom is null");
            }

            // surface box
            if (OutsideRoom is not null)
            {
                var schem = new Schematic(OutsideRoom.Transform);

                Color color = new(1f, 1f, 1f);
                Vector3 size = new(0.55f, 1f, 0.55f);

                schem.SpawnPrimitive(PrimitiveType.Plane, new(-15.44f, 0.8f, -51.24f), new(0f, 0f, 0f), size, color); // floor
                schem.SpawnPrimitive(PrimitiveType.Plane, new(-15.44f, 6.3f, -51.24f), new(180f, 0f, 0f), size, color); // roof
                schem.SpawnPrimitive(PrimitiveType.Plane, new(-15.44f, 3.55f, -53.99f), new(90f, 0f, 0f), size, color); // negz
                schem.SpawnPrimitive(PrimitiveType.Plane, new(-15.44f, 3.55f, -48.49f), new(90f, 0f, -180f), size, color); // posz
                schem.SpawnPrimitive(PrimitiveType.Plane, new(-18.19f, 3.55f, -51.24f), new(90f, 0f, -90f), size, color); // negx
                schem.SpawnLight(new(-15.44f, 3.55f, -51.24f), new(0f, 1f, 1f), 10f); // light

                BPLogger.Info("Spawned outside box.");
            }
            else
            {
                BPLogger.Error("Outside is null");
            }
        }
    }

    public class Schematic
    {
        static Schematic()
        {
            PrefabHelper.RunWhenReady(InitializeFields);
        }

        public const uint PrimitiveToyKey = 1321952889u;
        public const uint LightToyKey = 3956448839u;
        public const uint EzDoorKey = 1883254029u;
        public const uint HczDoorKey = 2295511789u;
        public const uint LczDoorKey = 3038351124u;

        private static PrimitiveObjectToy primitiveToy;
        private static LightSourceToy lightToy;
        private static DoorVariant ezDoor;
        private static DoorVariant hczDoor;
        private static DoorVariant lczDoor;

        private static bool objectsLoaded = false;

        public static bool ObjectsLoaded => objectsLoaded;

        public Schematic(Transform reference)
        {
            Transform = reference;
        }

        public Transform Transform { get; }

        public PrimitiveObjectToy SpawnPrimitive(PrimitiveType type, Vector3 pos, Vector3 rot, Vector3 scale, Color color)
        {
            var prim = Object.Instantiate(primitiveToy);

            prim.transform.position = RelativePositioning.FromRelativePos(Transform, pos);
            prim.transform.rotation = Quaternion.Euler(RelativePositioning.FromRelativeRot(Transform, rot));
            prim.transform.localScale = scale;

            NetworkServer.Spawn(prim.gameObject);

            prim.NetworkMaterialColor = color;
            prim.NetworkPrimitiveType = type;
            prim.NetworkScale = scale;

            return prim;
        }

        public DoorVariant SpawnDoor(DoorType type, Vector3 pos, Vector3 rot)
        {
            var door = type switch
            {
                DoorType.Light => lczDoor,
                DoorType.Heavy => hczDoor,
                DoorType.Entrance => ezDoor,
                _ => throw new System.ArgumentOutOfRangeException(nameof(type))
            };

            door = Object.Instantiate(door);

            door.transform.position = RelativePositioning.FromRelativePos(Transform, pos);
            door.transform.rotation = Quaternion.Euler(RelativePositioning.FromRelativeRot(Transform, rot));

            NetworkServer.Spawn(door.gameObject);
            return door;
        }

        public LightSourceToy SpawnLight(Vector3 pos, Color color, float range, float intensity = 1f)
        {
            var light = Object.Instantiate(lightToy);

            light.transform.position = RelativePositioning.FromRelativePos(Transform, pos);

            NetworkServer.Spawn(light.gameObject);

            light.NetworkLightColor = color;
            light.NetworkLightRange = range;
            light.NetworkLightIntensity = intensity;

            return light;
        }

        public ItemPickupBase SpawnPickup(ItemType type, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            pos = RelativePositioning.FromRelativePos(Transform, pos);
            rot = RelativePositioning.FromRelativeRot(Transform, rot);

            var item = Item.Create(type);

            item.Scale = scale;

            return item.CreatePickup(pos, Quaternion.Euler(rot)).Base;
        }

        private static void InitializeFields()
        {
            BPLogger.Info("(Schematics) Initializing schematic fields...");

            var prefabs = NetworkClient.prefabs;

            GameObject gameObj;
            DoorVariant door;
            PrimitiveObjectToy primToy;
            LightSourceToy lightsrcToy;

            if (!prefabs.TryGetValue(PrimitiveToyKey, out gameObj) || !gameObj.TryGetComponent(out primToy))
            {
                BPLogger.Error("(Schematics) Could not find Primitive Object Prefab! Please contact the developer.");
            }
            else
            {
                primitiveToy = primToy;
                BPLogger.Info("(Schematics) Primitive Object Prefab loaded!");
            }

            if (!prefabs.TryGetValue(LightToyKey, out gameObj) || !gameObj.TryGetComponent(out lightsrcToy))
            {
                BPLogger.Error("(Schematics) Could not find Light Source Prefab! Please contact the developer.");
            }
            else
            {
                lightToy = lightsrcToy;
                BPLogger.Info("(Schematics) Light Source Prefab loaded!");
            }

            if (!prefabs.TryGetValue(EzDoorKey, out gameObj) || !gameObj.TryGetComponent(out door))
            {
                BPLogger.Error("(Schematics) Could not find Entrance Door Prefab! Please contact the developer.");
            }
            else
            {
                ezDoor = door;
                BPLogger.Info("(Schematics) Entrance Door Prefab loaded!");
            }

            if (!prefabs.TryGetValue(HczDoorKey, out gameObj) || !gameObj.TryGetComponent(out door))
            {
                BPLogger.Error("(Schematics) Could not find Heavy Door Prefab! Please contact the developer.");
            }
            else
            {
                hczDoor = door;
                BPLogger.Info("(Schematics) Heavy Door Prefab loaded!");
            }

            if (!prefabs.TryGetValue(LczDoorKey, out gameObj) || !gameObj.TryGetComponent(out door))
            {
                BPLogger.Error("(Schematics) Could not find Light Door Prefab! Please contact the developer.");
            }
            else
            {
                lczDoor = door;
                BPLogger.Info("(Schematics) Light Door Prefab loaded!");
            }

            objectsLoaded = true;

            BPLogger.Info("(Schematics) Schematic fields initialized.");
        }
    }

    public static class RelativePositioning
    {
        public static Vector3 GetRelativePos(Transform original, Transform obj)
        {
            return original.InverseTransformVector(obj.position - original.transform.position);
        }

        public static Vector3 FromRelativePos(Transform relative, Vector3 pos)
        {
            return relative.TransformVector(pos) + relative.transform.position;
        }

        public static Vector3 GetRelativeRot(Transform original, Transform obj)
        {
            return Quaternion.Inverse(Quaternion.Inverse(obj.rotation) * original.rotation).eulerAngles;
        }

        public static Vector3 FromRelativeRot(Transform relative, Vector3 rot)
        {
            return (relative.transform.rotation * Quaternion.Euler(rot)).eulerAngles;
        }
    }

    public enum DoorType
    {
        Light,
        Heavy,
        Entrance,
    }
}
#nullable restore
#pragma warning restore