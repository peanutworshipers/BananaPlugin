namespace BananaPlugin.Commands.Main;

using BananaPlugin.Extensions;
using CommandSystem;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The command responsible for retrieving information about the banana plugin.
/// </summary>
public sealed class InformationCmd : ParentCommand, IUsageProvider, IHelpProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InformationCmd"/> class.
    /// </summary>
    public InformationCmd() => this.LoadGeneratedCommands();

    /// <inheritdoc/>
    public override string Command => "information";

    /// <inheritdoc/>
    public override string[] Aliases { get; } = new string[]
    {
        "info",
        "inf",
    };

    /// <inheritdoc/>
    public override string Description => "Displays information about the current banana plugin build.";

    /// <inheritdoc/>
    public string[] Usage { get; } = new string[]
    {
        "build/changelog/versions",
    };

    /// <inheritdoc/>
    public string GetHelp(ArraySegment<string> arguments)
    {
        return this.HelpProviderFormat("Use the build or changelog subcommand to view information regarding the current or previous builds.");
    }

    /// <inheritdoc/>
    public override void LoadGeneratedCommands()
    {
        this.RegisterCommand(new Information.Build());
        this.RegisterCommand(new Information.Changelog());
        this.RegisterCommand(new Information.Versions());
    }

    /// <inheritdoc/>
    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [NotNull] out string? response)
    {
        return Plugin.AssertEnabled(out response)
            && this.InvalidSubcommandFormat(out response);
    }
}
