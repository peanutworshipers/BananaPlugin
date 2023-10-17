namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.Extensions;
using MapEditorReborn.API.Features.Objects;
using MEC;
using SCP294.Classes;
using System.Collections.Generic;
using static BananaPlugin.API.Utils.DependencyChecker;

/// <summary>
/// The main feature responsible for handling SCP-294 balancing.
/// </summary>
public sealed class Scp294Balance : BananaFeature
{
    private CoroutineHandle mainHandle;

    private Scp294Balance()
    {
    }

    /// <inheritdoc/>
    public override string Name => "SCP-294 Balance";

    /// <inheritdoc/>
    public override string Prefix => "294";

    /// <inheritdoc/>
    protected override void Enable()
    {
        if (!CheckDependencies(Dependency.SCP294 & Dependency.MapEditorReborn))
        {
            return;
        }

        ExHandlers.Server.RoundStarted += this.RoundStarted;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ExHandlers.Server.RoundStarted -= this.RoundStarted;
    }

    private void RoundStarted()
    {
        MECExtensions.RunAfterFrames(1, Segment.Update, this.WaitOneFrame);
    }

    private void WaitOneFrame()
    {
        foreach (SchematicObject scp294 in Scp294Plugin.Instance.SpawnedSCP294s.Keys)
        {
            SCP294Object.SetSCP294Uses(scp294, 1);
        }

        this.mainHandle.KillAssignNew(this.IncrementScp294Uses, Segment.Update);
    }

    private IEnumerator<float> IncrementScp294Uses()
    {
        const float TimeBetweenIncrements = 180f;

        float time = 0f;

        while (true)
        {
            while (time < TimeBetweenIncrements)
            {
                yield return Timing.WaitForOneFrame;
                time += Timing.DeltaTime;
            }

            time -= TimeBetweenIncrements;

            foreach (SchematicObject scp294 in Scp294Plugin.Instance.SpawnedSCP294s.Keys)
            {
                SCP294Object.SetSCP294Uses(scp294, Scp294Plugin.Instance.SCP294UsesLeft[scp294] + 1);
            }
        }
    }
}
