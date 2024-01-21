// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         BananaCommand.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 3:01 PM
//    Created Date:     11/08/2023 3:01 PM
// -----------------------------------------

namespace BananaPlugin.API.Main;

using System;
using System.Diagnostics.CodeAnalysis;
using CommandSystem;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable once UnusedType.Global
#pragma warning disable SA1201
/// <summary>
/// The base parent command.
/// </summary>
public class BananaParentCommand : ParentCommand
{
    /// <inheritdoc/>
    public override void LoadGeneratedCommands()
    {
    }

    /// <inheritdoc/>
    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, [UnscopedRef] out string response)
    {
        response = "Could not find x";
        return false;
    }

    private string command = string.Empty;

    /// <inheritdoc/>
    public override string Command => this.command;

    private string[] aliases = Array.Empty<string>();

    /// <inheritdoc/>
    public override string[] Aliases => this.aliases;

    private string description = string.Empty;

    /// <inheritdoc/>
    public override string Description => this.description;
}