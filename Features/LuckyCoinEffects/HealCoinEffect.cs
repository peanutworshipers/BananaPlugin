namespace BananaPlugin.Features.LuckyCoinEffects;

using UnityEngine;

/// <summary>
/// The coin effect responsible for healing players.
/// </summary>
public sealed class HealCoinEffect : LuckyCoinEffect
{
    /// <summary>
    /// Gets the responses for a player being healed.
    /// </summary>
    public static string[] Responses { get; } =
    [
        "Ta-da! Your health bar just got a refreshing refill!",
        "Voila! Your wounds have magically mended themselves!",
        "Whoosh! Your health has made a triumphant comeback!",
        "Look at you, getting patched up like a pro!",
        "Wow, look at you, the master of self-inflicted injuries! Need a reminder on how to stay alive?",
        "Whoops, looks like someone needs a personal health coach to prevent these accidental boo-boos!",
        "Ah, the art of self-inflicted wounds. You're truly a master of your craft!",
        "Oopsie-daisy! Did you forget that health is a valuable resource? Time to brush up on those survival skills!",
        "Well, well, well...someone's playing the game of 'Let's Lose Health for Fun'. Impressive strategy, I must say!",
    ];

    /// <summary>
    /// Gets the curve to evaluate health to weight.
    /// </summary>
    public static AnimationCurve WeightCurve { get; } = new AnimationCurve()
    {
        keys =
        [
            new(0f, 2f),
            new(70f, 0.5f),
            new(70f, 0f),
        ],
    };

    /// <inheritdoc/>
    public override bool IsBuff => true;

    /// <inheritdoc/>
    public override void ApplyEffect(ExPlayer player, out string message)
    {
        player.Heal(100f, false);
        message = Responses.RandomItem();
    }

    /// <inheritdoc/>
    public override float EvaluateEffectWeight(ExPlayer player)
    {
        return WeightCurve.Evaluate(player.Health);
    }
}
