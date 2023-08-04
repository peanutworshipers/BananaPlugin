namespace BananaPlugin.Features.LuckyCoinEffects;

using BananaPlugin.API.Utils;
using PlayerRoles.PlayableScps.Scp096;
using UnityEngine;

/// <summary>
/// The coin effect that removes a player from SCP-096 target lists.
/// </summary>
public sealed class Scp096TargetRemoveCoinEffect : LuckyCoinEffect
{
    /// <summary>
    /// Gets the responses from no longer being targeted by SCP-096.
    /// </summary>
    public static string[] Responses { get; } = new string[]
    {
        "Phew! It's like that face was never etched in your memory...",
        "Seems like your mind hit the 'delete' button on that haunting image.",
        "Whoosh! The memory of that face vanished into thin air.",
        "Looks like your brain pulled off a sneaky trick and erased that face from its archives.",
        "Congratulations! You've successfully evicted that terrifying face from your thoughts.",
        "Ah, the joys of accidental face-gazing. Hope you enjoyed the adrenaline rush!",
        "Guess what? SCP-096 must have thought your memory wasn't worth the trouble. How flattering!",
        "Well, well, well... Looks like SCP-096 took pity on you and decided to spare your fragile mind from its haunting image.",
        "Congratulations! You've won the 'I saw SCP-096 by accident and got a free memory wipe' prize.",
        "Ah, the perks of unintentionally gazing upon SCP-096's face. Consider yourself lucky, or maybe just forgetful.",
    };

    /// <summary>
    /// Gets the weight curve for the rage time of SCP-096.
    /// </summary>
    public static AnimationCurve RageWeightCurve { get; } = AnimationCurve.EaseInOut(0f, 0f, 35f, 1f);

    /// <summary>
    /// Gets the weight curve for the distance from SCP-096.
    /// </summary>
    public static AnimationCurve SqrDistWeightCurve { get; } = AnimationCurve.EaseInOut(900f, 1.5f, 10000f, 0f);

    /// <inheritdoc/>
    public override bool IsBuff => true;

    /// <inheritdoc/>
    public override void ApplyEffect(ExPlayer player, out string message)
    {
        for (int i = 0; i < PlayerListUtils.VerifiedHubs.Count; i++)
        {
            ReferenceHub hub = PlayerListUtils.VerifiedHubs[i];

            if (hub.roleManager.CurrentRole is not Scp096Role scp096)
            {
                continue;
            }

            if (!scp096.SubroutineModule.TryGetSubroutine(out Scp096TargetsTracker tracker))
            {
                continue;
            }

            if (tracker.RemoveTarget(player.ReferenceHub))
            {
                Broadcast.Singleton.TargetAddElement(hub.connectionToClient, "<size=30>You suddenly feel as if you've forgotten somebody...</size>", 5, Broadcast.BroadcastFlags.Normal);
            }
        }

        message = Responses.RandomItem();
    }

    /// <inheritdoc/>
    public override float EvaluateEffectWeight(ExPlayer player)
    {
        float maxRageTime = 0f;
        float minSqrDist = 10000f;
        int totalTargets = 0;

        for (int i = 0; i < PlayerListUtils.VerifiedHubs.Count; i++)
        {
            ReferenceHub hub = PlayerListUtils.VerifiedHubs[i];

            if (hub.roleManager.CurrentRole is not Scp096Role scp096)
            {
                continue;
            }

            if (!scp096.SubroutineModule.TryGetSubroutine(out Scp096RageManager rageManager))
            {
                continue;
            }

            if (!rageManager._targetsTracker.Targets.Contains(player.ReferenceHub))
            {
                continue;
            }

            maxRageTime = Mathf.Max(maxRageTime, rageManager.IsEnraged ? rageManager.EnragedTimeLeft : 25f);
            minSqrDist = Mathf.Min(minSqrDist, Mathf.Clamp((hub.transform.position - player.Transform.position).sqrMagnitude, 3600f, 10000f));
            totalTargets++;
        }

        if (totalTargets == 0)
        {
            return 0f;
        }

        float weight = RageWeightCurve.Evaluate(maxRageTime) + SqrDistWeightCurve.Evaluate(minSqrDist);
        return weight;
    }
}
