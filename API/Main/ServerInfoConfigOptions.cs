// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         ServerInfoConfigOptions.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/05/2023 3:02 PM
//    Created Date:     11/05/2023 3:02 PM
// -----------------------------------------

namespace BananaPlugin.API.Main;

/// <summary>
/// Public static config options that can be set by the plugin using this api.
/// </summary>
public static class ServerInfoConfigOptions
{
    /// <summary>
    /// Gets or sets the method of indexing that plugin authors can use to find the <see cref="ServerInfo"/> profile that this server instance is.
    /// </summary>
    public static SearchIndex SearchMethod { get; set; } = SearchIndex.ServerId;

    /// <summary>
    /// Gets or sets whether the server instance profile detection system is enabled.
    /// </summary>
    public static bool EnableServerInstanceProfileSystem { get; set; } = true;
}

/// <summary>
/// A list of indexing methods that can be used to identify a server.
/// </summary>
public enum SearchIndex
{
    /// <summary>
    /// Compares the <see cref="ServerInfo.ServerId"/> listed in the main config for this plugin.
    /// </summary>
    ServerId,

    /// <summary>
    /// Compares the <see cref="ServerInfo.ServerPort"/> to the server port.
    /// </summary>
    ServerPort,

    /// <summary>
    /// Compares the <see cref="ServerInfo.ServerName"/> to the name of the server.
    /// </summary>
    ServerName,
}