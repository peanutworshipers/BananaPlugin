// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         IBpPlugin.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/05/2023 3:16 PM
//    Created Date:     11/05/2023 3:16 PM
// -----------------------------------------

namespace BananaPlugin.API.Main;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BananaPlugin.API.Attributes;
using BananaPlugin.API.Collections;
using BananaPlugin.API.Utils;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Interfaces;

#pragma warning disable SA1201

/// <summary>
/// The main plugin class for features to utilize.
/// </summary>
/// <typeparam name="TConfig">The config for this instance.</typeparam>
public abstract class BpPlugin<TConfig> : Exiled.API.Features.Plugin<TConfig>
    where TConfig : IConfig, new()
{
    private static readonly List<BpPlugin<TConfig>> PluginValues = new ();

    /// <summary>
    /// Gets the available instances of <see cref="BpPlugin<TConfig>"/> that are available.
    /// </summary>
    public static IReadOnlyList<BpPlugin<TConfig>> Plugins => PluginValues.AsReadOnly();

    /// <summary>
    /// Registers an instance of <see cref="BpPlugin"/>.
    /// </summary>
    /// <param name="plugin">The <see cref="BpPlugin"/> instance.</param>
    /// <typeparam name="T">The type of the <see cref="BpPlugin"/>.</typeparam>
    public static void RegisterPlugin<T>(T plugin)
        where T : BpPlugin<TConfig>
    {
        if (Plugins.Contains(plugin))
        {
            Log.Warn($"Tried to double register plugin {typeof(T).FullName}.");
            return;
        }

        PluginValues.Add(plugin);
    }

    /// <summary>
    /// Gets a registered <see cref="BpPlugin"/> instance from its type.
    /// </summary>
    /// <typeparam name="T">The type of the plugin to search for.</typeparam>
    /// <returns>The instance of the <see cref="BpPlugin"/>.</returns>
    /// <exception cref="NullReferenceException">Thrown if the <see cref="BpPlugin"/> has not been registered yet.</exception>
    public static T GetPluginInstance<T>()
        where T : BpPlugin<TConfig>
    {
        BpPlugin<TConfig>? plugin = Plugins.FirstOrDefault(x => x is T);
        if (plugin is null)
        {
            throw new NullReferenceException($"Plugin {typeof(T).FullName} has not been registered yet.");
        }

        return (T)plugin;
    }

    /// <summary>
    /// Gets a registered <see cref="BpPlugin"/> instance from its type.
    /// </summary>
    /// <param name="type">The type of the plugin to search for.</param>
    /// <returns>The instance of the <see cref="BpPlugin"/>.</returns>
    /// <exception cref="NullReferenceException">Thrown if the <see cref="BpPlugin"/> has not been registered yet.</exception>
    public static BpPlugin<TConfig> GetPluginInstance(Type type)
    {
        BpPlugin<TConfig>? plugin = Plugins.FirstOrDefault(x => x.GetType() == type);
        if (plugin is null)
        {
            throw new NullReferenceException($"Plugin {type.FullName} has not been registered yet.");
        }

        return plugin;
    }

    /// <summary>
    /// Gets a registered <see cref="BpPlugin"/> instance from its type.
    /// </summary>
    /// <param name="type">The type of the plugin to search for.</param>
    /// <returns>The instance of the <see cref="BpPlugin"/>.</returns>
    public static bool PluginIsRegistered(Type type)
    {
        return Plugins.Any(x => x.GetType() == type);
    }

    /// <summary>
    /// Gets a registered <see cref="BpPlugin"/> instance from its type.
    /// </summary>
    /// <typeparam name="T">The type of the plugin to search for.</typeparam>
    /// <returns>The instance of the <see cref="BpPlugin"/>.</returns>
    public static bool PluginIsRegistered<T>()
        where T : BpPlugin<TConfig>
    {
        return Plugins.Any(x => x is T);
    }

    /// <summary>
    /// Gzets the <see cref="Main.ServerInfo"/> instances defined for this plugin.
    /// </summary>
    public ServerInfoCollection ServerInfo => new ServerInfoCollection(global::BananaPlugin.API.Main.ServerInfo.InitializeServerInformation(this, out List<ServerInfo>? servers), servers);

    public TConfig Config { get; } = new();

    /// <summary>
    /// Gets the <see cref="Features"/> instance for this plugin.
    /// </summary>
    public FeatureCollection Features => new FeatureCollection();

    private void LoadFeatures()
    {
        Type[] types = this.Assembly.GetTypes();

        foreach (Type type in types)
        {
            if (!type.IsSubclassOf(typeof(PluginFeature)) || type.IsAbstract)
            {
                continue;
            }

            if (type.GetCustomAttribute<ObsoleteAttribute>() is not null)
            {
                continue;
            }

            AllowedPortsAttribute? allowedPorts = type.GetCustomAttribute<AllowedPortsAttribute>();

            if (allowedPorts is not null && !allowedPorts.ValidPorts.Contains(ServerStatic.ServerPort))
            {
                BPLogger.Warn($"Feature '{type.FullName}' skipped due to not having valid port selection.");
                continue;
            }

            if (type.GetCustomAttribute<DebugFeatureAttribute>() is not null)
            {
                BPLogger.Warn($"Feature '{type.FullName}' skipped due to being on a non-debugging server.");
            }

            PluginFeature feature = (PluginFeature)Activator.CreateInstance(type, nonPublic: true);

            if (!this.Features.TryAddItem(feature, out string? response))
            {
                BPLogger.Error(response);
            }
            else
            {
                feature.Enabled = true;
            }
        }

        this.Features.MarkAsLoaded();
    }
}