namespace BananaPlugin.Features;

using BananaPlugin.API.Attributes;
using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.Extensions;
using BananaPlugin.Features.LuckyCoinEffects;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp914;
using MEC;
using NorthwoodLib.Pools;
using Scp914;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The main feature responsible for lucky coins.
/// </summary>
[AllowedPorts(ServerPorts.TestServer)]
public sealed class LuckyCoins : BananaFeature
{
    private Dictionary<ushort, LuckyCoinType>? coinSerials;

    private LuckyCoins()
    {
    }

    /// <summary>
    /// An enumeration used to represent lucky coin types.
    /// </summary>
    public enum LuckyCoinType : byte
    {
        /// <summary>
        /// Represents a non-lucky coin.
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents a lucky coin that gives random effects.
        /// </summary>
        Random,

        /// <summary>
        /// Represents a lucky coin that always gives buffs.
        /// </summary>
        Lucky,
    }

    /// <summary>
    /// Gets the array of all coin effects.
    /// </summary>
    public static LuckyCoinEffect[] AllCoinEffects { get; } =
    [
        new HealCoinEffect(),
        new SpeedCoinEffect(),
        new PDEscapeCoinEffect(),
        new Scp096TargetRemoveCoinEffect(),
        new DropRandomItemCoinEffect(),
        new HurtCoinEffect(),
    ];

    /// <inheritdoc/>
    public override string Name => "Lucky Coins";

    /// <inheritdoc/>
    public override string Prefix => "lcoin";

    /// <summary>
    /// Gets a random coin effect.
    /// </summary>
    /// <param name="isBuff">Specifies whether to get a coin effect that is a buff or not.</param>
    /// <param name="player">The player associated with this random effect.</param>
    /// <returns>A random lucky coin effect that matches the specified parameters, or null if not found.</returns>
    public LuckyCoinEffect? GetRandomCoinEffect(bool isBuff, ExPlayer player)
    {
        int i;
        float randomWeight = 0f;
        LuckyCoinEffect? result = null;
        List<LuckyCoinEffect> coinEffects = ListPool<LuckyCoinEffect>.Shared.Rent();
        List<float> weightValues = ListPool<float>.Shared.Rent();

        for (i = 0; i < AllCoinEffects.Length; i++)
        {
            if (AllCoinEffects[i].IsBuff == isBuff)
            {
                coinEffects.Add(AllCoinEffects[i]);

                float weight = AllCoinEffects[i].EvaluateEffectWeight(player);
                randomWeight += weight;
                weightValues.Add(weight);
            }
        }

        randomWeight = Random.Range(0f, randomWeight);

        if (randomWeight == 0f)
        {
            goto ReturnResult;
        }

        for (i = 0; i < coinEffects.Count; i++)
        {
            BPLogger.Debug($"i: {i} | random: {randomWeight} | weightVal: {weightValues[i]}");
            if ((randomWeight -= weightValues[i]) > 0f)
            {
                continue;
            }

            result = coinEffects[i];
            goto ReturnResult;
        }

        ReturnResult:
        ListPool<LuckyCoinEffect>.Shared.Return(coinEffects);
        ListPool<float>.Shared.Return(weightValues);
        return result;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        ExHandlers.Scp914.UpgradingPickup += this.UpgradingPickup;
        ExHandlers.Player.PickingUpItem += this.PickingUpItem;
        ExHandlers.Player.FlippingCoin += this.FlippingCoin;

        this.coinSerials = new();
        ExHandlers.Server.WaitingForPlayers += this.coinSerials.Clear;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ExHandlers.Scp914.UpgradingPickup -= this.UpgradingPickup;
        ExHandlers.Player.PickingUpItem -= this.PickingUpItem;
        ExHandlers.Player.FlippingCoin -= this.FlippingCoin;

        ExHandlers.Server.WaitingForPlayers -= this.coinSerials!.Clear;
        this.coinSerials = null;
    }

    private void UpgradingPickup(UpgradingPickupEventArgs ev)
    {
        if (!ev.Pickup.Base || ev.Pickup.Type != ItemType.Coin)
        {
            return;
        }

        if (this.coinSerials!.TryGetValue(ev.Pickup.Serial, out LuckyCoinType coinType) && coinType > 0)
        {
            ev.IsAllowed = false;
            ev.Pickup.Destroy();
            return;
        }

        (float destroyChance, coinType) = ev.KnobSetting switch
        {
            Scp914KnobSetting.Fine => (0.3f, LuckyCoinType.Random),
            Scp914KnobSetting.VeryFine => (0.8f, LuckyCoinType.Lucky),
            _ => (0f, LuckyCoinType.None)
        };

        if (destroyChance > 0f && Random.value <= destroyChance)
        {
            ev.IsAllowed = false;
            ev.Pickup.Destroy();
            return;
        }

        ushort serial = Pickup.CreateAndSpawn(ItemType.Coin, ev.OutputPosition, ev.Pickup.Transform.rotation).Serial;

        this.coinSerials[serial] = coinType;

        ev.IsAllowed = false;
        ev.Pickup.Destroy();
    }

    private void PickingUpItem(PickingUpItemEventArgs ev)
    {
        if (!ev.Pickup.Base || ev.Pickup.Type != ItemType.Coin)
        {
            return;
        }

        if (!this.coinSerials!.TryGetValue(ev.Pickup.Serial, out LuckyCoinType coinType) || coinType <= 0)
        {
            // Not a lucky coin.
            return;
        }

        ev.Player.ShowHint("You picked up a lucky coin!", 3);
    }

    private void FlippingCoin(FlippingCoinEventArgs ev)
    {
        if (ev.Player.CurrentItem is null || ev.Player.CurrentItem.Type != ItemType.Coin)
        {
            return;
        }

        if (!this.coinSerials!.TryGetValue(ev.Player.CurrentItem.Serial, out LuckyCoinType coinType) || coinType <= 0)
        {
            return;
        }

        if (coinType == LuckyCoinType.Lucky)
        {
            ev.IsTails = false;
        }

        MECExtensions.Run(this.ApplyCoinEffect, Segment.Update, !ev.IsTails, ev.Player);
    }

    private IEnumerator<float> ApplyCoinEffect(bool isBuff, ExPlayer player)
    {
        yield return Timing.WaitForSeconds(1.75f);

        LuckyCoinEffect? effect = this.GetRandomCoinEffect(isBuff, player);

        effect ??= isBuff ? new NoneBuffCoinEffect() : new NoneDebuffCoinEffect();

        effect.ApplyEffect(player, out string message);

        player.ShowHint(message, 7);
    }
}
