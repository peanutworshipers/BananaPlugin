// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         PrefixableItem.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/05/2023 4:37 PM
//    Created Date:     11/05/2023 4:37 PM
// -----------------------------------------

namespace BananaPlugin.API.Interfaces;

/// <summary>
/// Adds a prefix to an item. Used for collections.
/// </summary>
public interface IPrefixableItem
{
    /// <summary>
    /// Gets the prefix of the item.
    /// </summary>
    string Prefix { get; }
}