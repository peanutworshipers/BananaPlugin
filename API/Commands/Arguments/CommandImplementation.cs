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
using CommandSystem;
using Main;

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
    /// <param name="requiredPermissions">The permissions required to execute this command.</param>
    public CommandImplementation(BananaCommandAttribute cmd, List<BananaPermission>? requiredPermissions = null)
    {
        this.command = cmd.Name;
        this.Description = cmd.Description;
        this.aliases = cmd.Aliases;
        this.RequiredPermissions = requiredPermissions ?? new List<BananaPermission>();
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
    internal void RegisterUsageProviderSafe(List<string> parentAliases, string primary)
    {
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
    }
}
