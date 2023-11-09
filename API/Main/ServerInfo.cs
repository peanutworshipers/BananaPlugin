// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         ServerInfo.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/05/2023 2:53 PM
//    Created Date:     11/05/2023 2:53 PM
// -----------------------------------------

namespace BananaPlugin.API.Main;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Interfaces;

/// <summary>
/// Defines an implementation of a single SL Server Instance.
/// </summary>
public abstract class ServerInfo : IPrefixableItem
{
    /// <summary>
    /// Initializes the server implementation for a plugin.
    /// </summary>
    /// <param name="pluginInstance">The instance of the plugin.</param>
    /// <param name="info">Returns a list of <see cref="ServerInfo"/> if allowed.</param>
    /// <exception cref="NullReferenceException">Thrown when the plugin doesn't have a valid Server.</exception>
    /// <returns>Returns the primary server info instance, if the server system is enabled. Otherwise returns null.</returns>
    internal static ServerInfo InitializeServerInformation(IServerInfo<BananaConfig> pluginInstance, out List<ServerInfo> info)
    {
        info = GetServerInfoInstances(pluginInstance);
        ServerInfo? server = FindCurrentServer(pluginInstance, info);
        if (server is null)
        {
            throw new NullReferenceException("Could not find a valid server profile, for this server. Try setting the Server Id.");
        }

        return server;
    }

    private static ServerInfo FindCurrentServer(IServerInfo<BananaConfig> instance, List<ServerInfo> values)
    {
        foreach (ServerInfo info in values)
        {
            switch (instance.SearchMethod)
            {
                case SearchIndex.ServerPort:
                    if (info.ServerPort != Server.Port)
                    {
                        continue;
                    }

                    return info;
                case SearchIndex.ServerName:
                    if (info.ServerName != Server.Name)
                    {
                        continue;
                    }

                    return info;
                case SearchIndex.ServerId:
                    if (info.ServerId != instance.Config.ServerId)
                    {
                        continue;
                    }

                    return info;
            }
        }

        throw new Exception("Server info not found from arguments supplied by the plugin. Ensure that the server info is defined.");
    }

    private static List<ServerInfo> GetServerInfoInstances(IServerInfo<BananaConfig> plugin)
    {
        List<ServerInfo> serverInfos = new List<ServerInfo>();
        Type[] types = plugin.GetType().Assembly.GetTypes();

        foreach (Type type in types)
        {
            if (!type.IsSubclassOf(typeof(ServerInfo)) || type.IsAbstract)
            {
                continue;
            }

            if (type.GetCustomAttribute<ObsoleteAttribute>() is not null)
            {
                continue;
            }

            ServerInfo info = (ServerInfo)Activator.CreateInstance(type, nonPublic: true);
            serverInfos.Add(info);
        }

        return serverInfos;
    }


    /// <summary>
    /// The port that the server uses.
    /// </summary>
    public abstract int ServerPort { get; }

    /// <summary>
    /// The name of the server.
    /// </summary>
    public abstract string ServerName { get; }

    /// <summary>
    /// This can be defined via the config of the server.
    /// </summary>
    public abstract string ServerId { get; }

    /// <inheritdoc />
    public abstract string Prefix { get; }

    /// <summary>
    /// Gets a value indicating whether features with the <see cref="global::BananaPlugin.API.Attributes.DebugFeatureAttribute"/> be loaded.
    /// </summary>
    public virtual bool EnableDebugFeatures => false;
}