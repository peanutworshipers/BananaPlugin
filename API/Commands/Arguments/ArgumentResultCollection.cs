﻿// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         ArgumentResult.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 4:04 PM
//    Created Date:     11/08/2023 4:04 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands.Arguments;

using System.Collections.Generic;
using CommandSystem;

/// <summary>
/// A collection of <see cref="ArgumentResult"/>.
/// </summary>
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class ArgumentResultCollection : Collections.Collection<ArgumentResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentResultCollection"/> class.
    /// </summary>
    /// <param name="sender">The <see cref="ICommandSender"/> executing the command.</param>
    /// <param name="usagePath"><inheritdoc cref="CommandArgument.UsagePath"/></param>
    /// <param name="arguments">A list of the provided command arguments.</param>
    public ArgumentResultCollection(ICommandSender sender, int usagePath, List<string> arguments)
    {
        this.Sender = sender;
        this.UsagePath = usagePath;
        this.Arguments = arguments.AsReadOnly();
    }

    /// <summary>
    /// Gets the <see cref="ICommandSender"/> of the command.
    /// </summary>
    public ICommandSender Sender { get; init; }

    /// <inheritdoc cref="ICommandArgument.UsagePath" />
    public int UsagePath { get; init; }

    /// <summary>
    /// Gets a readonly list of the provided arguments.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; }
}