namespace BananaPlugin.Patches.Fixes;

using BananaPlugin.API.Utils;
using HarmonyLib;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

#pragma warning disable SA1118 // Parameter should not span multiple lines

/// <summary>
/// A debug patch for <see cref="NetPeer.SendUserData"/>.
/// </summary>
[HarmonyPatch(typeof(NetPeer), nameof(NetPeer.SendUserData))]
internal static class SendUserDataPatch
{
    // ReSharper disable UnusedMember.Local
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

        LocalBuilder src = generator.DeclareLocal(typeof(Array));
        LocalBuilder srcOffset = generator.DeclareLocal(typeof(int));
        LocalBuilder dst = generator.DeclareLocal(typeof(Array));
        LocalBuilder dstOffset = generator.DeclareLocal(typeof(int));
        LocalBuilder count = generator.DeclareLocal(typeof(int));

        Label exitTryLabel = generator.DefineLabel();

        MethodInfo blockCopy = Method(typeof(Buffer), nameof(Buffer.BlockCopy));

        int index = newInstructions.FindLastIndex(x => x.Calls(blockCopy));

        newInstructions.RemoveAt(index);
        newInstructions[index].labels.Add(exitTryLabel);

        newInstructions.InsertRange(index, new[]
        {
            // Store sent arguments.
            new(OpCodes.Stloc_S, count),
            new(OpCodes.Stloc_S, dstOffset),
            new(OpCodes.Stloc_S, dst),
            new(OpCodes.Stloc_S, srcOffset),
            new(OpCodes.Stloc_S, src),

            // count = Math.Min(src.Length, count);
            new(OpCodes.Ldloc_S, src),
            new(OpCodes.Call, PropertyGetter(typeof(Array), nameof(Array.Length))),
            new(OpCodes.Ldloc_S, count),
            new(OpCodes.Call, Method(typeof(Math), nameof(Math.Min), new[] { typeof(int), typeof(int) })),
            new(OpCodes.Stloc_S, count),

            // Load them again.
            new(OpCodes.Ldloc_S, src),
            new(OpCodes.Ldloc_S, srcOffset),
            new(OpCodes.Ldloc_S, dst),
            new(OpCodes.Ldloc_S, dstOffset),
            new(OpCodes.Ldloc_S, count),

            // try
            // {
            //     System.Buffer.BlockCopy(src, srcOffset, dst, dstOffset, count);
            // }
            // catch (Exception ex)
            // {
            //     SendUserDataPatch.OnError(ex, packet, src, srcOffset, dst, dstOffset, count);
            // }
            new CodeInstruction(OpCodes.Call, blockCopy)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginExceptionBlock)),

            new(OpCodes.Leave_S, exitTryLabel),

            new CodeInstruction(OpCodes.Ldarg_1)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.BeginCatchBlock, typeof(Exception))),
            new(OpCodes.Ldloc_S, src),
            new(OpCodes.Ldloc_S, srcOffset),
            new(OpCodes.Ldloc_S, dst),
            new(OpCodes.Ldloc_S, dstOffset),
            new(OpCodes.Ldloc_S, count),
            new(OpCodes.Call, Method(typeof(SendUserDataPatch), nameof(OnError))),

            new CodeInstruction(OpCodes.Leave_S, exitTryLabel)
                .WithBlocks(new ExceptionBlock(ExceptionBlockType.EndExceptionBlock)),
        });

        return newInstructions.FinishTranspiler();
    }

    private static void OnError(Exception e, NetPacket packet, Array src, int srcOffset, Array dst, int dstOffset, int count)
    {
        string log = $"[DEBUGERROR-1] Network manager thread failed to invoke Buffer.BlockCopy:\npacket.ChannelId: {packet.ChannelId}\npacket.Property: {packet.Property}\nsrc.Length: {src.Length}\nsrcOffset: {srcOffset}\ndst.Length: {dst.Length}\ndstOffset: {dstOffset}\ncount: {count}\nStackTrace: {Environment.StackTrace}\nError message: {e.Message}";
        BPLogger.Error(log);

        string path = $"{Path.Combine(ExFeatures.Paths.Exiled, $"BananaDebug")}";

        Directory.CreateDirectory(path);

        StreamWriter writer = File.CreateText(Path.Combine(path, $"{DateTime.Now:MM-dd HH-mm-ss}.txt"));

        writer.WriteLine(log);

        writer.Close();
    }
}
