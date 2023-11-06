// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         IPluginConfig.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/05/2023 5:39 PM
//    Created Date:     11/05/2023 3:09 PM
// -----------------------------------------

namespace BananaPlugin.API.Interfaces;

using Exiled.API.Interfaces;

public interface IBpConfig : IConfig
{
    string ServerId { get; }
}