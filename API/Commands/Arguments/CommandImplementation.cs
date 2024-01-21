// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         CommandImplementation.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 6:44 PM
//    Created Date:     11/08/2023 6:44 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands.Arguments;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CommandSystem;
using Main;
using RemoteAdmin;
using Utils;

#pragma warning disable SA1011
#pragma warning disable SA1401
/// <summary>
/// The default implementation of a <see cref="BananaCommandAttribute"/>.
/// </summary>
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class CommandImplementation : ICommand
{
    /// <summary>
    /// A list of the aliases.
    /// </summary>
    protected string[] aliases;

    /// <summary>
    /// The name of the command.
    /// </summary>
    protected string command;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandImplementation"/> class.
    /// </summary>
    /// <param name="cmd">The base <see cref="BananaCommandAttribute"/> this command represents.</param>
    /// <param name="method">The method that will be invoked when the command is run.</param>
    /// <param name="requiredPermissions">The permissions required to execute this command.</param>
    /// <param name="instance">The instance of the object to use when invoking the method. If this is null, it is assumed that the method is static.</param>
    public CommandImplementation(BananaCommandAttribute cmd, MethodInfo method, List<BananaPermission>? requiredPermissions = null, object? instance = null)
    {
        this.command = cmd.Name;
        this.Description = cmd.Description;
        this.aliases = cmd.Aliases;
        this.Method = method;
        this.RequiredPermissions = requiredPermissions ?? new List<BananaPermission>();
        this.Instance = instance;
    }

    /// <summary>
    /// Gets or sets a list of permissions required to execute the command.
    /// </summary>
    public List<BananaPermission> RequiredPermissions { get; protected set; }

    /// <inheritdoc/>
    public string Command => this.command;

    /// <inheritdoc/>
    public string[] Aliases => this.aliases;

    /// <inheritdoc/>
    public string Description { get; }

    /// <summary>
    /// Gets the instance for the class if it is non-static. If the method is static, this will be null.
    /// </summary>
    public object? Instance { get; init; }

    /// <summary>
    /// Gets the method to be executed.
    /// </summary>
    public MethodInfo Method { get; init; }

    /// <inheritdoc/>
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        response = "Could not execute command.";
        return false;
    }

    /// <summary>
    /// Registers in a usage provider safe manner.
    /// </summary>
    /// <param name="parentAliases">A list of parent command aliases to implement.</param>
    /// <param name="primary">The primary interpolated name of all parent commands.</param>
    internal virtual void RegisterUsageProviderSafe(List<string> parentAliases, string primary)
    {
        if (!this.Method.IsStatic && this.Instance is null)
        {
            BPLogger.Error($"Command '{this.Command}' cannot be registered. The method is non-static and the instance is missing!");
            return;
        }

        List<string> newAliases = new();
        this.command = primary + $"_{this.Command}";
        foreach(string parentAlias in parentAliases)
        {
            newAliases.Add(parentAlias + $"_{this.Command}");
            foreach (string alias in this.Aliases)
            {
                newAliases.Add(parentAlias + $"_{alias}");
            }
        }

        this.aliases = newAliases.ToArray();
        CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(this);
    }

    /// <summary>
    /// Used to register the command in the case that it is not using the usage provider system, or it is the only command, and is not use a parent command.
    /// </summary>
    internal void RegisterNonUsageProviderSafe()
    {
        if (!this.Method.IsStatic && this.Instance is null)
        {
            BPLogger.Error($"Command '{this.Command}' cannot be registered. The method is non-static and the instance is missing!");
            return;
        }

        CommandProcessor.RemoteAdminCommandHandler.RegisterCommand(this);
    }
}
