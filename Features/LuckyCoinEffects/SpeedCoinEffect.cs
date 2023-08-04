namespace BananaPlugin.Features.LuckyCoinEffects;

using BananaPlugin.API.Utils;
using CustomPlayerEffects;
using UnityEngine;

/// <summary>
/// The coin effect that gives you a speed boost.
/// </summary>
public sealed class SpeedCoinEffect : LuckyCoinEffect
{
    /// <summary>
    /// Gets the responses for the speed boost effect.
    /// </summary>
    public static string[] Responses { get; } = new string[]
    {
        "Luck is on your side, as a 50% speed boost empowers you. Move with lightning speed, catching your enemies off guard.",
        "Your speed surges by 50%! Harness this newfound momentum to outpace any nearby adversaries.",
        "A burst of speed engulfs you, providing a 50% boost. Use it wisely to gain the upper hand against nearby foes.",
        "With a stroke of fortune, you gain a 50% speed boost. Race through the battlefield, leaving your enemies struggling to keep up.",
        "Well, isn't this your lucky day! You finally managed to get a 50% speed boost. Maybe now you won't be so slow and can get things done.",
        "Congratulations! The coin took pity on you and decided to grant you a 50% speed boost. Now, don't waste your time like you usually do.",
        "Oh, look who got a 50% speed boost! It's a shame you couldn't accomplish anything without it. But hey, better late than never, right?",
        "Wow, you got a 50% speed boost! If only you were as quick in your actions as you are in flipping coins, you might have succeeded by now.",
        "Finally, a 50% speed boost for you! Let's hope this newfound speed can make up for all those times you stumbled and failed before.",
    };

    /// <inheritdoc/>
    public override bool IsBuff => true;

    /// <inheritdoc/>
    public override void ApplyEffect(ExPlayer player, out string message)
    {
        if (!player.ReferenceHub.playerEffectsController.TryGetEffect(out MovementBoost effect))
        {
            message = "Bad Code! Bad Code! Quick! Somebody call the developers!";
            return;
        }

        effect.ServerSetState(50, Random.Range(5f, 15f), true);
        message = Responses.RandomItem();
    }

    /// <inheritdoc/>
    public override float EvaluateEffectWeight(ExPlayer player)
    {
        float smallestSqrDist = float.MaxValue;

        for (int i = 0; i < PlayerListUtils.VerifiedHubs.Count; i++)
        {
            ReferenceHub hub = PlayerListUtils.VerifiedHubs[i];

            if (hub.roleManager.CurrentRole.Team == PlayerRoles.Team.SCPs)
            {
                smallestSqrDist = Mathf.Min(smallestSqrDist, (hub.transform.position - player.Transform.position).sqrMagnitude);
            }
        }

        return AnimationCurve.Linear(0f, 2f, 900f, 0.5f).Evaluate(smallestSqrDist);
    }
}
