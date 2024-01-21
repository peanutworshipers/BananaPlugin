// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         Config.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/09/2023 10:48 AM
//    Created Date:     11/09/2023 10:48 AM
// -----------------------------------------

namespace BananaPlugin;

using Exiled.API.Interfaces;

/// <summary>
/// The main instance of the config.
/// </summary>
public sealed class Config : IConfig
{
    /// <inheritdoc/>
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc/>
    public bool Debug { get; set; }
}