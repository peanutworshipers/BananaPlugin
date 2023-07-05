namespace BananaPlugin.Features;

using BananaPlugin.API;
using BananaPlugin.API.Interfaces;
using BananaPlugin.API.Main;
using BananaPlugin.API.Utils.CustomWriters;
using BananaPlugin.Commands;
using BananaPlugin.Extensions;
using CommandSystem;
using CustomPlayerEffects;
using Exiled.Permissions.Extensions;
using Hazards;
using InventorySystem;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Pickups;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp173;
using RelativePositioning;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Utils;

/// <summary>
/// The main feature responsible for trolling in-game players.
/// </summary>
public sealed class Trolling : BananaFeature
{
    private CoroutineHandle spinHandle;

    private Trolling()
    {
        this.TrollCommands = new ICommand[]
        {
            new SpinCommand(this),
            new SeverCommand(this),
            new ForceTantrum(this),
            new BottleInvasion(this),
        };

        MainCommand.OnAssigned += this.EnableCommands;
    }

    /// <inheritdoc/>
    public override string Name => "Trolling";

    /// <inheritdoc/>
    public override string Prefix => "trol";

    /// <summary>
    /// Gets the hashset of currently spinning players.
    /// </summary>
    public HashSet<ReferenceHub> SpinningPlayers { get; } = new();

    /// <summary>
    /// Gets the troll commands for this feature.
    /// </summary>
    /// <remarks>We use a different array to apply subcommands to the main banana command, instead of overriding <see cref="BananaFeature.Commands"/>.</remarks>
    public ICommand[] TrollCommands { get; }

    /// <inheritdoc/>
    protected override void Enable()
    {
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        BottleInvasionHandler.ClearBottles();
        this.SpinningPlayers.Clear();
        Timing.KillCoroutines(this.spinHandle);
    }

    private void EnableCommands(MainCommand command)
    {
        for (int i = 0; i < this.TrollCommands.Length; i++)
        {
            MainCommand.Instance?.UnregisterCommand(this.TrollCommands[i]);
            MainCommand.Instance?.RegisterCommand(this.TrollCommands[i]);
        }
    }

    private void EnsureSpinHandle()
    {
        this.spinHandle.KillAssignNew(this.SpinPlayers, Segment.LateUpdate);
    }

    private IEnumerator<float> SpinPlayers()
    {
        while (true)
        {
            foreach (ReferenceHub ply in this.SpinningPlayers)
            {
                ply.SetRotation((ushort)Mathf.RoundToInt((Time.fixedUnscaledTime % 2f) / 2f * ushort.MaxValue), ushort.MaxValue / 2);
            }

            yield return Timing.WaitForOneFrame;
        }
    }

    /// <summary>
    /// A struct containing info about spawned bottles.
    /// </summary>
    public struct SpawnedBottleInfo
    {
        /// <summary>
        /// The netId of the object associated with this bottle.
        /// </summary>
        public uint NetId;

        /// <summary>
        /// The spawn time of the object associated with this bottle.
        /// </summary>
        public float SpawnTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpawnedBottleInfo"/> struct.
        /// </summary>
        /// <param name="netId">The netId of the instance.</param>
        /// <param name="spawnTime">The spawn time of the instance.</param>
        public SpawnedBottleInfo(uint netId, float spawnTime)
        {
            this.NetId = netId;
            this.SpawnTime = spawnTime;
        }
    }

    /// <summary>
    /// The main class responsible for handling bottle invasions.
    /// </summary>
    public static class BottleInvasionHandler
    {
        /// <summary>
        /// The maximum number of bottles that can be spawned (global).
        /// </summary>
        public const int MaxBottles = 100;

        /// <summary>
        /// The amount of time the bottles are sustained for.
        /// </summary>
        #warning implement this lul
        [Obsolete("Not implemented.")]
        public const float SpawnedTime = 60f;

        static BottleInvasionHandler()
        {
            ExHandlers.Server.WaitingForPlayers += WaitingForPlayers;
        }

        /// <summary>
        /// Gets the queue of spawned bottles.
        /// </summary>
        public static Queue<SpawnedBottleInfo> SpawnedBottles { get; } = new(MaxBottles);

        /// <summary>
        /// Attempts to spawn a bottle with the given hit information.
        /// </summary>
        /// <param name="hitInfo">The hit information to use.</param>
        /// <param name="response">The response.</param>
        /// <returns>A value indicating whether the operation was a success.</returns>
        public static bool TrySpawnBottle(RaycastHit hitInfo, out string response)
        {
            if (!hitInfo.collider)
            {
                response = "Can't spawn bottle! No valid surface!";
                return false;
            }

            if (SpawnedBottles.Count >= MaxBottles)
            {
                SpawnedBottleInfo bottleInfo = SpawnedBottles.Dequeue();

                NetworkServer.spawned.TryGetValue(bottleInfo.NetId, out NetworkIdentity? networkIdentity);

                NetworkServer.Destroy(networkIdentity?.gameObject);
            }

            Quaternion rotation = Quaternion.LookRotation(hitInfo.normal);
            Vector3 position = hitInfo.point + (hitInfo.normal * 0.125f);

            ItemPickupBase bottle = UnityEngine.Object.Instantiate(InventoryItemLoader.AvailableItems[ItemType.SCP207].PickupDropModel, position, rotation);
            bottle.Info.Locked = true;
            NetworkServer.Spawn(bottle.gameObject);
            SpawnedBottles.Enqueue(new SpawnedBottleInfo(bottle.netId, Time.fixedUnscaledTime));

            response = "Bottle spawned!";
            return true;
        }

