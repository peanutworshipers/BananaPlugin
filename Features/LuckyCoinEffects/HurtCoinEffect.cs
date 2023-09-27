namespace BananaPlugin.Features.LuckyCoinEffects;

using UnityEngine;

/// <summary>
/// The coin effect responsible for hurting players.
/// </summary>
public sealed class HurtCoinEffect : LuckyCoinEffect
{
    /// <summary>
    /// Gets the responses for being hurt by the coin effect.
    /// </summary>
    public static string[] HurtResponses { get; } =
    [
        "You grit your teeth as a searing discomfort takes hold of you. Your knees buckle slightly, and you feel weakened.",
        "A surge of pain radiates through your body, leaving you feeling vulnerable and unsteady on your feet, but you manage to stay standing.",
        "You wince in response to an intense and lingering ache, but despite feeling weakened, you muster the strength to endure it.",
        "A distressing sensation leaves you breathless and drained. Though weakened, you manage to remain standing, determined to weather the storm.",
        "Enduring a sharp pain, you feel your body growing feeble, but you push through, refusing to succumb to the impact.",
    ];

    /// <summary>
    /// Gets the responses for being killed by the coin effect.
    /// </summary>
    public static string[] DiedResponses { get; } =
    [
        "Agony surges through your body, and you find it hard to stay on your feet. Darkness engulfs your vision as you succumb to the overwhelming force.",
        "A sharp and unbearable pain strikes you, weakening your resolve. Your legs give out, and you collapse, unable to fight it.",
        "You feel an excruciating sensation coursing through every fiber of your being. Your strength abandons you, and you crumple to the ground, defeated.",
        "A sudden impact hits you like a tidal wave of suffering. You struggle to stay conscious, but your body gives in, and you fall to the floor, life slipping away.",
        "A sudden and relentless force overwhelms you. Your last moments are filled with anguish as you lose the battle against it.",
    ];

    /// <summary>
    /// Gets the curve to evaluate health to weight.
    /// </summary>
    public static AnimationCurve WeightCurve { get; } = new AnimationCurve()
    {
        keys =
        [
            new(100f, 1f),
            new(51f, 0.25f),
            new(0f, 0f),
        ],
    };

    /// <inheritdoc/>
    public override bool IsBuff => false;

    /// <inheritdoc/>
    public override void ApplyEffect(ExPlayer player, out string message)
    {
        player.Hurt(Random.Range(25f, 65f));

        message = player.IsDead
            ? DiedResponses.RandomItem()
            : HurtResponses.RandomItem();
    }

    /// <inheritdoc/>
    public override float EvaluateEffectWeight(ExPlayer player)
    {
        return player.IsGodModeEnabled
            ? 0f
            : WeightCurve.Evaluate(player.Health);
    }
}
