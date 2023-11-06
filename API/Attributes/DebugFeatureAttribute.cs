// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         DebugFeature.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/05/2023 2:47 PM
//    Created Date:     11/05/2023 2:47 PM
// -----------------------------------------

namespace BananaPlugin.API.Attributes;

using System;

/// <summary>
/// Allows defining a feature that is meant for debugging. Features with this attribute will only be enabled if debugging is enabled for the server.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class DebugFeatureAttribute : System.Attribute
{
    public DebugFeatureAttribute() { }
}