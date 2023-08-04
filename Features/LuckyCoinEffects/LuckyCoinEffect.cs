namespace BananaPlugin.Features.LuckyCoinEffects;

/// <summary>
/// The base class for lucky coin effects.
/// </summary>
public abstract class LuckyCoinEffect
{
    /// <summary>
    /// Gets a value indicating whether the effect from this coin is a buff or debuff.
    /// </summary>
    public abstract bool IsBuff { get; }

    /// <summary>
    /// Evaluate the coin effect weight for the given player.
    /// </summary>
    /// <param name="player">The player to evaluate the weight for.</param>
    /// <returns>A float representing the weight value for the specified player.</returns>
    public abstract float EvaluateEffectWeight(ExPlayer player);

    /// <summary>
    /// Applies this coin effect for the given player.
    /// </summary>
    /// <param name="player">The player to apply the effect for.</param>
    /// <param name="message">The output message after applying the effect.</param>
    public abstract void ApplyEffect(ExPlayer player, out string message);
}
