// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         BananaParentCommandExecutorAttribute.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/10/2023 3:14 PM
//    Created Date:     11/10/2023 3:14 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands;

using System;

/// <summary>
/// Used to define an execution method to invoke when a parent command gets called.
/// The method this is on must be directly inside of the type that has the parent command attribute.
/// You cannot use roles, permissions, or argument paths with this currently.
/// Only one <see cref="BananaParentCommandExecutorAttribute"/> can be used for a parent command.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class BananaParentCommandExecutorAttribute : Attribute
{
}