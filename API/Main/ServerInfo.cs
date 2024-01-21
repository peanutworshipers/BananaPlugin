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
using System.Reflection;
using Exiled.API.Features;
using Interfaces;
using Utils;

/// <summary>
/// Defines an implementation of a single SL Server Instance.
/// </summary>
// ReSharper disable MemberCanBeProtected.Global
public abstract class ServerInfo : IPrefixableItem
{
    /// <summary>
    /// Gets the port that the server uses.
    /// </summary>
    public abstract int ServerPort { get; }

    /// <summary>
    /// Gets the name of the server.
    /// </summary>
    public abstract string ServerName { get; }

    /// <summary>
    /// Gets this can be defined via the config of the server.
    /// </summary>
    public abstract string ServerId { get; }

    /// <inheritdoc />
    public abstract string Prefix { get; }

    /// <summary>
    /// Gets a value indicating whether features with the <see cref="global::BananaPlugin.API.Attributes.DebugFeatureAttribute"/> be loaded.
    /// </summary>
    public virtual bool EnableDebugFeatures => false;

    /// <summary>
    /// Initializes the server implementation for a plugin.
    /// </summary>
    /// <param name="pluginInstance">The instance of the plugin.</param>
    /// <param name="info">Returns a list of <see cref="ServerInfo"/> if allowed.</param>
    /// <typeparam name="T">The config type.</typeparam>
    /// <exception cref="NullReferenceException">Thrown when the plugin doesn't have a valid Server.</exception>
    /// <returns>Returns the primary server info instance, if the server system is enabled. Otherwise returns null.</returns>
    internal static ServerInfo InitializeServerInformation<T>(IServerInfo<T> pluginInstance, out List<ServerInfo> info)
        where T : BananaConfig, new()
    {
        info = GetServerInfoInstances(pluginInstance);
        ServerInfo? server = FindCurrentServer(pluginInstance, info);
        if (server is null)
        {
            Log.Error("Could not find a valid server profile, for this server. Try setting the Server Id.");
            throw new NullReferenceException("Could not find a valid server profile, for this server. Try setting the Server Id.");
        }

        return server;
    }

    private static ServerInfo FindCurrentServer<T>(IServerInfo<T> instance, List<ServerInfo> values)
        where T : BananaConfig, new()
    {
        BPLogger.Debug($"Current Server Info: {instance.Config.ServerId} [{Server.Port}]. Search method - {instance.SearchMethod}");
        foreach (ServerInfo info in values)
        {
            switch (instance.SearchMethod)
            {
                case SearchIndex.ServerPort:
                    if (info.ServerPort != Server.Port)
                    {
                        continue;
                    }

                    BPLogger.Debug($"Found Server {info.ServerName} [{info.ServerPort}]");
                    return info;
                case SearchIndex.ServerName:
                    if (info.ServerName != Server.Name)
                    {
                        continue;
                    }

                    BPLogger.Debug($"Found Server {info.ServerName} [{info.ServerPort}]");
                    return info;
                case SearchIndex.ServerId:
                    if (info.ServerId != instance.Config.ServerId)
                    {
                        continue;
                    }

                    BPLogger.Debug($"Found Server {info.ServerName} [{info.ServerPort}]");
                    return info;
            }
        }

        Log.Error("Server info not found from arguments supplied by the plugin. Ensure that the server info is defined.");
        throw new Exception("Server info not found from arguments supplied by the plugin. Ensure that the server info is defined.");
    }

    private static List<ServerInfo> GetServerInfoInstances<T>(IServerInfo<T> plugin)
        where T : BananaConfig, new()
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
            BPLogger.Debug($"Found Server Info {info.ServerName} [{info.ServerPort}] - {info.ServerId} {(info.EnableDebugFeatures ? "[Debug]" : string.Empty)}");
        }

        return serverInfos;
    }
}