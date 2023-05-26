#nullable enable
namespace BananaPlugin;

using Exiled.API.Interfaces;
using System.ComponentModel;

/// <summary>
/// The default config class for this assembly.
/// </summary>
public sealed class Config : IConfig
{
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
}
