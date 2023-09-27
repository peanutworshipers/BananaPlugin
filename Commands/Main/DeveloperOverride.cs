namespace BananaPlugin.Commands.Main;

using BananaPlugin.API;
using BananaPlugin.API.Interfaces;
using BananaPlugin.API.Main;
using BananaPlugin.Extensions;
using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The command responsible for applying developer mode for developers.
/// </summary>
public sealed class DeveloperOverride : ICommand, IUsageProvider, IHelpProvider, IHiddenCommand, IRequiresRank
{
    /// <inheritdoc/>
    public string Command => "developeroverride";

    /// <inheritdoc/>
    public string[] Aliases { get; } =
    [
        "doverride",
    ];

    /// <inheritdoc/>
    public string Description => "Enables developer override mode for you.";

    /// <inheritdoc/>
    public string[] Usage => Array.Empty<string>();

    /// <inheritdoc/>
    public BRank RankRequirement => BRank.Developer;

    /// <inheritdoc/>
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [NotNull] out string? response)
    {
        // Make sure plugin is enabled.
        if (!Plugin.AssertEnabled(out response))
        {
            return false;
        }

        if (!sender.CheckPermission(BRank.Developer, out response))
        {
            return false;
        }

        switch (sender)
        {
            case DeveloperCommandSender dSender2:

                bool newValue = !dSender2.DeveloperModeActive;

                dSender2.DeveloperModeActive = newValue;

                response = $"\nDeveloper access {(newValue ? "granted to" : "revoked from")} you.";
                return true;

            case PlayerCommandSender pSender:

                QueryProcessor proc = pSender.ReferenceHub.queryProcessor;
                DeveloperCommandSender dSender = new (proc._sender);
                proc._sender = dSender;

                dSender.DeveloperModeActive = true;

                response = "\nDeveloper access granted to you.";
                return true;

            default:

                response = "\nYou must be a player to use this command.";
                return false;
        }
    }

    /// <inheritdoc/>
    public string GetHelp(ArraySegment<string> arguments)
    {
        return this.HelpProviderFormat("Enables developer mode when executed.");
    }
}
