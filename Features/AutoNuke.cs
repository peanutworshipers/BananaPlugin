namespace BananaPlugin.Features;

using BananaPlugin.API;
using BananaPlugin.API.Main;
using BananaPlugin.Extensions;
using BananaPlugin.Features.Configs;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Warhead;
using Exiled.Permissions.Extensions;
using MEC;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static Broadcast;

/// <summary>
/// The main feature responsible for automatically enabling the alpha warhead after a specified amount of time from round start.
/// </summary>
public sealed class AutoNuke : BananaFeatureConfig<CfgAutoNuke>
{
    private CoroutineHandle mainHandle;

    private AutoNuke()
    {
        if (!Plugin.AssertEnabled())
        {
            throw new InvalidOperationException();
        }

        this.LocalConfig = Plugin.Instance.Config.AutoNuke;
        Config.AutoNukeUpdated = this.OnConfigUpdated;

        ExHandlers.Server.WaitingForPlayers += this.ForceEnableOnWaitingForPlayers;

        this.Commands = new ICommand[]
        {
            new SetStartTimeCmd(this),
        };
    }

    /// <summary>
    /// Gets a value indicating whether the facility is being auto-nuked.
    /// </summary>
    public static bool AutoNuking { get; private set; }

    /// <inheritdoc/>
    public override string Name => "Auto Nuke";

    /// <inheritdoc/>
    public override string Prefix => "anuke";

    /// <inheritdoc/>
    public override ICommand[] Commands { get; }

    /// <inheritdoc/>
    public override CfgAutoNuke LocalConfig { get; protected set; }

    /// <summary>
    /// Starts the warhead from the 90 second count.
    /// </summary>
    public static void StartFrom90Seconds()
    {
        ref AlphaWarheadSyncInfo info = ref Warhead.Controller.Info;

        info.ResumeScenario = false;
        info.ScenarioId = 3;

        AlphaWarheadController.Singleton.InstantPrepare();
        AlphaWarheadController.Singleton.StartDetonation(false, false, null);
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        ExHandlers.Server.RoundStarted += this.RoundStarted;
        ExHandlers.Warhead.Stopping += this.Stopping;

        if (Round.IsStarted)
        {
            this.RoundStarted();
        }
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ExHandlers.Server.RoundStarted -= this.RoundStarted;
        ExHandlers.Warhead.Stopping -= this.Stopping;

        this.StopAutoNuking();
    }

    private void StopAutoNuking()
    {
        if (!AutoNuking || Warhead.IsDetonated)
        {
            return;
        }

        AutoNuking = false;
        Warhead.DetonationTimer = 15f;
        Warhead.Stop();
        Map.Broadcast(14, this.LocalConfig.BroadcastDisabledMsg, BroadcastFlags.Normal, true);
    }

    private void ForceEnableOnWaitingForPlayers()
    {
        this.Enabled = true;
    }

    private void RoundStarted()
    {
        AutoNuking = false;

        this.mainHandle.KillAssignNew(this.CheckRoundTime, Segment.Update);
    }

    private void Stopping(StoppingEventArgs ev)
    {
        if (AutoNuking)
        {
            ev.IsAllowed = false;
        }
    }

    private IEnumerator<float> CheckRoundTime()
    {
        while (Round.IsStarted && Round.InProgress)
        {
            if (Round.ElapsedTime.TotalSeconds < this.LocalConfig.NukeSeconds)
            {
                goto NextFrame;
            }

            StartFrom90Seconds();
            Map.Broadcast(14, this.LocalConfig.BroadcastMsg, BroadcastFlags.Normal, true);
            AutoNuking = true;

            while (!Warhead.IsDetonated && Warhead.IsInProgress && Round.InProgress)
            {
                Cassie.Clear();
                Cassie.Message("PITCH_0.2 .g3 PITCH_1.0", false, false, false);
                yield return Timing.WaitForSeconds(7f);
            }

            yield break;

            // Make yield return last statement
            // to force roundstarted check on
            // next frame, instead of the frame before.
            NextFrame:
            yield return Timing.WaitForOneFrame;
        }
    }

    /// <summary>
    /// The command responsible for setting the start time of auto nuke.
    /// </summary>
    public sealed class SetStartTimeCmd : ICommand, IUsageProvider, IHelpProvider
    {
        private readonly AutoNuke parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetStartTimeCmd"/> class.
        /// </summary>
        /// <param name="parent">The parent feature instance.</param>
        public SetStartTimeCmd(AutoNuke parent)
        {
            this.parent = parent;
        }

        /// <inheritdoc/>
        public string Command => "setstarttime";

        /// <inheritdoc/>
        public string[] Aliases { get; } = new string[]
        {
            "starttime",
            "stime",
        };

        /// <inheritdoc/>
        public string Description => "Sets the start time of auto nuke.";

        /// <inheritdoc/>
        public string[] Usage { get; } = new string[]
        {
            "time (seconds) / default",
        };

        /// <inheritdoc/>
        public string GetHelp(ArraySegment<string> arguments)
        {
            return this.HelpProviderFormat($"Enter the time auto nuke should enable after round start. (in seconds)" +
                $"\nUse 'default' to set the value to its default ({TimeSpan.FromSeconds(CfgAutoNuke.DefaultNukeSeconds):g}).");
        }

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [NotNullWhen(true)] out string? response)
        {
            if (!sender.CheckPermission(BRank.Developer, out _) && !sender.CheckPermission(PlayerPermissions.ServerConfigs, out response))
            {
                return false;
            }

            string input = arguments.At(0);

            double result;

            if (input.ToLower() == "default")
            {
                result = CfgAutoNuke.DefaultNukeSeconds;
            }
            else if (!double.TryParse(input, out result) || result is < 0)
            {
                response = "Input is invalid. Must be a number greater than zero.";
                return false;
            }

            this.parent.LocalConfig.NukeSeconds = result;
            response = $"Nuke time set to: {TimeSpan.FromSeconds(result):g}";
            return true;
        }
    }
}
