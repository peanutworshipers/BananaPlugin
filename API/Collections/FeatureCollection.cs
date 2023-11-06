namespace BananaPlugin.API.Collections;

using BananaPlugin.API.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Interfaces;

/// <summary>
/// Used to contain all <see cref="PluginFeature"/> for a <see cref="BpPlugin"/>.
/// </summary>
public sealed class FeatureCollection : Collection<PluginFeature>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureCollection"/> class.
    /// </summary>
    public FeatureCollection()
    {
    }

    /// <inheritdoc cref="TryGetFeature"/>
    public new PluginFeature this[string prefix]
    {
        get
        {
            if (!this.TryGetFeature(prefix, out PluginFeature? result))
            {
                throw new ArgumentOutOfRangeException($"Feature {prefix} does not exist, and cannot be retrieved.");
            }

            return result;
        }
    }

    /// <summary>
    /// Attempts to get a feature by its prefix.
    /// </summary>
    /// <param name="prefix">The prefix to find.</param>
    /// <param name="feature">The feature, if found.</param>
    /// <returns>A value indicating whether the operation was a success.</returns>
    public bool TryGetFeature(string prefix, [NotNullWhen(true)] out PluginFeature? feature) =>
        this.TryGetItem(prefix, out feature);

}
