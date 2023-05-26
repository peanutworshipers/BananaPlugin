namespace BananaPlugin.Commands.Main.Information;

using BananaPlugin.Extensions;
using CommandSystem;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The command responsible for handling changelog queries.
/// </summary>
public sealed class Changelog : ICommand, IUsageProvider, IHelpProvider
{
    /// <inheritdoc/>
    public string Command => "changelog";

    /// <inheritdoc/>
    public string[] Aliases { get; } = new string[]
    {
        "changelogs",
        "clog",
    };

    /// <inheritdoc/>
    public string Description => "Displays the changelog for a given version of the banana plugin.";

    /// <inheritdoc/>
    public string[] Usage => new string[]
    {
        "version",
    };

    /// <inheritdoc/>
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [NotNull] out string? response)
    {
        if (!Plugin.AssertEnabled(out response))
        {
            return false;
        }

        if (arguments.Count == 0)
        {
            response = "You must provide a version to query.";
            return false;
        }

        string query = arguments.At(0);

        if (query.ToLower() == "current")
        {
            query = Versioning.VersionString;
        }

        if (!Versioning.GetChangelog(query, out string? changelog))
        {
            response = $"Version '{query}' could not be found.";
            return false;
        }

        response = $"\n<b><u>Changelogs for version '{query}'</u></b>\n{changelog}";
        return true;
    }

    /// <inheritdoc/>
    public string GetHelp(ArraySegment<string> arguments)
    {
        return this.HelpProviderFormat("Provide a valid version and this command will return the changelogs.");
    }
}
