namespace BananaPlugin.Commands;

using BananaPlugin.API.Utils;
using BananaPlugin.Commands.Main;
using BananaPlugin.Extensions;
using CommandSystem;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The main command for this assembly.
/// </summary>
[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(ClientCommandHandler))]
public sealed class MainCommand : ParentCommand, IUsageProvider, IHelpProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainCommand"/> class.
    /// </summary>
    public MainCommand()
    {
        this.LoadGeneratedCommands();

        Instance = this;

        try
        {
            OnAssigned?.Invoke(this);
        }
        catch (Exception ex)
        {
            BPLogger.Error("Failed to invoke OnAssigned delegate.\n" + ex.ToString());
        }
    }

    /// <summary>
    /// Called when a new command instance is assigned.
    /// </summary>
    internal static event Action<MainCommand>? OnAssigned;

    /// <summary>
    /// Gets the currently active instance of the main banana plugin command.
    /// </summary>
    public static MainCommand? Instance { get; private set; }

    /// <inheritdoc/>
    public override string Command => "bananaplugin";

    /// <inheritdoc/>
    public override string[] Aliases => new string[]
    {
        "bp",
    };

    /// <inheritdoc/>
    public override string Description => "The main command for banana plugin.";

    /// <inheritdoc/>
    public string[] Usage { get; } = new string[]
    {
        "subcommand",
    };

    /// <inheritdoc/>
    public string GetHelp(ArraySegment<string> arguments)
    {
        return this.HelpProviderFormat("Provide a subcommand you wish to execute.");
    }

    /// <inheritdoc/>
    public override void LoadGeneratedCommands()
    {
        this.RegisterCommand(new DeveloperOverride());

        this.RegisterCommand(new EnableOrDisable(true));
        this.RegisterCommand(new EnableOrDisable(false));
        this.RegisterCommand(new ShowFeatures());
        this.RegisterCommand(new InformationCmd());
        this.RegisterCommand(new JoinTestServer());
        this.RegisterCommand(new ToggleTestServerJoining());

#if DEBUG
        this.RegisterCommand(new TestCmd());
#endif
    }

    /// <inheritdoc/>
    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [NotNull] out string? response)
    {
        return Plugin.AssertEnabled(out response)
            && this.InvalidSubcommandFormat(out response);
    }
}
