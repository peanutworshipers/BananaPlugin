namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.Features.Configs;
using Exiled.API.Features;
using MEC;
using System.Collections.Generic;

/// <summary>
/// The main feature responsible for automatically enabling the alpha warhead after a specified amount of time from round start.
/// </summary>
public sealed class AutoNuke : BananaFeatureConfig<CfgAutoNuke>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoNuke"/> class.
    /// </summary>
    public AutoNuke()
    {
        if (!Plugin.AssertEnabled())
        {
            throw new System.InvalidOperationException();
        }

        this.LocalConfig = Plugin.Instance.Config.AutoNuke;
        Config.AutoNukeUpdated = this.OnConfigUpdated;
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
    public override CfgAutoNuke LocalConfig { get; protected set; }

    /// <summary>
    /// Starts the warhead from the 90 second count.
    /// </summary>
    public static void StartFrom90Seconds()
    {
        ref AlphaWarheadSyncInfo info = ref Warhead.Controller.Info;

        info.ResumeScenario = false;
        info.ScenarioId = 3;

        Warhead.Controller.InstantPrepare();
        Warhead.Controller.StartDetonation(false, false, null);
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
    }

    private IEnumerator<float> CheckRoundTime()
    {
        while (Round.IsStarted)
        {
            yield return Timing.WaitForOneFrame;

            if (Round.ElapsedTime.TotalSeconds < this.LocalConfig.NukeSeconds)
            {
                continue;
            }

            // Do stuff here.
        }
    }
}
