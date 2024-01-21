// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         Plugin.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/09/2023 10:48 AM
//    Created Date:     11/09/2023 10:48 AM
// -----------------------------------------

namespace BananaPlugin;

using System;
using API.Utils;
using Exiled.API.Enums;

/// <summary>
/// The main plugin for loading the banana api interface.
/// </summary>
// ReSharper disable ClassNeverInstantiated.Global
public sealed class Plugin : Exiled.API.Features.Plugin<Config>
{
    /// <inheritdoc/>
    public override string Author => "O5Zereth and Redforce04";

    /// <inheritdoc/>
    public override string Name => "BananaFramework";

    /// <inheritdoc/>
    public override string Prefix => "BP";

    /// <inheritdoc/>
    public override PluginPriority Priority => PluginPriority.Highest;

    /// <inheritdoc/>
    public override Version Version => Version.Parse("1.0.0");

    /// <inheritdoc/>
    public override void OnEnabled()
    {
        BPLogger.Debug($"Loaded BananaFramework v{this.Version}");
        base.OnEnabled();
    }
}