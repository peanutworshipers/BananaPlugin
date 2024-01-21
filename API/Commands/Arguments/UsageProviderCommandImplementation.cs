// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         UsageProviderCommandImplementation.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 7:33 PM
//    Created Date:     11/08/2023 7:33 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands.Arguments;

using System;
using System.Collections.Generic;
using System.Reflection;
using CommandSystem;
using Main;

#pragma warning disable SA1648
/// <inheritdoc cref="CommandImplementation" />
public class UsageProviderCommandImplementation : CommandImplementation, IUsageProvider
{
    /// <inheritdoc />
    public UsageProviderCommandImplementation(BananaCommandAttribute cmd, MethodInfo method, List<BananaPermission>? requiredPermissions = null, object? instance = null)
        : base(cmd, method, requiredPermissions, instance)
    {
        this.Usage = Array.Empty<string>();
    }

    /// <inheritdoc />
    public string[] Usage { get; }
}