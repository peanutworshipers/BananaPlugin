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