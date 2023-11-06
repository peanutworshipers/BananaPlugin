// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         CollectionPrimaryKet.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/05/2023 4:53 PM
//    Created Date:     11/05/2023 4:53 PM
// -----------------------------------------

namespace BananaPlugin.API.Interfaces;

/// <summary>
/// Adds a primary instance of an item to a dictionary for caching or other purposes.
/// </summary>
/// <typeparam name="T">The type of the primary key.</typeparam>
public interface ICollectionPrimaryKey<T>
{
    /// <summary>
    /// The primary item of the collection.
    /// </summary>
    public T PrimaryKey { get; }
}