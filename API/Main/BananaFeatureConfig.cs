namespace BananaPlugin.API.Main;

/// <summary>
/// The main feature implementation allowing for config usage.
/// </summary>
/// <typeparam name="T">The config type to use.</typeparam>
public abstract class BananaFeatureConfig<T> : BananaFeature
    where T : class, new()
{
    /// <summary>
    /// Gets or sets the local feature config.
    /// </summary>
    public abstract T LocalConfig { get; protected set; }

    /// <summary>
    /// Called when updating the local feature config.
    /// </summary>
    /// <param name="config">The config to apply.</param>
    protected void OnConfigUpdated(T config) => this.LocalConfig = config;
}
