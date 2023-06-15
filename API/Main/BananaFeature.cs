namespace BananaPlugin.API.Main;

using BananaPlugin.API.Utils;
using System;

/// <summary>
/// The main feature implementation.
/// </summary>
public abstract class BananaFeature
{
    private bool enabled = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="BananaFeature"/> class.
    /// </summary>
    protected BananaFeature()
    {
    }

    /// <summary>
    /// Gets the name of the feature.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the prefix of the feature.
    /// </summary>
    public abstract string Prefix { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the feature is enabled.
    /// </summary>
    public bool Enabled
    {
        get => this.enabled;
        set
        {
            // Check if setting same value.
            if (this.enabled == value)
            {
                return;
            }

            try
            {
                if (value)
                {
                    this.Enable();

                    BPLogger.Info($"Feature '{this.Name}' was enabled!");
                }
                else
                {
                    this.Disable();

                    BPLogger.Info($"Feature '{this.Name}' was disabled!");
                }

                this.enabled = value;
            }
            catch (Exception e)
            {
                BPLogger.Error($"Failed setting enabled to {value}" + e);
                throw;
            }
        }
    }

    /// <summary>
    /// Enables the feature.
    /// </summary>
    protected abstract void Enable();

    /// <summary>
    /// Disables the feature.
    /// </summary>
    protected abstract void Disable();
}
