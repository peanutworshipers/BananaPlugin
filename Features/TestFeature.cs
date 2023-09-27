namespace BananaPlugin.Features;

#if DEBUG
using BananaPlugin.API;
using BananaPlugin.API.Interfaces;
using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.API.Utils.CustomWriters;
using BananaPlugin.Extensions;
using CommandSystem;
using Exiled.API.Features;
using Mirror.LiteNetLib4Mirror;
using RoundRestarting;
using System;
using System.Threading;

/// <summary>
/// The test feature used for debugging.
/// </summary>
public sealed class TestFeature : BananaFeature
{
    static TestFeature()
    {
        new Thread(CheckDeadThread)
        {
            IsBackground = true,
        }.Start();
    }

    private TestFeature()
    {
        this.Commands =
        [
            new TestCommand(this),
        ];
    }

    /// <inheritdoc/>
    public override string Name => "Test Feature";

    /// <inheritdoc/>
    public override string Prefix => "test";

    /// <inheritdoc/>
    public override ICommand[] Commands { get; }

    /// <inheritdoc/>
    protected override void Enable()
    {
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
    }

    private static void CheckDeadThread()
    {
        int attempts = 0;

    Loop:
        while (true)
        {
            if (LiteNetLib4MirrorCore.Host is null || LiteNetLib4MirrorCore.Host._logicThread is null)
            {
                Thread.Sleep(500);
                continue;
            }

            break;
        }

        while (true)
        {
            if (!LiteNetLib4MirrorCore.Host._logicThread.IsAlive)
            {
                if (attempts++ < 5)
                {
                    BPLogger.Error($"NetworkManager thread has died. Attempting to restart thread...");

                    Thread.Sleep(3000);

                    LiteNetLib4MirrorCore.Host._logicThread = new Thread(LiteNetLib4MirrorCore.Host.UpdateLogic)
                    {
                        Name = "LogicThread",
                        IsBackground = true,
                    };
                    LiteNetLib4MirrorCore.Host._logicThread.Start();
                    Thread.Sleep(200);
                    Map.Broadcast(15, $"<color=#ff0000><b>[SERVER CONSOLE]</b> FATAL ERROR:\n<i><size=35>NETWORK MANAGER THREAD DIED.</size></i></color>", global::Broadcast.BroadcastFlags.Normal, true);
                }
                else
                {
                    BPLogger.Error("Attempted restarting thread 5 times. Aborting. Disconnecting all players and restarting...");
                    LiteNetLib4MirrorCore.Host.DisconnectAll();
                    Thread.Sleep(5000);
                    ServerStatic.StopNextRound = ServerStatic.NextRoundAction.Restart;
                    RoundRestart.ChangeLevel(noShutdownMessage: true);
                    return;
                }

                goto Loop;
            }

            Thread.Sleep(10000);
        }
    }

    private sealed class TestCommand : IFeatureSubcommand<TestFeature>, IRequiresRank, IHiddenCommand
    {
        public TestCommand(TestFeature parent)
        {
            this.Parent = parent;
        }

        public TestFeature Parent { get; }

        public string Command => "test";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "A command used for testing.";

        public string[] Usage => Array.Empty<string>();

        public BRank RankRequirement => BRank.Developer;

        public string GetHelp(ArraySegment<string> arguments)
        {
            return this.HelpProviderFormat("Time to test!");
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string? response)
        {
            if (!sender.CheckPermission(BRank.Developer, out response))
            {
                return false;
            }

            if (!ExPlayer.TryGet(sender, out ExPlayer player))
            {
                response = "Not a player.";
                return false;
            }

            (ushort hor, ushort vert) = player.CameraTransform.rotation.ToClientUShorts();

            player.ReferenceHub.SetHubRotation(hor, vert);

            response = "Testing...";
            return true;
        }
    }
}
#endif
