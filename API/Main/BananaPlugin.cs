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
using MEC;

#pragma warning disable SA1201

/// <summary>
/// The main plugin class for features to utilize.
/// </summary>
/// <typeparam name="TConfig">The config for this instance.</typeparam>
public abstract class BananaPlugin<TConfig> : Exiled.API.Features.Plugin<TConfig>
    where TConfig : IConfig, new()
{
    private static readonly List<BananaPlugin<TConfig>> PluginValues = new ();

    /// <summary>
    /// Gets the available instances of <see cref="BananaPlugin{TConfig}<TConfig>"/> that are available.
    /// </summary>
    public static IReadOnlyList<BananaPlugin<TConfig>> Plugins => PluginValues.AsReadOnly();

    /// <summary>
    /// Registers an instance of <see cref="BananaPlugin{TConfig}"/>.
    /// </summary>
    /// <param name="plugin">The <see cref="BananaPlugin{TConfig}"/> instance.</param>
    /// <typeparam name="T">The type of the <see cref="BananaPlugin{TConfig}"/>.</typeparam>
    public static void RegisterPlugin<T>(T plugin)
        where T : BananaPlugin<TConfig>
    {
        if (Plugins.Contains(plugin))
        {
            Log.Warn($"Tried to double register plugin {typeof(T).FullName}.");
            return;
        }

        PluginValues.Add(plugin);
    }

    /// <summary>
    /// Gets a registered <see cref="BananaPlugin{TConfig}"/> instance from its type.
    /// </summary>
    /// <typeparam name="T">The type of the plugin to search for.</typeparam>
    /// <returns>The instance of the <see cref="BananaPlugin{TConfig}"/>.</returns>
    /// <exception cref="NullReferenceException">Thrown if the <see cref="BananaPlugin{TConfig}"/> has not been registered yet.</exception>
    public static T GetPluginInstance<T>()
        where T : BananaPlugin<TConfig>
    {
        BananaPlugin<TConfig>? plugin = Plugins.FirstOrDefault(x => x is T);
        if (plugin is null)
        {
            throw new NullReferenceException($"Plugin {typeof(T).FullName} has not been registered yet.");
        }

        return (T)plugin;
    }

    /// <summary>
    /// Gets a registered <see cref="BananaPlugin{TConfig}"/> instance from its type.
    /// </summary>
    /// <param name="type">The type of the plugin to search for.</param>
    /// <returns>The instance of the <see cref="BananaPlugin{TConfig}"/>.</returns>
    /// <exception cref="NullReferenceException">Thrown if the <see cref="BananaPlugin{TConfig}"/> has not been registered yet.</exception>
    public static BananaPlugin<TConfig> GetPluginInstance(Type type)
    {
        BananaPlugin<TConfig>? plugin = Plugins.FirstOrDefault(x => x.GetType() == type);
        if (plugin is null)
        {
            throw new NullReferenceException($"Plugin {type.FullName} has not been registered yet.");
        }

        return plugin;
    }

    /// <summary>
    /// Gets a registered <see cref="BananaPlugin{TConfig}"/> instance from its type.
    /// </summary>
    /// <param name="type">The type of the plugin to search for.</param>
    /// <returns>The instance of the <see cref="BananaPlugin{TConfig}"/>.</returns>
    public static bool PluginIsRegistered(Type type)
    {
        return Plugins.Any(x => x.GetType() == type);
    }

    /// <summary>
    /// Gets a registered <see cref="BananaPlugin{TConfig}"/> instance from its type.
    /// </summary>
    /// <typeparam name="T">The type of the plugin to search for.</typeparam>
    /// <returns>The instance of the <see cref="BananaPlugin{TConfig}"/>.</returns>
    public static bool PluginIsRegistered<T>()
        where T : BananaPlugin<TConfig>
    {
        return Plugins.Any(x => x is T);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BananaPlugin{TConfig}"/> class.
    /// </summary>
    protected BananaPlugin()
    {
        BPLogger.Debug($"Registering Plugin {this.Name}");
        RegisterPlugin(this);
        Initializer.Initialize();

        Timing.CallDelayed(1f, this.LoadFeatures);


        // ReSharper disable once SuspiciousTypeConversion.Global
        bool serverInfo = this is IServerInfo<BananaConfig>;

        // ReSharper disable SuspiciousTypeConversion.Global
        BPLogger.Debug($"{this.Config is BananaConfig} {serverInfo}");
        if (this.Config is BananaConfig conf && this is IServerInfo<BananaConfig> serverInfo)
        {
            serverInfo.ServerInfo = new ServerInfoCollection(ServerInfo.InitializeServerInformation(serverInfo, out List<ServerInfo> servers), servers);
        }
    }

    /// <summary>
    /// The main config instance for the plugin.
    /// </summary>
    public TConfig Config { get; } = new();

    /// <summary>
    /// Gets the <see cref="Features"/> instance for this plugin.
    /// </summary>
    public FeatureCollection Features => new FeatureCollection();

    private void LoadFeatures()
    {
        Type[] types = this.GetType().Assembly.GetTypes();

        foreach (Type type in types)
        {
            // Ensure the feature is a BananaFeature and isn't abstract.
            if (!type.IsSubclassOf(typeof(BananaFeature)) || type.IsAbstract)
            {
                continue;
            }

            // Ensure the class is not Obsolete.
            if (type.GetCustomAttribute<ObsoleteAttribute>() is not null)
            {
                continue;
            }

            // Ensure that the server is on an allowed port.
            AllowedPortsAttribute? allowedPorts = type.GetCustomAttribute<AllowedPortsAttribute>();

            if (allowedPorts is not null && !allowedPorts.ValidPorts.Contains(ServerStatic.ServerPort))
            {
                BPLogger.Warn($"Command '{type.FullName}' skipped due to not having valid port selection.");
                continue;
            }

            // If this server doesn't use the server info system, skip this part.
            if (this is not IServerInfo<BananaConfig> info)
            {
                goto skipServerCheck;
            }

            // If this server is non debugging and the feature is a debug command, skip this part.
            if(!info.ServerInfo.PrimaryKey.EnableDebugFeatures && type.GetCustomAttribute<DebugFeatureAttribute>() is not null)
            {
                BPLogger.Warn($"Debug Command '{type.FullName}' skipped due to being on a non-debugging server.");
                continue;
            }

            List<AllowedServersAttribute> allowedServers = type.GetCustomAttributes<AllowedServersAttribute>().ToList();

            // If the command doesnt have any allowed server attributes, skip this part.
            if (allowedServers.Count < 0)
            {
                goto skipServerCheck;
            }

            // If the command isn't allowed on this server, skip this part.
            if (allowedServers.Any(x => x.ValidServers.Any(x => info.ServerInfo.PrimaryKey.GetType() != x)))
            {
                goto skipServerCheck;
            }

            BPLogger.Warn($"Feature '{type.FullName}' skipped because it is not enabled on this server.");
            continue;

            // Create an instance of the feature.
            skipServerCheck:
            BananaFeature feature = (BananaFeature)Activator.CreateInstance(type, nonPublic: true);

            if (!this.Features.TryAddItem(feature, out string? response))
            {
                BPLogger.Error(response);
            }
            else
            {
                feature.Enabled = true;
            }
        }

        // Mark all features for this plugin as loaded.
        this.Features.MarkAsLoaded();
    }

    public void RegisterCommands()
    {
        List<BananaParentCommand> bananaParentCommands = new();
        Type[] types = this.GetType().Assembly.GetTypes();
        foreach (Type type in types)
        {
            // Ignore nested types as we will do recursive checks anyways.
            if (type.IsNested)
            {
                continue;
            }

            // Ensure the feature is a BananaFeature and isn't abstract.
            if (!type.IsSubclassOf(typeof(BananaFeature)) || type.IsAbstract)
            {
                continue;
            }

            // Ensure the feature is not Obsolete.
            if (type.GetCustomAttribute<ObsoleteAttribute>() is not null)
            {
                continue;
            }

            // Ensure that the server is on an allowed port.
            AllowedPortsAttribute? allowedPorts = type.GetCustomAttribute<AllowedPortsAttribute>();

            if (allowedPorts is not null && !allowedPorts.ValidPorts.Contains(ServerStatic.ServerPort))
            {
                BPLogger.Warn($"Feature '{type.FullName}' skipped due to not having valid port selection.");
                continue;
            }

            // If this server doesn't use the server info system, skip this part.
            if (this is not IServerInfo<BananaConfig> info)
            {
                goto skipServerCheck;
            }

            // If this server is non debugging and the feature is a debug feature, skip this part.
            if (!info.ServerInfo.PrimaryKey.EnableDebugFeatures &&
                type.GetCustomAttribute<DebugFeatureAttribute>() is not null)
            {
                BPLogger.Warn($"Debug Feature '{type.FullName}' skipped due to being on a non-debugging server.");
                continue;
            }

            List<AllowedServersAttribute> allowedServers = type.GetCustomAttributes<AllowedServersAttribute>().ToList();

            // If the feature doesnt have any allowed server attributes, skip this part.
            if (allowedServers.Count < 0)
            {
                goto skipServerCheck;
            }

            // If the feature isn't allowed on this server, skip this part.
            if (allowedServers.Any(x => x.ValidServers.Any(x => info.ServerInfo.PrimaryKey.GetType() != x)))
            {
                goto skipServerCheck;
            }

            BPLogger.Warn($"Feature '{type.FullName}' skipped because it is not enabled on this server.");
            continue;

            // Create an instance of the feature.
            skipServerCheck:
            BananaFeature feature = (BananaFeature)Activator.CreateInstance(type, nonPublic: true);

            if (!this.Features.TryAddItem(feature, out string? response))
            {
                BPLogger.Error(response);
            }
            else
            {
                feature.Enabled = true;
            }
        }
    }
}