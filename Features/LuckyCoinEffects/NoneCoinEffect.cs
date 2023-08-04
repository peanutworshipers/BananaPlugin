namespace BananaPlugin.Features.LuckyCoinEffects;

/// <summary>
/// A buff coin effect that does nothing.
/// </summary>
public sealed class NoneBuffCoinEffect : LuckyCoinEffect
{
    /// <inheritdoc/>
    public override bool IsBuff => true;

    /// <inheritdoc/>
    public override void ApplyEffect(ExPlayer player, out string message)
    {
        message = "Nothing happened...";
    }

    /// <inheritdoc/>
    public override float EvaluateEffectWeight(ExPlayer player)
    {
        return 0f;
    }
}

/// <summary>
/// A debuff coin effect that does nothing.
/// </summary>
public sealed class NoneDebuffCoinEffect : LuckyCoinEffect
{
    /// <inheritdoc/>
    public override bool IsBuff => false;

    /// <inheritdoc/>
    public override void ApplyEffect(ExPlayer player, out string message)
    {
        message = "Nothing happened...";
    }

    /// <inheritdoc/>
    public override float EvaluateEffectWeight(ExPlayer player)
    {
        return 0f;
    }
}