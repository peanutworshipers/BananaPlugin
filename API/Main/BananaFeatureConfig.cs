namespace BananaPlugin.API.Main;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The main feature implementation allowing for config usage.
/// </summary>
/// <typeparam name="T">The config type to use.</typeparam>
public abstract class BananaFeatureConfig<T> : BananaFeature
    where T : class, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BananaFeatureConfig{T}"/> class.
    /// </summary>
    protected BananaFeatureConfig()
    {
        if (!Plugin.AssertEnabled())
        {
            throw new System.InvalidOperationException();
        }

        this.OnConfigUpdated(Plugin.Instance.Config);
        Config.FeatureConfigUpdated += this.OnConfigUpdated;
    }

    /// <summary>
    /// Gets the local feature config.
    /// </summary>
    public T LocalConfig { get; private set; }

    /// <summary>
    /// Called when updating the local feature config.
    /// </summary>
    /// <param name="config">The config to apply.</param>
    [MemberNotNull(nameof(LocalConfig))]
    protected void OnConfigUpdated(Config config) => this.LocalConfig = this.RetrieveLocalConfig(config);

    /// <summary>
    /// Retrieves the local config from the specified config instance.
    /// </summary>
    /// <param name="config">The config to retrieve the local config from.</param>
    /// <returns>When overriden, returns the found local config instance.</returns>
    protected abstract T RetrieveLocalConfig(Config config);
}
