// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         ICommandArgument.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 3:30 PM
//    Created Date:     11/08/2023 3:30 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands.Arguments;

using System;

/// <summary>
/// The base interface for command arguments.
/// </summary>
public interface ICommandArgument
{
    /// <summary>
    /// Gets the type of the argument.
    /// </summary>
    public Type Type { get; init; }

    /// <summary>
    /// Gets the name of the argument.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the description of the argument.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether the argument is required or not required.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Gets a value indicating whether the argument should be given a remainder of the properties or not.
    /// </summary>
    public bool IsRemainder { get; init; }

    /// <summary>
    /// Gets a value which represents the id of the usage path.
    /// Usage paths can be used to specify different overloads of command arguments for a command.
    /// </summary>
    /// <example>
    /// <code>
    /// 1 - [Arg 1 - Required] [Arg 2]
    /// 2 - [Arg 1]
    /// </code>
    /// Both examples would be considered as valid options for players that are using the command.
    /// </example>
    public int UsagePath { get; init; }
}