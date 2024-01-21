// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         BananaPermission.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 4:00 PM
//    Created Date:     11/08/2023 4:00 PM
// -----------------------------------------

namespace BananaPlugin.API.Main;

using Collections;
using Interfaces;

/// <summary>
/// The implementation for banana permissions.
/// </summary>
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PublicConstructorInAbstractClass
// ReSharper disable MemberCanBeProtected.Global
public abstract class BananaPermission : IPrefixableItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BananaPermission"/> class.
    /// </summary>
    public BananaPermission()
    {
        this.InheritingRoles = new RoleCollection();
        this.ChildPermissions = new PermissionCollection();
    }

    /// <summary>
    /// Gets the name of the permission.
    /// </summary>
    public abstract string PermissionsName { get; }

    /// <inheritdoc/>
    public string Prefix => this.PermissionsName;

    /// <summary>
    /// Gets the list of roles that inherit this permission.
    /// </summary>
    public RoleCollection InheritingRoles { get; }

    /// <summary>
    /// Gets the child instances of this permission (if they exist).
    /// </summary>
    public PermissionCollection ChildPermissions { get; }
}

/// <inheritdoc />
public abstract class BananaChildPermission<TParentPermission> : BananaPermission
    where TParentPermission : BananaPermission
{
    /// <summary>
    /// Gets the parent permission.
    /// </summary>
    public TParentPermission ParentPermission { get; internal set; } = null!;
}