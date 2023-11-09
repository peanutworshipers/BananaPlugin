// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         InvalidPermissionException.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 6:23 PM
//    Created Date:     11/08/2023 6:23 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands.Arguments.Exceptions;

using System;

/// <summary>
/// Thrown when a player doesn't have permission to execute a command.
/// </summary>
// ReSharper disable MemberCanBePrivate.Global
public sealed class InvalidPermissionsException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPermissionsException"/> class.
    /// </summary>
    /// <param name="permission">The permission that the player is missing.</param>
    public InvalidPermissionsException(string permission)
    {
        this.Permission = permission;
    }

    /// <summary>
    /// Gets the permission necessary to execute the given action.
    /// </summary>
    public string Permission { get; private set; }

    /// <inheritdoc/>
    public override string Message => $"You do not have permissions to use this command. You need the {this.Permission} permission.";
}