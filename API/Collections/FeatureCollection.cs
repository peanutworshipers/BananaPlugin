namespace BananaPlugin.API.Collections;

using Main;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Used to contain all <see cref="BananaFeature"/> for a <see cref="BananaPlugin{TConfig}"/>.
/// </summary>
public sealed class FeatureCollection : Collection<BananaFeature>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureCollection"/> class.
    /// </summary>
    public FeatureCollection()
    {
    }

    /// <inheritdoc cref="TryGetFeature"/>
    public new BananaFeature this[string prefix]
    {
        get
        {
            if (!this.TryGetFeature(prefix, out BananaFeature? result))
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
    public bool TryGetFeature(string prefix, [NotNullWhen(true)] out BananaFeature? feature) =>
        this.TryGetItem(prefix, out feature);
}