        /// <summary>
        /// Clears all spawned bottles.
        /// </summary>
        public static void ClearBottles()
        {
            while (SpawnedBottles.TryDequeue(out SpawnedBottleInfo bottleInfo))
            {
                NetworkServer.spawned.TryGetValue(bottleInfo.NetId, out NetworkIdentity? networkIdentity);

                NetworkServer.Destroy(networkIdentity?.gameObject);
            }
        }

        private static void WaitingForPlayers()
        {
            SpawnedBottles.Clear();
        }
    }

    /// <summary>
    /// The main command responsible for spinning players.
    /// </summary>
    public sealed class SpinCommand : IFeatureSubcommand<Trolling>, IRequiresRank
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpinCommand"/> class.
        /// </summary>
        /// <param name="parent">The parent feature associated with this command.</param>
        public SpinCommand(Trolling parent)
        {
            this.Parent = parent;
        }

        /// <inheritdoc/>
        public Trolling Parent { get; }

        /// <inheritdoc/>
        public string Command => "spin";

        /// <inheritdoc/>
        public string[] Aliases => Array.Empty<string>();

        /// <inheritdoc/>
        public string Description => "Spins or unspins the specified player(s).";

        /// <inheritdoc/>
        public string[] Usage { get; } = new string[]
        {
            "%player%",
            "0 / 1",
        };

        /// <inheritdoc/>
        public BRank RankRequirement => BRank.JuniorAdministrator;

