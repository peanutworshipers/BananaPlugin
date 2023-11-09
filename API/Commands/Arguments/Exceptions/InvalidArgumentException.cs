// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         InvalidArgumentException.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 6:22 PM
//    Created Date:     11/08/2023 6:22 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands.Arguments.Exceptions;

using System;

/// <summary>
/// Thrown when a player specifies an invalid argument while using a command.
/// </summary>
// ReSharper disable MemberCanBePrivate.Global
public sealed class InvalidArgumentException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidArgumentException"/> class.
    /// </summary>
    /// <param name="name">The name of the invalid argument.</param>
    /// <param name="argType">The Type of the invalid argument.</param>
    public InvalidArgumentException(string name, Type argType)
    {
        this.ArgumentName = name;
        this.ArgumentType = argType;
    }

    /// <summary>
    /// Gets the type of the invalid argument.
    /// </summary>
    public Type ArgumentType { get; private set; }

    /// <summary>
    /// Gets the name of the invalid argument.
    /// </summary>
    public string ArgumentName { get; private set; }

    /// <inheritdoc/>
    public override string Message => $"The value for argument {this.ArgumentName} is invalid. It must be a {this.ArgumentType.FullName}.";
}
