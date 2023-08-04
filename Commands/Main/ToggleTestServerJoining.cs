namespace BananaPlugin.Commands.Main;

using BananaPlugin.API;
using BananaPlugin.API.Interfaces;
using BananaPlugin.API.Utils;
using BananaPlugin.Extensions;
using CommandSystem;
using Exiled.API.Features;
using System;

/// <summary>
/// The main command responsible for toggling the join test server command.
/// </summary>
public sealed class ToggleTestServerJoining : ICommand, IUsageProvider, IHelpProvider, IHiddenCommand, IRequiresRank
{
    /// <inheritdoc/>
    public string Command => "toggletestsrv";

    /// <inheritdoc/>
    public string[] Aliases => Array.Empty<string>();

    /// <inheritdoc/>
    public string Description => "Toggles test server joining.";

    /// <inheritdoc/>
    public string[] Usage => Array.Empty<string>();

    /// <inheritdoc/>
    public BRank RankRequirement => BRank.Developer;

    /// <inheritdoc/>
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string? response)
    {
        if (!sender.CheckPermission(BRank.Developer, out response))
        {
            return false;
        }

        if (Server.Port == ServerPorts.TestServer)
        {
            response = "You are already on the test server.";
            return false;
        }

        JoinTestServer.EnableJoining = !JoinTestServer.EnableJoining;
        response = $"Test server joining was {(JoinTestServer.EnableJoining ? "enabled." : "disabled.")}";
        return true;
    }

    /// <inheritdoc/>
    public string GetHelp(ArraySegment<string> arguments)
    {
        return this.HelpProviderFormat("Execute the command to toggle test server joining.");
    }
}
