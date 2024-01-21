// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         CommandArgument.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 3:16 PM
//    Created Date:     11/08/2023 3:16 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands.Arguments;

using System;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ArrangeModifiersOrder
// ReSharper disable UnusedAutoPropertyAccessor.Global

/// <summary>
/// Gets information about the arguments required for a command.
/// </summary>
public class CommandArgument : ICommandArgument
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandArgument"/> class.
    /// </summary>
    /// <param name="type">
    ///     <inheritdoc cref="Type"/>
    /// </param>
    /// <param name="name">
    ///     <inheritdoc cref="Name"/>
    /// </param>
    /// <param name="description">
    ///     <inheritdoc cref="Description"/>
    /// </param>
    /// <param name="isRequired">
    ///     <inheritdoc cref="IsRequired"/>
    /// </param>
    /// <param name="isRemainder">
    ///     <inheritdoc cref="IsRemainder"/>
    /// </param>
    /// <param name="usagePath">
    ///     <inheritdoc cref="UsagePath"/>
    /// </param>
    public CommandArgument(Type type, string name, string description, bool isRequired = false, bool isRemainder = false, int usagePath = -1)
    {
        this.Type = type;
        this.Name = name;
        this.Description = description;
        this.IsRequired = isRequired;
        this.IsRemainder = isRemainder;
        this.UsagePath = usagePath;
    }

    /// <inheritdoc/>
    public Type Type { get; init; }

    /// <inheritdoc/>
    public string Name { get; init; }

    /// <inheritdoc/>
    public string Description { get; init; }

    /// <inheritdoc/>
    public bool IsRequired { get; init; }

    /// <inheritdoc/>
    public bool IsRemainder { get; init; }

    /// <inheritdoc/>
    public int UsagePath { get; init; }
}