namespace BananaPlugin.Commands.Main;

using BananaPlugin.API.Utils;
using BananaPlugin.Extensions;
using CommandSystem;
using Exiled.API.Features;
using MEC;
using System;

/// <summary>
/// The main command responsible for redirecting players to the test server.
/// </summary>
public sealed class JoinTestServer : ICommand, IUsageProvider, IHelpProvider
{
    /// <summary>
    /// Gets a value indicating whether execution of this command is allowed.
    /// </summary>
    public static bool EnableJoining { get; internal set; } = false;

    /// <inheritdoc/>
    public string[] Usage => Array.Empty<string>();

    /// <inheritdoc/>
    public string Command => "testsrv";

    /// <inheritdoc/>
    public string[] Aliases => Array.Empty<string>();

    /// <inheritdoc/>
    public string Description => "Redirects you to the test server. Note this may disconnect you if it isnt open.";

    /// <inheritdoc/>
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (!EnableJoining && (ServerStatic.ServerPort != ServerPorts.TestServer))
        {
            response = "Joining the test server is currently disabled.";
            return false;
        }

        if (!ExPlayer.TryGet(sender, out ExPlayer player))
        {
            response = "You must be a player to use this command.";
            return false;
        }

        ushort portToRedirect = ServerPorts.TestServer;

        if (arguments.Count != 0)
        {
            portToRedirect = arguments.At(0) switch
            {
                "1" => ServerPorts.ServerOne,
                "one" => ServerPorts.ServerOne,

                "2" => ServerPorts.ServerTwo,
                "two" => ServerPorts.ServerTwo,

                "3" => ServerPorts.ServerThree,
                "three" => ServerPorts.ServerThree,

                _ => ServerPorts.TestServer,
            };
        }

        if (portToRedirect == ServerStatic.ServerPort)
        {
            response = "You are already on that server!";
            return false;
        }

        MECExtensions.RunAfterFrames(
            90,
            Segment.Update,
            player.Reconnect,
            portToRedirect,
            5,
            true,
            RoundRestarting.RoundRestartType.RedirectRestart);

        response = $"Sending you to server: {ServerPorts.ServerNames[portToRedirect]}";
        return true;
    }

    /// <inheritdoc/>
    public string GetHelp(ArraySegment<string> arguments)
    {
        return this.HelpProviderFormat("Execute the command to join the test server.");
    }
}
