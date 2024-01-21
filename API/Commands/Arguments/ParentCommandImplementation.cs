// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         ParentCommandImplementation.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 6:32 PM
//    Created Date:     11/08/2023 6:32 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands.Arguments;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CommandSystem;
using RemoteAdmin;
using Utils;

#pragma warning disable SA1011

/// <inheritdoc />
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ConvertToAutoProperty
// [CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class ParentCommandImplementation : ParentCommand
{
    private bool usageProviderFriendly;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParentCommandImplementation"/> class.
    /// </summary>
    /// <param name="cmd">The represented command instance.</param>
    /// <param name="method">The method that will be called when executing this command instance.</param>
    /// <param name="parent">The parent command instance of this command (if it has one).</param>
    /// <param name="instance">The instance that will be used to invoke the method, if the method is not static.</param>
    public ParentCommandImplementation(BananaParentCommandAttribute cmd, ParentCommandImplementation? parent = null, MethodInfo? method = null, object? instance = null)
    {
        this.Command = cmd.Name;
        this.Description = cmd.Description;
        this.Aliases = cmd.Aliases;
        this.usageProviderFriendly = cmd.UsageProviderFriendly;
        this.Method = method;
        this.ParentCommand = parent;
        this.Instance = instance;
    }

    /// <inheritdoc/>
    public override string Command { get; }

    /// <inheritdoc/>
    public override string[] Aliases { get; }

    /// <inheritdoc/>
    public override string Description { get; }

    /// <summary>
    /// Gets the method that will be invoked when the command is run. If this is null, the default method will be run.
    /// </summary>
    public MethodInfo? Method { get; }

    /// <summary>
    /// Gets the instance that will be invoked when the command is run. This will be null if the method is static.
    /// </summary>
    public object? Instance { get; }

    /// <summary>
    /// Gets the parent command instance of this command.
    /// </summary>
    private ParentCommandImplementation? ParentCommand { get; }

    /// <inheritdoc/>
    public override void LoadGeneratedCommands()
    {
    }

    /// <summary>
    /// Registers the command and subcommands.
    /// </summary>
    /// <param name="name">Recursive name iteration.</param>
    /// <param name ="primaryCommand">The value of the primary parent command path (if present).</param>
    public void RegisterCommandSequences(List<string>? name = null, string primaryCommand = "")
    {
        if (this.Command == string.Empty)
        {
            BPLogger.Error("Command cannot be registered, as it has no name!");
            return;
        }

        if (this.Method is not null && !this.Method.IsStatic && this.Instance is null)
        {
            BPLogger.Error($"Command '{this.Command}' cannot be registered. The method is non-static and the instance is missing!");
            return;
        }

        if (!this.usageProviderFriendly && name == null)
        {
            CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(this);
            return;
        }

        primaryCommand = (primaryCommand == string.Empty ? "_" : string.Empty) + this.Command;
        List<string> newValues = new();
        name = name ?? new();
        if (name.Count < 1)
        {
            // Dont add the primary key - we want to pass this separately so it can be used as the main command name.
            // name.Add(this.Command);
            foreach (string alias in this.Aliases)
            {
                name.Add(alias);
            }
        }
        else
        {
            foreach (string parentCmd in name)
            {
                newValues.Add(parentCmd + $"_{this.Command}");
                foreach (string alias in this.Aliases)
                {
                    newValues.Add(parentCmd + $"_{alias}");
                }
            }

            name = newValues;
        }

        foreach (KeyValuePair<string, ICommand> command in this.Commands)
        {
            if (command.Value is CommandImplementation cmd)
            {
                cmd.RegisterUsageProviderSafe(name, primaryCommand);
            }
            else if (command.Value is ParentCommandImplementation parent)
            {
                parent.RegisterCommandSequences(name, primaryCommand);
            }
            else
            {
                BPLogger.Warn($"Subcommand \'{command.Value.Command}\' is not a {nameof(CommandImplementation)}, so it will be skipped. [{this.Command}]");
            }
        }
    }

    /// <inheritdoc/>
    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        if (this.Method is not null)
        {
            ArgumentResultCollection results = new (sender, -1, arguments.ToList());
            results.MarkAsLoaded();
            CommandResponse cmdResponse = new (results);
            try
            {
                this.Method.Invoke(this.Instance!, new object[] { cmdResponse, });
            }
            catch (Exception e)
            {
                response = "An error has occured while executing this command.";
                BPLogger.Warn($"Caught an exception while executing the command '{this.Command}' -> {this.Method.DeclaringType?.FullName}.{this.Method.Name}(CommandResponse)");
                if (e is TargetInvocationException invoc)
                {
                    BPLogger.Debug($"Exception: \n{invoc.InnerException}");
                }
                else
                {
                    BPLogger.Debug($"Exception: \n{e}");
                }

                return false;
            }

            response = cmdResponse.Result;
            return cmdResponse.Success;
        }

        if (arguments.Count < 1)
        {
            response = "Please specify a subcommand.";
        }
        else
        {
            response = $"Could not find command '{arguments.At(0)}'";
        }

        response += "Available Sub-Commands: ";
        if (this.Commands.Count < 1)
        {
            response += " - [None]";
        }

        foreach (ICommand command in this.Commands.Values)
        {
            string usage = string.Empty;
            if (command is IUsageProvider provider && provider.Usage.Length > 0)
            {
                foreach (string usg in provider.Usage)
                {
                    usage += $" [{usg}]";
                }
            }

            response += $" - {command.Command} {usage} {(command.Description == string.Empty ? string.Empty : $"- {command.Description}")}";
        }

        return false;
    }
}