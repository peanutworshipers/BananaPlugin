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
