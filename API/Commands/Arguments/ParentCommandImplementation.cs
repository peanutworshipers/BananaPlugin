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
using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Main;
using Utils;

#pragma warning disable SA1011

/// <inheritdoc />
// ReSharper disable ConvertToAutoProperty
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class ParentCommandImplementation : ParentCommand
{
    private bool usageProviderFriendly;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParentCommandImplementation"/> class.
    /// </summary>
    /// <param name="cmd">The represented command instance.</param>
    public ParentCommandImplementation(BananaParentCommandAttribute cmd, ParentCommandImplementation? parent = null)
    {
        this.Command = cmd.Name;
        this.Description = cmd.Description;
        this.Aliases = cmd.Aliases;
        this.usageProviderFriendly = cmd.UsageProviderFriendly;
        this.ParentCommand = parent;
    }

    private ParentCommandImplementation? ParentCommand { get; }

    /// <inheritdoc/>
    public override string Command { get; }

    /// <inheritdoc/>
    public override string[] Aliases { get; }

    /// <inheritdoc/>
    public override string Description { get; }

    /// <inheritdoc/>
    public override void LoadGeneratedCommands()
    {
    }

    /// <summary>
    /// Registers the command and subcommands.
    /// </summary>
    /// <param name="name">Recursive name iteration.</param>
    public void RegisterCommandSequences(List<string>? name = null, string primaryCommand = "")
    {
        if (this.Command == string.Empty)
        {
            BPLogger.Error("Command cannot be registered, as it has no name!");
            return;
        }

        if (!this.usageProviderFriendly && name == null)
        {
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
                continue;
            }
        }
    }

    /// <inheritdoc/>
    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        response = $"Couldn't find that command!";
        return false;
    }
}