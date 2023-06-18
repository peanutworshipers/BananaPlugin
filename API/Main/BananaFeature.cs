namespace BananaPlugin.API.Main;

using BananaPlugin.API.Utils;
using BananaPlugin.Commands;
using BananaPlugin.Extensions;
using CommandSystem;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using static BananaPlugin.Features.AutoNuke;

/// <summary>
/// The main feature implementation.
/// </summary>
public abstract class BananaFeature
{
    private bool enabled = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="BananaFeature"/> class.
    /// </summary>
    protected BananaFeature()
    {
        MainCommand.OnAssigned += this.RegisterCommands;

        this.Command = new(this);
    }

    /// <summary>
    /// Gets the name of the feature.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the prefix of the feature.
    /// </summary>
    public abstract string Prefix { get; }

    /// <summary>
    /// Gets the commands for this feature.
    /// </summary>
    public virtual ICommand[] Commands => Array.Empty<ICommand>();

    /// <summary>
    /// Gets or sets a value indicating whether the feature is enabled.
    /// </summary>
    public bool Enabled
    {
        get => this.enabled;
        set
        {
            // Check if setting same value.
            if (this.enabled == value)
            {
                return;
            }

            try
            {
                if (value)
                {
                    this.Enable();

                    BPLogger.Info($"Feature '{this.Name}' was enabled!");
                }
                else
                {
                    this.Disable();

                    BPLogger.Info($"Feature '{this.Name}' was disabled!");
                }

                this.enabled = value;
            }
            catch (Exception e)
            {
                BPLogger.Error($"Failed setting enabled to {value}" + e);
                throw;
            }
        }
    }

    /// <summary>
    /// Gets the feature command associated with this feature.
    /// </summary>
    public FeatureCommand Command { get; }

    /// <summary>
    /// Enables the feature.
    /// </summary>
    protected abstract void Enable();

    /// <summary>
    /// Disables the feature.
    /// </summary>
    protected abstract void Disable();

    private void RegisterCommands(MainCommand command)
    {
        command.UnregisterCommand(this.Command);
        command.RegisterCommand(this.Command);

        ICommand[] cmds = this.Commands;

        for (int i = 0; i < cmds.Length; i++)
        {
            this.Command.UnregisterCommand(cmds[i]);
            this.Command.RegisterCommand(cmds[i]);
        }
    }

    /// <summary>
    /// Acts as the parent command for feature commands.
    /// </summary>
    public sealed class FeatureCommand : ParentCommand, IUsageProvider, IHelpProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureCommand"/> class.
        /// </summary>
        /// <param name="feature">The feature attached to this command.</param>
        public FeatureCommand(BananaFeature feature)
        {
            this.Command = feature.Prefix;
            this.Description = $"Main command for feature: [{feature.Name}]";
        }

        /// <inheritdoc/>
        public override string Command { get; }

        /// <inheritdoc/>
        public override string[] Aliases => Array.Empty<string>();

        /// <inheritdoc/>
        public override string Description { get; }

        /// <inheritdoc/>
        public string[] Usage { get; } = new string[]
        {
            "subcommand",
        };

        /// <inheritdoc/>
        public override void LoadGeneratedCommands()
        {
        }

        /// <inheritdoc/>
        public string GetHelp(ArraySegment<string> arguments)
        {
            return this.HelpProviderFormat("Provide a subcommand you wish to execute.");
        }

        /// <inheritdoc/>
        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [NotNullWhen(true)] out string? response)
        {
            return Plugin.AssertEnabled(out response)
                && this.InvalidSubcommandFormat(out response);
        }
    }
}
