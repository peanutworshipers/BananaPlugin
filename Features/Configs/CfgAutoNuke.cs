namespace BananaPlugin.Features.Configs;

using System.ComponentModel;

/// <summary>
/// The main config for auto nuke.
/// </summary>
public sealed class CfgAutoNuke
{
    /// <summary>
    /// Gets or sets the time after round start before auto nuke is activated.
    /// </summary>
    [Description("The time after round start before auto nuke is activated.")]
    public double NukeSeconds { get; set; } = 1500d;

    /// <summary>
    /// Gets or sets the message broadcasted when the auto nuke starts.
    /// </summary>
    [Description("The message broadcasted when the auto nuke starts.")]
    public string BroadcastMsg { get; set; } = "<size=50><color=red><b>AutoNuke is enabled.</b></color></size>\\n<size=35>The warhead cannot be disabled. Escape while you can!</size>";

    /// <summary>
    /// Gets or sets the message broadcasted when the auto nuke is disabled.
    /// </summary>
    [Description("The message broadcasted when the auto nuke is disabled.")]
    public string BroadcastDisabledMsg { get; set; } = "<size=50><color=#00ff00><b>AutoNuke disabled by admin.</b></color></size>\\n<size=35>Sad! no more funny nuke ;(</size>";
}
