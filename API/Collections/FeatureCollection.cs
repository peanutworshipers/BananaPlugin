namespace BananaPlugin.API.Collections;

using BananaPlugin.API.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Used to contain all bananaplugin Features.
/// </summary>
public sealed class FeatureCollection : IEnumerable<BananaFeature>
{
    private readonly Dictionary<string, BananaFeature> featuresByPrefix;
    private readonly List<BananaFeature> features;

    private bool isLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureCollection"/> class.
    /// </summary>
    public FeatureCollection()
    {
        this.featuresByPrefix = new ();
        this.features = new ();
    }

    /// <summary>
    /// Gets a value indicating whether the collection is loaded.
    /// </summary>
    public bool IsLoaded => this.IsLoaded;

    /// <summary>
    /// Attempts to get a feature by its prefix.
    /// </summary>
    /// <param name="prefix">The prefix to find.</param>
    /// <param name="feature">The feature, if found.</param>
    /// <returns>A value indicating whether the operation was a success.</returns>
    public bool TryGetFeature(string prefix, [NotNullWhen(true)] out BananaFeature? feature)
    {
        return this.featuresByPrefix.TryGetValue(prefix, out feature);
    }

    /// <summary>
    /// Gets the enumerator.
    /// </summary>
    /// <returns>An enumerator over the list of Features.</returns>
    public IEnumerator<BananaFeature> GetEnumerator()
    {
        return this.features.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    /// <summary>
    /// Adds a feature to the list.
    /// </summary>
    /// <param name="feature">The feature to add.</param>
    /// <param name="response">The error response.</param>
    /// <returns>A value indicating whether the operation was a success.</returns>
    internal bool TryAddFeature(BananaFeature feature, [NotNullWhen(false)] out string? response)
    {
        if (this.isLoaded)
        {
            response = $"Feature '{feature.Prefix}' could not be added. The collection is already loaded.";
            return false;
        }

        try
        {
            if (this.featuresByPrefix.ContainsKey(feature.Prefix))
            {
                response = $"Feature '{feature.Name}' could not be added due to a duplicate prefix.";
                return false;
            }

            this.featuresByPrefix[feature.Prefix] = feature;
            this.features.Add(feature);
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
}
