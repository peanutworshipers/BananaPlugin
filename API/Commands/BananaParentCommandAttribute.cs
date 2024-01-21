// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         BananaParentCommand.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 4:58 PM
//    Created Date:     11/08/2023 4:58 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands;

using System;

/// <summary>
/// Used to define a parent command via attributes.
/// </summary>
// ReSharper disable once UnusedType.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CommentTypo
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
#pragma warning disable SA1625
public class BananaParentCommandAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BananaParentCommandAttribute"/> class.
    /// </summary>
    /// <param name="name">
    ///     <inheritdoc cref="UsageProviderFriendly"/>
    /// </param>
    /// <param name="description">
    ///     <inheritdoc cref="Description"/>
    /// </param>
    /// <param name="aliases">
    ///     <inheritdoc cref="Aliases"/>
    /// </param>
    /// <param name="usageProviderFriendly">
    ///     <inheritdoc cref="UsageProviderFriendly"/>
    /// </param>
    public BananaParentCommandAttribute(string name, string description, bool usageProviderFriendly = true, params string[] aliases)
    {
        this.Name = name;
        this.Description = description;
        this.Aliases = aliases;
        this.UsageProviderFriendly = usageProviderFriendly;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BananaParentCommandAttribute"/> class.
    /// </summary>
    /// <param name="name">
    ///     <inheritdoc cref="UsageProviderFriendly"/>
    /// </param>
    /// <param name="description">
    ///     <inheritdoc cref="Description"/>
    /// </param>
    /// <param name="usageProviderFriendly">
    ///     <inheritdoc cref="UsageProviderFriendly"/>
    /// </param>
    public BananaParentCommandAttribute(string name, string description, bool usageProviderFriendly = true)
    {
        this.Name = name;
        this.Description = description;
        this.Aliases = Array.Empty<string>();
        this.UsageProviderFriendly = usageProviderFriendly;
    }

    /// <inheritdoc cref="ParentCommand.Command"/>
    public string Name { get; init; }

    /// <inheritdoc cref="ParentCommand.Description"/>
    public string Description { get; init; }

    /// <inheritdoc cref="ParentCommand.Aliases"/>
    public string[] Aliases { get; init; }

    /// <summary>
    /// Gets a value indicating whether subcommands should be registered as normal commands with the parent command name separated by an underscore.
    /// </summary>
    /// <example>
    /// <code>
    /// Command: parent_subcommand
    /// - playertools_nickname
    /// - playertools_scale
    /// - playertools_warn
    /// </code>
    /// </example>
    public bool UsageProviderFriendly { get; init; }
}