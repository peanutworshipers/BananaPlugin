// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         BananaRole.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 4:00 PM
//    Created Date:     11/08/2023 4:00 PM
// -----------------------------------------

namespace BananaPlugin.API.Main;

using System.Collections.Generic;
using Collections;
using Interfaces;

/// <summary>
/// The implementation for banana roles.
/// </summary>
public abstract class BananaRole : IPrefixableItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BananaRole"/> class.
    /// </summary>
    public BananaRole()
    {
        this.PermissionCollection = new PermissionCollection();
    }

    /// <summary>
    /// Gets the Id of the role.
    /// </summary>
    public abstract string RoleId { get; }

    /// <summary>
    /// Gets the permissions the role has.
    /// </summary>
    public PermissionCollection PermissionCollection { get; internal set; }

    /// <summary>
    /// Gets the types of permissions this role should have.
    /// </summary>
    public abstract List<Type> DefinedPermissions { get; }

    /// <inheritdoc/>
    public string Prefix => this.RoleId;
}