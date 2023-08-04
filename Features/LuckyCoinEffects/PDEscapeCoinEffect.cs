namespace BananaPlugin.Features.LuckyCoinEffects;

using Achievements;
using CustomPlayerEffects;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp106;
using UnityEngine;

/// <summary>
/// The coin effect that lets you escape the pocket dimension.
/// </summary>
public sealed class PDEscapeCoinEffect : LuckyCoinEffect
{
    /// <summary>
    /// Gets the responses for escaping SCP-106's pocket dimension.
    /// </summary>
    public static string[] Responses { get; } = new string[]
    {
        "Congratulations, you've managed to escape for now. But don't get too comfortable; old wrinkly's got all the time in the world to come after you again.",
        "Look at you, popping in and out of dimensions like a cosmic jack-in-the-box. Enjoy the breather, but remember, that old ghoul will be eagerly waiting for round two.",
        "Whoa, you're out! Guess SCP-106's pocket dimension didn't have a 'no escape' policy after all. Just don't expect a parade in your honor.",
        "Bravo! You've outfoxed the pocket dimension, but don't gloat too soon – it's got a nasty habit of giving second chances.",
        "Well, well, well, look who's back from their 'wondrous' trip. Hope you soaked up enough otherworldly charm, because you're not done dancing with Mr. 106.",
        "Well, well, looks like Lady Luck took pity on you this time. Welcome back to the real world, enjoy it while it lasts.",
        "Congratulations, you've won the grand prize - an escape from the nightmare dimension! Don't get too comfy though, fate has its ways.",
        "Heads up, you're out! But don't think you're off the hook just yet. The universe has a funny way of keeping tabs on people like you.",
        "You owe the coin a nice thank-you note, 'cause you just got a one-way ticket out of the abyss. Hope you cherish this stroke of fortune.",
        "You've hit the cosmic jackpot, escaping that dreadful pocket dimension. Just remember, luck can change as quickly as a flip of a coin.",
    };

    /// <inheritdoc/>
    public override bool IsBuff => true;

    /// <inheritdoc/>
    public override void ApplyEffect(ExPlayer player, out string message)
    {
        ReferenceHub hub = player.ReferenceHub;
        IFpcRole fpcRole = (IFpcRole)player.ReferenceHub.roleManager.CurrentRole;
        fpcRole.FpcModule.ServerOverridePosition(Scp106PocketExitFinder.GetBestExitPosition(fpcRole), Vector3.zero);
        hub.playerEffectsController.DisableEffect<PocketCorroding>();
        hub.playerEffectsController.DisableEffect<Corroding>();
        AchievementHandlerBase.ServerAchieve(hub.connectionToClient, AchievementName.HeWillBeBack);
        message = Responses.RandomItem();
    }

    /// <inheritdoc/>
    public override float EvaluateEffectWeight(ExPlayer player)
    {
        return player.IsInPocketDimension
            ? 3f
            : 0f;
    }
}
