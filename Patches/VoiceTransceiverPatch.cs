namespace BananaPlugin.Patches;

using BananaPlugin.API;
using BananaPlugin.API.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using VoiceChat;
using VoiceChat.Networking;
using static HarmonyLib.AccessTools;

/// <summary>
/// The main patch responsible for overriding voice transceiving.
/// </summary>
[HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
public static class VoiceTransceiverPatch
{
    private static readonly List<TranceivingVoice> Handlers = new(16);

    /// <summary>
    /// The delegate used for tranceiving voice.
    /// </summary>
    /// <param name="data">The data of the visibility check.</param>
    public delegate void TranceivingVoice(ref TranceiveData data);

    /// <summary>
    /// Event called when tranceiving voice.
    /// </summary>
    public static event TranceivingVoice TranceivingVoiceData
    {
        add => Handlers.Add(value);
        remove => Handlers.Remove(value);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
#pragma warning disable SA1118 // Parameter should not span multiple lines
        instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

        // VoiceChatChannel voiceChatChannel2 = voiceRole2.VoiceModule.ValidateReceive(msg.Speaker, voiceChatChannel);
        //
        // <------- Patch goes here.
        //
        // if (voiceChatChannel2 != 0)
        // {
        //     msg.Channel = voiceChatChannel2;
        //     allHub.connectionToClient.Send(msg);
        // }
        int index = newInstructions.FindLastIndex(x => x.opcode == OpCodes.Stloc_S && x.operand is LocalBuilder local && local.LocalIndex == 5);

        // VoiceTransceiverPatch.CallSafely(ref VoiceMessage msg, ref VoiceChatChannel voiceChatChannel2, hub, msg.Speaker);
        newInstructions.InsertRange(index + 1, new CodeInstruction[]
        {
            // ref VoiceMessage msg
            new(OpCodes.Ldarga_S, 1),

            // ref VoiceChatChannel voiceChatChannel2
            new(OpCodes.Ldloca_S, 5),

            // ReferenceHub hub
            new(OpCodes.Ldloc_S, 3),

            // ReferenceHub msg.Speaker
            new(OpCodes.Ldarga_S, 1),
            new(OpCodes.Ldfld, Field(typeof(VoiceMessage), nameof(VoiceMessage.Speaker))),

            // VoiceTransceiverPatch.CallSafely(ref VoiceMessage msg, ref VoiceChatChannel voiceChatChannel2, hub, msg.Speaker);
            new(OpCodes.Call, Method(typeof(VoiceTransceiverPatch), nameof(CallSafely))),
        });

        return newInstructions.FinishTranspiler();
#pragma warning restore SA1118 // Parameter should not span multiple lines
    }

    private static void CallSafely(ref VoiceMessage voiceMessage, ref VoiceChatChannel channel, ReferenceHub receiver, ReferenceHub player)
    {
        try
        {
            TranceiveData data = new(voiceMessage, channel, receiver, player);

            for (int i = 0; i < Handlers.Count; i++)
            {
                try
                {
                    Handlers[i].Invoke(ref data);
                }
                catch (Exception e)
                {
                    BPLogger.Error($"Failed to invoke event: {e}");
                }
            }

            voiceMessage = data.VoiceMessage;
            channel = data.Channel;
        }
        catch (Exception e)
        {
            BPLogger.Error(e.ToString());
        }
    }

    /// <summary>
    /// Struct responsible for holding visiblity check data.
    /// </summary>
    public struct TranceiveData
    {
        /// <summary>
        /// The receiver of the voice information.
        /// </summary>
        public readonly ReferenceHub Receiver;

        /// <summary>
        /// The player sending the voice data.
        /// </summary>
        public readonly ReferenceHub Player;

        /// <summary>
        /// Value representing the voice message that is being sent and received.
        /// </summary>
        public VoiceMessage VoiceMessage;

        /// <summary>
        /// Value representing the channel this voice message is being sent to.
        /// </summary>
        /// <remarks><see cref="VoiceChatChannel.None"/> prevents voice data from being sent.</remarks>
        public VoiceChatChannel Channel;

        /// <summary>
        /// The current event change priority.
        /// </summary>
        public EventChangePriority Priority;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranceiveData"/> struct.
        /// </summary>
        /// <param name="voiceMessage">Represents the voice message that is being sent and received.</param>
        /// <param name="intendedChannel">Represents the intended channel of this voice message.</param>
        /// <param name="receiver">The receiver of the voice information.</param>
        /// <param name="player">The player sending the voice data.</param>
        public TranceiveData(VoiceMessage voiceMessage, VoiceChatChannel intendedChannel, ReferenceHub receiver, ReferenceHub player)
        {
            this.VoiceMessage = voiceMessage;
            this.Channel = intendedChannel;
            this.Priority = EventChangePriority.None;
            this.Receiver = receiver;
            this.Player = player;
        }
    }
}