        /// <inheritdoc/>
        public string GetHelp(ArraySegment<string> arguments)
        {
            return this.HelpProviderFormat("Specify playerids, and then 1 to enable spinning, or 0 to disable spinning for the specified players.");
        }

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [NotNullWhen(true)] out string? response)
        {
            if (!sender.CheckPermission(this.RankRequirement, out response))
            {
                return false;
            }

            if (arguments.Count < 2)
            {
                response = "To execute this command provide at least 2 arguments!";
                return false;
            }

            if (!this.Parent.Enabled)
            {
                response = "This feature for this command is disabled.";
                return false;
            }

            List<ReferenceHub> players = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out string[] newargs);

            if (newargs == null || newargs.Length == 0)
            {
                response = "Could not process provided arguments.";
                return false;
            }

            if (!int.TryParse(newargs[0], out int result))
            {
                response = "Could not process provided arguments.";
                return false;
            }

            if (result != 0 && result != 1)
            {
                response = "Second argument must be zero or one.";
                return false;
            }

            foreach (ReferenceHub player in players)
            {
                if (result == 0)
                {
                    this.Parent.SpinningPlayers.Remove(player);
                }
                else
                {
                    this.Parent.SpinningPlayers.Add(player);
                }
            }

            this.Parent.EnsureSpinHandle();

            response = "funny spin go brrr...";
            return true;
        }
    }

    /// <summary>
    /// The main command responsible for severing players hands.
    /// </summary>
    public sealed class SeverCommand : IFeatureSubcommand<Trolling>, IRequiresRank
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SeverCommand"/> class.
        /// </summary>
        /// <param name="parent">The parent feature associated with this command.</param>
        public SeverCommand(Trolling parent)
        {
            this.Parent = parent;
        }

        /// <inheritdoc/>
        public Trolling Parent { get; }

        /// <inheritdoc/>
        public string Command => "severhands";

        /// <inheritdoc/>
        public string[] Aliases => new string[]
        {
            "shands",
        };

        /// <inheritdoc/>
        public string Description => "Severs or unsevers players hands.";

        /// <inheritdoc/>
        public string[] Usage { get; } = new string[]
        {
            "%player%",
        };

        /// <inheritdoc/>
        public BRank RankRequirement => BRank.JuniorAdministrator;

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [NotNullWhen(true)] out string? response)
        {
            if (!sender.CheckPermission(this.RankRequirement, out response))
            {
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "To execute this command provide at least 1 argument!";
                return false;
            }

            if (!this.Parent.Enabled)
            {
                response = "This feature for this command is disabled.";
                return false;
            }

            List<ReferenceHub> players = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out string[] newargs);

            foreach (ReferenceHub player in players)
            {
                SeveredHands effect = player.playerEffectsController.GetEffect<SeveredHands>();

                effect.Intensity = effect.IsEnabled ? (byte)0 : (byte)1;
                effect.TimeLeft = 0;
            }

            response = "Toggled severed hands for the specified players!";
            return true;
        }

        /// <inheritdoc/>
        public string GetHelp(ArraySegment<string> arguments)
        {
            return this.HelpProviderFormat("Supply playerids to toggle severed hands for.");
        }
    }

    /// <summary>
    /// The main command responsible for forcing a tantrum on players.
    /// </summary>
    public sealed class BottleInvasion : IFeatureSubcommand<Trolling>, IRequiresRank
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BottleInvasion"/> class.
        /// </summary>
        /// <param name="parent">The parent feature associated with this command.</param>
        public BottleInvasion(Trolling parent)
        {
            this.Parent = parent;
        }

        /// <inheritdoc/>
        public Trolling Parent { get; }

        /// <inheritdoc/>
        public string Command => "bottleinvasion";

        /// <inheritdoc/>
        public string[] Aliases => new string[]
        {
            "bottle",
            "binv",
        };

        /// <inheritdoc/>
        public string Description => "Forces a tantrum on yourself.";

        /// <inheritdoc/>
        public string[] Usage => Array.Empty<string>();

        /// <inheritdoc/>
        public BRank RankRequirement => BRank.JuniorAdministrator;

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [NotNullWhen(true)] out string? response)
        {
            if (!sender.CheckPermission(this.RankRequirement, out response))
            {
                return false;
            }

            if (!this.Parent.Enabled)
            {
                response = "This feature for this command is disabled.";
                return false;
            }

            if (arguments.Count > 0 && arguments.At(0).ToLower() == "clear")
            {
                BottleInvasionHandler.ClearBottles();
                response = "Cleared all bottles!";
                return true;
            }

            if (!ExFeatures.Player.TryGet(sender, out ExFeatures.Player plySender))
            {
                response = "Must be a player to use this command!";
                return false;
            }

            Physics.Raycast(plySender.CameraTransform.position, plySender.CameraTransform.forward, out RaycastHit hitInfo, 100f, MicroHIDItem.WallMask);

            return BottleInvasionHandler.TrySpawnBottle(hitInfo, out response);
        }

        /// <inheritdoc/>
        public string GetHelp(ArraySegment<string> arguments)
        {
            return this.HelpProviderFormat("Execute the command, or run with the argument 'clear' to clear all spawned bottles.");
        }
    }

    /// <summary>
    /// The main command responsible for forcing a tantrum on players.
    /// </summary>
    public sealed class ForceTantrum : IFeatureSubcommand<Trolling>, IRequiresRank
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ForceTantrum"/> class.
        /// </summary>
        /// <param name="parent">The parent feature associated with this command.</param>
        public ForceTantrum(Trolling parent)
        {
            this.Parent = parent;
        }

        /// <inheritdoc/>
        public Trolling Parent { get; }

        /// <inheritdoc/>
        public string Command => "forcetantrum";

        /// <inheritdoc/>
        public string[] Aliases => new string[]
        {
            "ftant",
            "tantrum",
            "ftantrum",
        };

        /// <inheritdoc/>
        public string Description => "Forces a tantrum on yourself.";

        /// <inheritdoc/>
        public string[] Usage => Array.Empty<string>();

        /// <inheritdoc/>
        public BRank RankRequirement => BRank.JuniorAdministrator;

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [NotNullWhen(true)] out string? response)
        {
            if (!sender.CheckPermission(this.RankRequirement, out response))
            {
                return false;
            }

            if (!this.Parent.Enabled)
            {
                response = "This feature for this command is disabled.";
                return false;
            }

            ExFeatures.Player playerSender = ExFeatures.Player.Get(sender);

            if (playerSender is null)
            {
                response = "Must be a player to use this command.";
                return false;
            }

            if (!PlayerRoleLoader.TryGetRoleTemplate(RoleTypeId.Scp173, out PlayerRoleBase result))
            {
                response = "SCP-173 role couldn't be found?";
                return false;
            }

            Scp173TantrumAbility ability = ((Scp173Role)result).GetComponentInChildren<Scp173TantrumAbility>();

            if (!Physics.Raycast(playerSender.Position, Vector3.down, out RaycastHit hitInfo, 3f, ability._tantrumMask))
            {
                response = "You are not in a good position to do that...";
                return false;
            }

            TantrumEnvironmentalHazard tantrumEnvironmentalHazard = UnityEngine.Object.Instantiate(ability._tantrumPrefab);
            Vector3 targetPos = hitInfo.point + (Vector3.up * 1.25f);
            tantrumEnvironmentalHazard.SynchronizedPosition = new RelativePosition(targetPos);
            NetworkServer.Spawn(tantrumEnvironmentalHazard.gameObject);
            foreach (TeslaGate teslaGate in TeslaGateController.Singleton.TeslaGates)
            {
                if (teslaGate.IsInIdleRange(playerSender.Position))
                {
                    teslaGate.TantrumsToBeDestroyed.Add(tantrumEnvironmentalHazard);
                }
            }

            response = "Placed a tantrum!";
            return true;
        }

        /// <inheritdoc/>
        public string GetHelp(ArraySegment<string> arguments)
        {
            return this.HelpProviderFormat("Just execute the command! :)");
        }
    }
}
