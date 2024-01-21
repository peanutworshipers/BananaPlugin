// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         IServerInstancePlugin.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 2:32 PM
//    Created Date:     11/08/2023 2:32 PM
// -----------------------------------------

namespace BananaPlugin.API.Interfaces;

using Collections;
using Main;

/// <summary>
/// Used to define settings relevant to the server instancing system.
/// </summary>
/// <typeparam name="TConfig">The plugin config.</typeparam>
public interface IServerInfo<TConfig>
    where TConfig : BananaConfig, new()
{
    /// <summary>
    /// Gets or sets the <see cref="Main.ServerInfo"/> instances defined for this plugin.
    /// </summary>
    public ServerInfoCollection ServerInfo { get; set; }

    /// <summary>
    /// Gets the method of indexing that plugin authors can use to find the <see cref="ServerInfo"/> profile that this server instance is.
    /// </summary>
    public SearchIndex SearchMethod { get; }

    /// <summary>
    /// Gets the config instance.
    /// </summary>
    public TConfig Config { get; }
}