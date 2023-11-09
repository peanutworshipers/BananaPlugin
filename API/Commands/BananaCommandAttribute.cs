// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         BananaCommandAttribute.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 3:11 PM
//    Created Date:     11/08/2023 3:11 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands;

using System;
using System.Collections.Generic;
using Arguments;

/// <summary>
/// Used to define a command via attributes.
/// </summary>
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
public class BananaCommandAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BananaCommandAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <param name="description">The description of the command.</param>
    /// <param name="aliases">The aliases for the command.</param>
    public BananaCommandAttribute(string name, string description, params string[] aliases)
    {
        this.Name = name;
        this.Description = description;
        this.Aliases = aliases;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BananaCommandAttribute"/> class.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    /// <param name="description">The description of the command.</param>
    public BananaCommandAttribute(string name, string description)
    {
        this.Name = name;
        this.Description = description;
        this.Aliases = Array.Empty<string>();
    }

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    public string Description { get; init; }

    /// <summary>
    /// Gets the aliases for the command.
    /// </summary>
    public string[] Aliases { get; init; }

    /// <summary>
    /// Gets or sets the required <see cref="CommandArguments"/> in correspondence to the proper usage path. />
    /// </summary>
    public Dictionary<int, List<CommandArgument>> CommandArguments { get; set; } = new ();
}