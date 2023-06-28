namespace BananaPlugin;

using BananaPlugin.Features;
using BananaPlugin.Features.Configs;
using Exiled.API.Interfaces;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The default config class for this assembly.
/// </summary>
public sealed class Config : IConfig
{
    private CfgGuardEscape guardEscape;
    private CfgAutoNuke autoNuke;
    private CfgPinkCandyBowl pinkCandyBowl;
    private CfgCleanup cleanup;
    private CfgLevelSystem levelSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="Config"/> class.
    /// </summary>
    public Config()
    {
        this.GuardEscape = new();
        this.AutoNuke = new();
        this.PinkCandyBowl = new();
        this.Cleanup = new();
        this.LevelSystem = new();
    }

    /// <summary>
    /// Event that invokes whenever a feature based config is updated.
    /// </summary>
    internal static event Action<Config>? FeatureConfigUpdated;

    /// <summary>
    /// Gets or sets a value indicating whether the plugin is enabled.
    /// </summary>
    [Description("Enables the plugin.")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether debug logging is enabled.
    /// </summary>
    [Description("Enables debug logging.")]
    public bool Debug { get; set; } = true;

    /// <summary>
    /// Gets or sets the guard escape config.
    /// </summary>
    public CfgGuardEscape GuardEscape
    {
        get => this.guardEscape;
        [MemberNotNull(nameof(guardEscape))]
        set
        {
            this.guardEscape = value;
            FeatureConfigUpdated?.Invoke(this);
        }
    }

    /// <summary>
    /// Gets or sets the auto nuke config.
    /// </summary>
    public CfgAutoNuke AutoNuke
    {
        get => this.autoNuke;
        [MemberNotNull(nameof(autoNuke))]
        set
        {
            this.autoNuke = value;
            FeatureConfigUpdated?.Invoke(this);
        }
    }

    /// <summary>
    /// Gets or sets the pink candy bowl config.
    /// </summary>
    public CfgPinkCandyBowl PinkCandyBowl
    {
        get => this.pinkCandyBowl;
        [MemberNotNull(nameof(pinkCandyBowl))]
        set
        {
            this.pinkCandyBowl = value;
            FeatureConfigUpdated?.Invoke(this);
        }
    }

    /// <summary>
    /// Gets or sets the cleanup config.
    /// </summary>
    public CfgCleanup Cleanup
    {
        get => this.cleanup;
        [MemberNotNull(nameof(cleanup))]
        set
        {
            this.cleanup = value;
            FeatureConfigUpdated?.Invoke(this);
        }
    }

    /// <summary>
    /// Gets or sets the level system config.
    /// </summary>
    public CfgLevelSystem LevelSystem
    {
        get => this.levelSystem;
        [MemberNotNull(nameof(levelSystem))]
        set
        {
            this.levelSystem = value;
            FeatureConfigUpdated?.Invoke(this);
        }
    }
}
