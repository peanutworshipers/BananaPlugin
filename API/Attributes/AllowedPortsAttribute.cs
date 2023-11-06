namespace BananaPlugin.API.Attributes;

using BananaPlugin.API.Main;
using System;

/// <summary>
/// An attribute that specifies a <see cref="PluginFeature"/> must only enable on certain server ports.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class AllowedPortsAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedPortsAttribute"/> class.
    /// </summary>
    /// <param name="ports">The allowed ports for this attribute.</param>
    public AllowedPortsAttribute(params int[] ports)
    {
        this.ValidPorts = ports;
    }

    /// <summary>
    /// Gets the valid ports associated with this attribute.
    /// </summary>
    public int[] ValidPorts { get; }
}
