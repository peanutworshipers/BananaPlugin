// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         CommandArgumentAttribute.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 3:15 PM
//    Created Date:     11/08/2023 3:14 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands.Arguments;

using System;
using System.Collections.Generic;
using System.Linq;

/// <inheritdoc />
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class CommandArgumentAttribute<T> : CommandArgumentAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandArgumentAttribute{T}"/> class.
    /// </summary>
    /// <param name="name">
    ///     <inheritdoc cref="ICommandArgument.Name"/>
    /// </param>
    /// <param name="description">
    ///     <inheritdoc cref="ICommandArgument.Description"/>
    /// </param>
    /// <param name="isRequired">
    ///     <inheritdoc cref="ICommandArgument.IsRequired"/>
    /// </param>
    /// <param name="isRemainder">
    ///     <inheritdoc cref="ICommandArgument.IsRemainder"/>
    /// </param>
    /// <param name="usagePath">
    ///     <inheritdoc cref="ICommandArgument.UsagePath"/>
    /// </param>
    public CommandArgumentAttribute(string name, string description, bool isRequired = false, bool isRemainder = false, int usagePath = -1)
        : base(typeof(T), name, description, isRequired, isRemainder, usagePath)
    {
    }
}

/// <summary>
/// An attribute instance of a command argument.
/// </summary>
public class CommandArgumentAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandArgumentAttribute"/> class.
    /// </summary>
    /// <param name="type">
    ///     <inheritdoc cref="ICommandArgument.Type"/>
    /// </param>
    /// <param name="name">
    ///     <inheritdoc cref="ICommandArgument.Name"/>
    /// </param>
    /// <param name="description">
    ///     <inheritdoc cref="ICommandArgument.Description"/>
    /// </param>
    /// <param name="isRequired">
    ///     <inheritdoc cref="ICommandArgument.IsRequired"/>
    /// </param>
    /// <param name="isRemainder">
    ///     <inheritdoc cref="ICommandArgument.IsRemainder"/>
    /// </param>
    /// <param name="usagePath">
    ///     <inheritdoc cref="ICommandArgument.UsagePath"/>
    /// </param>
    // ReSharper disable once MemberCanBeProtected.Global
    public CommandArgumentAttribute(Type type, string name, string description, bool isRequired = false, bool isRemainder = false, int usagePath = -1)
    {
        this.Arguments = new List<CommandArgument> { new CommandArgument(type, name, description, isRequired, isRemainder, usagePath) };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandArgumentAttribute"/> class.
    /// </summary>
    /// <param name="arguments">A list of <see cref="CommandArgument"/> to use.</param>
    public CommandArgumentAttribute(params CommandArgument[] arguments)
    {
        this.Arguments = arguments.ToList();
    }

    /// <summary>
    /// Gets a list of CommandArguments.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public List<CommandArgument> Arguments { get; init; }
}
