namespace BananaPlugin.Commands.Main.Information;

using BananaPlugin.Extensions;
using CommandSystem;
using NorthwoodLib.Pools;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

/// <summary>
/// The information subcommand that displays build information.
/// </summary>
public sealed class Build : ICommand, IUsageProvider, IHelpProvider
{
    /// <inheritdoc/>
    public string Command => "build";

    /// <inheritdoc/>
    public string[] Aliases => Array.Empty<string>();

    /// <inheritdoc/>
    public string Description => "Displays banana plugin build information.";

    /// <inheritdoc/>
    public string[] Usage { get; } = Array.Empty<string>();

    /// <inheritdoc/>
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [NotNull] out string? response)
    {
        if (!Plugin.AssertEnabled(out response))
        {
            return false;
        }

        StringBuilder strBuilder = StringBuilderPool.Shared.Rent();

        strBuilder.AppendLine("CURRENT BUILD VERSION: " + Versioning.FullVersionString + "\n");

        strBuilder.AppendLine("DEBUG BUILD: " + (Versioning.DebugBuild ? "TRUE" : "FALSE"));
        strBuilder.AppendLine("LOCAL BUILD: " + (Versioning.LocalBuild ? "TRUE" : "FALSE"));
        strBuilder.AppendLine("RELEASE BUILD: " + (Versioning.ReleaseBuild ? "TRUE\n" : "FALSE\n"));

        if (!Versioning.GetChangelog(Versioning.FullVersionString, out string? changelog))
        {
            strBuilder.AppendLine("Failed to obtain build changelogs.");
        }
        else
        {
            strBuilder.AppendLine("<b><u>Changelogs:</u></b>\n" + changelog);
        }

        response = StringBuilderPool.Shared.ToStringReturn(strBuilder);
        return true;
    }

    /// <inheritdoc/>
    public string GetHelp(ArraySegment<string> arguments)
    {
        return this.HelpProviderFormat("Running the command returns some basic information about the curernt build.");
    }
}
