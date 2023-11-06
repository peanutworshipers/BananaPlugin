// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         IPluginConfig.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/05/2023 3:09 PM
//    Created Date:     11/05/2023 3:09 PM
// -----------------------------------------

namespace BananaPlugin.API.Main;

using System.ComponentModel;
using Exiled.API.Interfaces;

#pragma warning disable CS8618

/// <summary>
/// The interface that plugins should use.
/// </summary>
public abstract class BpConfig : IConfig
{
    /// <summary>
    /// Gets or sets the <see cref="ServerInfo.ServerId"/> of this server.
    /// </summary>
    [Description("The id of the server that this plugin is being run on.")]
    public abstract string ServerId { get; set; }

    /// <inheritdoc cref="IConfig.IsEnabled"/>
    [Description("The id of the server that this plugin is being run on.")]
    public abstract bool IsEnabled { get; set; }

    /// <inheritdoc cref="IConfig.Debug"/>
    [Description("Enables or Disables debugging logs for a plugin.")]
    public abstract bool Debug { get; set; }
}