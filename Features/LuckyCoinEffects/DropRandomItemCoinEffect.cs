namespace BananaPlugin.Features.LuckyCoinEffects;

using InventorySystem;
using InventorySystem.Items;
using NorthwoodLib.Pools;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A coin effect that forces the player to drop a random item.
/// </summary>
public sealed class DropRandomItemCoinEffect : LuckyCoinEffect
{
    /// <summary>
    /// Gets the responses for a player's item being dropped.
    /// </summary>
    public static string[] Responses { get; } = new string[]
    {
        "Seems like your item had a sudden urge for freedom and made a daring escape from your inventory...",
        "Uh-oh! Your item decided it needed some alone time on the ground...",
        "Oopsie-daisy! Your item decided to take a spontaneous leap of faith...",
        "Whoopsie! Your item took a detour and landed on the floor...",
        "Uh-oh, looks like your item went on a little adventure without you...",
        "Oops! Did you drop that item on purpose or are you just trying to make the floor feel special?",
        "Looks like someone's got butterfingers! Careful, we don't want you losing anything else.",
        "Well, well, well... Seems like you're having a hard time holding onto your precious items. Need some help with those slippery hands?",
        "Congratulations! You've just unlocked the 'Losing Items' achievement. Keep up the good work!",
        "Who needs an inventory anyway? Just toss your items around like confetti, it's much more fun!",
    };

    /// <inheritdoc/>
    public override bool IsBuff => false;

    /// <inheritdoc/>
    public override void ApplyEffect(ExPlayer player, out string message)
    {
        List<ItemBase> items = ListPool<ItemBase>.Shared.Rent();

        foreach (ItemBase item in player.Inventory.UserInventory.Items.Values)
        {
            if (player.CurrentItem.Base == item)
            {
                continue;
            }

            items.Add(item);
        }

        if (items.Count == 0)
        {
            message = "What? This... shouldn't happen...\nIf you are seeing this, tell Zereth his code is bad.";
            return;
        }

        player.Inventory.ServerDropItem(items[Random.Range(0, items.Count)].ItemSerial);

        message = Responses.RandomItem();
    }

    /// <inheritdoc/>
    public override float EvaluateEffectWeight(ExPlayer player)
    {
        int itemCount = player.Inventory.UserInventory.Items.Count;

        return itemCount > 1
            ? itemCount * 0.15f
            : 0f;
    }
}
