// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         Initializer.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/05/2023 3:18 PM
//    Created Date:     11/05/2023 3:18 PM
// -----------------------------------------

namespace BananaPlugin.API;

using System.Runtime.Remoting;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Interfaces;
using Main;
using MEC;

/// <summary>
/// Contains the Initializer that is used to enable the features of this framework.
/// </summary>
// ReSharper disable once UnusedType.Global
public static class Initializer
{
    private static bool initialized = false;

    /// <summary>
    /// Initializes the features of this framework.
    /// </summary>
    public static void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        CosturaUtility.Initialize();
    }
}