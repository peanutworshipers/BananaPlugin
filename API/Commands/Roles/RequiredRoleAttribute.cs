// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         RequiredRoleAttribute.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 3:11 PM
//    Created Date:     11/08/2023 3:11 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands.Roles;

using System;

/// <summary>
/// Indicates that a role is require to use a feature.
/// </summary>
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class RequiredRoleAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredRoleAttribute"/> class.
    /// </summary>
    /// <param name="type">The role that has permission to use this command.</param>
    public RequiredRoleAttribute(Type type)
    {
        this.Roles = new[] { type };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredRoleAttribute"/> class.
    /// </summary>
    /// <param name="type">An array of roles that has permission to use this command.</param>
    public RequiredRoleAttribute(params Type[] type)
    {
        this.Roles = type;
    }

    /// <summary>
    /// Gets the types of the roles that are required to use this item.
    /// </summary>
    public Type[] Roles { get; }
}