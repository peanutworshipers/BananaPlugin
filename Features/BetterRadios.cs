namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using Hints;
using InventorySystem;
using InventorySystem.Configs;
using InventorySystem.Items.Radio;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using VoiceChat.Playbacks;
using static BananaPlugin.Patches.VoiceTransceiverPatch;
using static HarmonyLib.AccessTools;
using static InventorySystem.Items.Radio.RadioMessages;

/// <summary>
/// The main feature responsible for radio channels.
/// </summary>
public sealed class BetterRadios : BananaFeature
{
    private Dictionary<int, int>? onChannels;

    private BetterRadios()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the RadioChannels instance.
    /// </summary>
    public static BetterRadios? Instance { get; private set; }

    /// <inheritdoc/>
    public override string Name => "Radio Channels";

    /// <inheritdoc/>
    public override string Prefix => "radio";

    /// <summary>
    /// Gets the players channel. 1 is the default.
    /// </summary>
    /// <param name="player">The player to get the radio channel of.</param>
    /// <param name="channel">The channel of the player.</param>
    public void GetPlayerChannel(ReferenceHub player, out int channel)
    {
        if (player.roleManager.CurrentRole.Team == PlayerRoles.Team.Dead)
        {
            channel = -1;
            return;
        }

        if (this.onChannels is null)
        {
            channel = -1;
            return;
        }

        if (!this.onChannels.TryGetValue(player.PlayerId, out channel))
        {
            channel = this.onChannels[player.PlayerId] = 1;
        }
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        TranceivingVoiceData += this.Chatting;
        ExHandlers.Player.UsingRadioBattery += this.UsingRadioBattery;
        ExHandlers.Player.ChangingRole += this.ChangingRole;
        ExHandlers.Player.PickingUpItem += this.PickingUpItem;

        this.onChannels = [];
        ExHandlers.Server.WaitingForPlayers += this.onChannels.Clear;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        TranceivingVoiceData -= this.Chatting;
        ExHandlers.Player.UsingRadioBattery -= this.UsingRadioBattery;
        ExHandlers.Player.ChangingRole -= this.ChangingRole;
        ExHandlers.Player.PickingUpItem -= this.PickingUpItem;

        ExHandlers.Server.WaitingForPlayers -= this.onChannels!.Clear;
        this.onChannels = null;
    }

    private static TextHint GetRadioHint(RoleTypeId role)
    {
        const int duration = 8;

        string roleColor = ExExtensions.RoleExtensions.GetColor(role).ToHex();

        string text = $"<line-height=-1100>\n<line-height=1.5em>\n<size=60><color={roleColor}><b><u>You can switch radio channels by \nright-clicking your radio while equipped!</u></b></color></size>";

        HintParameter[] parameters =
        [
            new StringHintParameter(text),
        ];

        HintEffect[] effects =
        [
             HintEffectPresets.TrailingPulseAlpha(1f, 0.4f, 0.5f, 4, 0f, 3),
        ];

        return new(text, parameters, effects, duration);
    }

    private void ChangingRole(ChangingRoleEventArgs ev)
    {
        // Only show a hint upon round start or respawning.
        if (ev.Reason != ExEnums.SpawnReason.RoundStart && ev.Reason != ExEnums.SpawnReason.Respawn)
        {
            return;
        }

        // Make sure their inventory is being assigned.
        if ((ev.SpawnFlags & PlayerRoles.RoleSpawnFlags.AssignInventory) == 0)
        {
            return;
        }

        if (!StartingInventories.DefinedInventories.TryGetValue(ev.NewRole, out InventoryRoleInfo info))
        {
            return;
        }

        if (!info.Items.Contains(ItemType.Radio))
        {
            return;
        }

        ev.Player.HintDisplay.Show(GetRadioHint(ev.NewRole));
    }

    private void PickingUpItem(PickingUpItemEventArgs ev)
    {
        if (!ev.IsAllowed)
        {
            return;
        }

        if (ev.Pickup.Type != ItemType.Radio)
        {
            return;
        }

        ev.Player.HintDisplay.Show(GetRadioHint(ev.Player.Role));
    }

    private void UsingRadioBattery(UsingRadioBatteryEventArgs ev)
    {
        const float OneThird = 1f / 3f;

        // Don't drain battery unless radio is being used.
        ev.Drain *= PersonalRadioPlayback.IsTransmitting(ev.Player.ReferenceHub)
            ? OneThird
            : 0f;
    }

    private void Chatting(ref TranceiveData data)
    {
        if (data.Channel != VoiceChat.VoiceChatChannel.Radio)
        {
            return;
        }

        this.GetPlayerChannel(data.Player, out int playerChannel);
        this.GetPlayerChannel(data.Receiver, out int recvChannel);

        if (playerChannel == -1 || recvChannel == -1 || playerChannel != recvChannel)
        {
            data.Channel = VoiceChat.VoiceChatChannel.Proximity;
        }
    }

    private int GetNextChannel(int curChannel)
    {
        curChannel = curChannel <= 0
            ? 1
            : (curChannel + 1) % 6;

        if (curChannel == 0)
        {
            curChannel = 6;
        }

        return curChannel;
    }

    [HarmonyPatch(typeof(RadioItem), nameof(RadioItem.ServerProcessCmd))]
    private static class RadioCommandPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
#pragma warning disable SA1118 // Parameter should not span multiple lines
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            FieldInfo radioItem_enabled = Field(typeof(RadioItem), nameof(RadioItem._enabled));

            CodeInstruction switchInstruction = newInstructions.Find(x => x.opcode == OpCodes.Switch);

            Label[] labels = (Label[])switchInstruction.operand;

            int index = newInstructions.FindIndex(x => x.labels.Contains(labels[(int)RadioCommand.Enable]));
            index = newInstructions.FindIndex(index, x => x.StoresField(radioItem_enabled));

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, Method(typeof(RadioCommandPatch), nameof(RadioToggled))),
            });

            index = newInstructions.FindIndex(x => x.labels.Contains(labels[(int)RadioCommand.Disable]));
            index = newInstructions.FindIndex(index, x => x.StoresField(radioItem_enabled));

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, Method(typeof(RadioCommandPatch), nameof(RadioToggled))),
            });

            return newInstructions.FinishTranspiler();
#pragma warning restore SA1118 // Parameter should not span multiple lines
        }

        /// <remarks>Forces the enabled state of the radio on return.</remarks>
        private static bool RadioToggled(bool changingState, RadioItem item)
        {
            if (!ExPlayer.TryGet(item.Owner, out ExPlayer player))
            {
                return changingState;
            }

            if (!Instance || !Instance.Enabled)
            {
                player.ShowHint("<b><color=#ff4040>RADIO CHANNELS ARE DISABLED</color></b>", 1);
                return changingState;
            }

            Instance.GetPlayerChannel(item.Owner, out int channel);

            // Radio is turning off.
            if (!changingState)
            {
                // Channel is max.
                if (channel >= 6)
                {
                    Instance.onChannels![item.Owner.PlayerId] = -1;
                    player.ShowHint($"<b><color=#ff0000>RADIO OFF</color></b>", 1);
                    return false;
                }

                // Cycle channel, dont turn off.
                channel = Instance.GetNextChannel(channel);
                Instance.onChannels![item.Owner.PlayerId] = channel;
                player.ShowHint($"<b>RADIO CHANNEL: {channel}</b>", 1);
                return true;
            }
            else
            {
                Instance.onChannels![item.Owner.PlayerId] = 1;
                player.ShowHint($"<b>RADIO CHANNEL: 1</b>", 1);
                return true;
            }
        }
    }
}
