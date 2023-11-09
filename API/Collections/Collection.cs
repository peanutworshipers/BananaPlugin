namespace BananaPlugin.API.Collections;

using BananaPlugin.API.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Interfaces;

/// <summary>
/// Used to contain all bananaplugin Features.
/// </summary>
public class Collection<T> : IEnumerable<T>
    where T : IPrefixableItem
{
    protected readonly Dictionary<string, T> itemsByPrefix;
    protected  readonly List<T> items;

    bool isLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="Collection{T}"/> class.
    /// </summary>
    public Collection()
    {
        this.itemsByPrefix = new ();
        this.items = new ();
    }

    /// <summary>
    /// Gets the count of the amount of items in the collection.
    /// </summary>
    public int Count { get => this.items.Count; }

    /// <summary>
    /// Gets a value indicating whether the collection is loaded.
    /// </summary>
    public bool IsLoaded => this.isLoaded;

    /// <summary>
    /// Attempts to get an item by its prefix.
    /// </summary>
    /// <param name="prefix">The prefix to find.</param>
    /// <param name="item">The item, if found.</param>
    /// <returns>A value indicating whether the operation was a success.</returns>
    public bool TryGetItem(string prefix, [NotNullWhen(true)] out T? item)
    {
        return this.itemsByPrefix.TryGetValue(prefix, out item);
    }

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns>An enumerator over the list of items.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return this.items.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    /// <summary>
    /// Adds an item to the list.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="response">The error response.</param>
    /// <returns>A value indicating whether the operation was a success.</returns>
    internal virtual bool TryAddItem(T item, [NotNullWhen(false)] out string? response)
    {
        if (this.isLoaded)
        {
            response = $"Item '{item.Prefix}' could not be added. The collection is already loaded.";
            return false;
        }

        try
        {
            if (this.itemsByPrefix.ContainsKey(item.Prefix))
            {
                response = $"Item '{item.Prefix}' could not be added due to a duplicate prefix.";
                return false;
            }

            this.itemsByPrefix[item.Prefix] = item;
            this.items.Add(item);
            response = null;
            return true;
        }
        catch (Exception e)
        {
            response = $"Error adding feature to list: {e}";
            return false;
        }
    }

    /// <summary>
    /// Marks the collection as loaded, and no more Features can be added.
    /// </summary>
    internal void MarkAsLoaded()
    {
        this.isLoaded = true;
    }

    /// <inheritdoc cref="TryGetItem"/>
    public T this[string prefix]
    {
        get
        {
            if (!this.TryGetItem(prefix, out T? result))
            {
                throw new ArgumentOutOfRangeException($"Feature {prefix} does not exist, and cannot be retrieved.");
            }

            if (result is null)
            {
                throw new ArgumentOutOfRangeException($"Feature {prefix} does not exist, and cannot be retrieved.");
            }

            return result;
        }
    }
}
