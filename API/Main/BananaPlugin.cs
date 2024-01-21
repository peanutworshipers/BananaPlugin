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
using Attributes;
using Collections;
using Commands;
using Commands.Arguments;
using Utils;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Interfaces;

#pragma warning disable SA1201
#pragma warning disable SA1011

/// <summary>
/// The main plugin class for features to utilize.
/// </summary>
/// <typeparam name="TConfig">The config for this instance.</typeparam>
// ReSharper disable MemberCanBePrivate.Global
public abstract class BananaPlugin<TConfig> : Plugin<TConfig>
    where TConfig : BananaConfig, IConfig, new()
{
    private static readonly List<BananaPlugin<TConfig>> PluginValues = new();

    /// <summary>
    /// Gets the available instances of <see cref="BananaPlugin{TConfig}"/> that are available.
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
        BPLogger.Debug($"Registering Plugin {this.GetType().FullName}");
        RegisterPlugin(this);
        Initializer.Initialize();

        try
        {
            // ReSharper disable SuspiciousTypeConversion.Global
            if (this.Config is BananaConfig && this is IServerInfo<TConfig> serverInfo)
            {
                serverInfo.ServerInfo =
                    new ServerInfoCollection(
                        ServerInfo.InitializeServerInformation(serverInfo, out List<ServerInfo> servers), servers);
            }
        }
        catch (Exception e)
        {
            BPLogger.Warn("Could not load server info due to an error.");
            BPLogger.Debug($"Exception: \n{e}");
        }

        try
        {
            this.LoadFeatures();
        }
        catch (Exception e)
        {
            BPLogger.Warn("Could not load features due to an error.");
            BPLogger.Debug($"Exception: \n{e}");
        }

        try
        {
            this.RegisterCommands();
        }
        catch (Exception e)
        {
            BPLogger.Warn("Could not load commands due to an error.");
            BPLogger.Debug($"Exception: \n{e}");
        }

        try
        {
            this.LoadRoles();
        }
        catch (Exception e)
        {
            BPLogger.Warn("Could not load roles due to an error.");
            BPLogger.Debug($"Exception: \n{e}");
        }
    }

    /// <summary>
    /// Gets the main config instance for the plugin.
    /// </summary>
    public new TConfig Config { get; } = new();

    /// <summary>
    /// Gets the <see cref="FeatureCollection"/> instance for this plugin.
    /// </summary>
    public FeatureCollection Features => new FeatureCollection();

    /// <summary>
    /// Gets the <see cref="PermissionCollection"/> for this plugin.
    /// </summary>
    public PermissionCollection Permissions => new PermissionCollection();

    /// <summary>
    /// Gets the <see cref="RoleCollection"/> for this plugin.
    /// </summary>
    public RoleCollection Roles => new RoleCollection();

    /// <summary>
    /// Used to register the relative commands for a plugin.
    /// </summary>
    // ReSharper disable RedundantAssignment
    public void RegisterCommands()
    {
        try
        {
            // List<BananaParentCommand> bananaParentCommands = new();
            List<MethodInfo> commands = this.GetAllMethodsWithAttribute<BananaCommandAttribute>();

            BPLogger.Debug($"Methods found: {commands.Count}");
            int i = 0;
            foreach (MethodInfo methodInfo in commands)
            {
                BananaCommandAttribute? cmdAttribute = methodInfo.GetCustomAttribute<BananaCommandAttribute>();
                if (cmdAttribute is null)
                {
                    continue;
                }

                i++;
                Type? baseType = methodInfo.DeclaringType;
                List<ParentCommandImplementation> cmds = new();
                bool parentCmdFailedToRegister = false;
                while (true)
                {
                    if (baseType is null)
                    {
                        break;
                    }

                    BananaParentCommandAttribute? attribute = baseType.GetCustomAttribute<BananaParentCommandAttribute>();
                    if (attribute is null)
                    {
                        break;
                    }

                    object? parentInstance = null;
                    MethodInfo? executor = this.GetAllMethodsWithAttribute<BananaParentCommandExecutorAttribute>()
                        .FirstOrDefault();
                    if (executor is not null && !executor.IsStatic)
                    {
                        if (methodInfo.DeclaringType is null)
                        {
                            BPLogger.Warn($"Could not register parent command \'{cmdAttribute.Name}\' because the declaring type could not be found.");
                            parentCmdFailedToRegister = true;
                            break;
                        }

                        if (methodInfo.DeclaringType.IsSubclassOf(typeof(BananaFeature)))
                        {
                            BananaFeature? feature = this.Features.FirstOrDefault(ftr => ftr.GetType() == methodInfo.DeclaringType);
                            if (feature is not null)
                            {
                                parentInstance = feature;
                                goto skipParentInstanceCheck;
                            }
                        }

                        if (methodInfo.DeclaringType.IsSubclassOf(this.GetType()))
                        {
                            parentInstance = this;
                            goto skipParentInstanceCheck;
                        }

                        parentCmdFailedToRegister = true;
                        BPLogger.Warn($"Could not register parent command \'{cmdAttribute.Name}\' because the executor method defined is non-static and is not located within a banana item (plugin or feature).");
                        break;
                    }

                    skipParentInstanceCheck:

                    if (executor is not null)
                    {
                        ParameterInfo[] parentParameters = methodInfo.GetParameters();
                        if (parentParameters.Length != 1)
                        {
                            parentCmdFailedToRegister = true;
                            BPLogger.Warn($"Could not register parent command \'{cmdAttribute.Name}\' because the executor method did not have valid parameters. The executor method must define a single CommandResult.");
                            break;
                        }

                        if (parentParameters[0].ParameterType != typeof(CommandResponse))
                        {
                            parentCmdFailedToRegister = true;
                            BPLogger.Warn($"Could not register parent command \'{cmdAttribute.Name}\' because the executor method did not have valid parameters. The executor method must define a single CommandResult.");
                            break;
                        }
                    }

                    i++;
                    ParentCommandImplementation parentCmd = new(attribute, cmds.Count > 0 ? cmds[-1] : null, methodInfo, parentInstance);
                    cmds.Add(parentCmd);
                }

                if (parentCmdFailedToRegister)
                {
                    BPLogger.Warn($"Could not register command \'{cmdAttribute.Name}\' because a parent command failed to register.");
                    continue;
                }

                object? instance = null;

                if (!methodInfo.IsStatic)
                {
                    if (methodInfo.DeclaringType is null)
                    {
                        BPLogger.Warn($"Could not register command \'{cmdAttribute.Name}\' because the declaring type could not be found.");
                        continue;
                    }

                    if (methodInfo.DeclaringType.IsSubclassOf(typeof(BananaFeature)))
                    {
                        BPLogger.Debug($"Command BaseType: {methodInfo.DeclaringType.FullName} [{this.Features.GetCount()}]");
                        foreach (BananaFeature ftr in this.Features)
                        {
                            BPLogger.Debug($"Feature Found {ftr.GetType().FullName}");
                        }

                        BananaFeature? feature = this.Features.FirstOrDefault(ftr => ftr.GetType() == methodInfo.DeclaringType);
                        if (feature is not null)
                        {
                            instance = feature;
                            goto skipInstanceCheck;
                        }

                        BPLogger.Debug($"BananaFeature Not found.");
                    }

                    if (methodInfo.DeclaringType.IsSubclassOf(this.GetType()))
                    {
                        instance = this;
                        goto skipInstanceCheck;
                    }

                    BPLogger.Warn($"Could not register command \'{cmdAttribute.Name}\' because it is non-static and is not located within a banana item (plugin or feature).");
                    continue;
                }

                skipInstanceCheck:

                ParameterInfo[] parameters = methodInfo.GetParameters();
                if (parameters.Length != 1)
                {
                    parentCmdFailedToRegister = true;
                    BPLogger.Warn($"Could not register parent command \'{cmdAttribute.Name}\' because the executor method did not have valid parameters. The executor method must define a single CommandResult.");
                    continue;
                }

                if (parameters[0].ParameterType != typeof(CommandResponse))
                {
                    parentCmdFailedToRegister = true;
                    BPLogger.Warn($"Could not register parent command \'{cmdAttribute.Name}\' because the executor method did not have valid parameters. The executor method must define a single CommandResult.");
                    continue;
                }

                List<BananaPermission> permissions = new List<BananaPermission>();
                CommandImplementation cmd = new CommandImplementation(cmdAttribute, methodInfo, permissions, instance);
                if (cmds.Count > 0)
                {
                    // calculate roles
                    cmds[-1].RegisterCommand(cmd);
                    cmds[0].RegisterCommandSequences();
                }
                else
                {
                    // calculate usages
                    // calculate roles
                    cmd.RegisterNonUsageProviderSafe();
                }
            }

            BPLogger.Debug($"Registered {i} commands for {this.GetType().FullName}.");
        }
        catch (Exception e)
        {
            BPLogger.Error($"Caught an exception while loading commands.");
            BPLogger.Debug($"Exception: {e}");
        }
    }

    private void LoadFeatures()
    {
        try
        {
            List<BananaFeature> features = this.GetAllInstancesOfItem<BananaFeature>();

            foreach (BananaFeature feature in features)
            {
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
            BPLogger.Debug($"Registered {features.Count} features for {this.GetType().FullName}.");
        }
        catch (Exception e)
        {
            BPLogger.Error($"Caught an exception while loading features.");
            BPLogger.Debug($"Exception: {e}");
        }
    }

    private void LoadRoles()
    {
        try
        {
            List<BananaPermission> permissions = this.GetAllInstancesOfItem<BananaPermission>();
            List<BananaRole> roles = this.GetAllInstancesOfItem<BananaRole>();
            foreach (BananaPermission perm in permissions)
            {
                if (perm is BananaChildPermission<BananaPermission> permission)
                {
                    permission.ParentPermission =
                        permissions.First(x => x.GetType() == permission.ParentPermission.GetType());
                    if (!permission.ParentPermission.ChildPermissions.TryAddItem(perm, out string? response3))
                    {
                        BPLogger.Warn(response3);
                    }
                }
            }

            foreach (BananaRole role in roles)
            {
                foreach (BananaPermission permission in permissions)
                {
                    if (role.DefinedPermissions.Contains(permission.GetType()))
                    {
                        if (!role.PermissionCollection.TryAddItem(permission, out string? response1))
                        {
                            BPLogger.Warn(response1);
                        }

                        if (!permission.InheritingRoles.TryAddItem(role, out string? response2))
                        {
                            BPLogger.Warn(response2);
                        }
                    }
                }

                role.PermissionCollection.MarkAsLoaded();

                if (!this.Roles.TryAddItem(role, out string? response))
                {
                    BPLogger.Warn(response);
                }
            }

            foreach (BananaPermission perm in permissions)
            {
                perm.ChildPermissions.MarkAsLoaded();
                perm.InheritingRoles.MarkAsLoaded();
                if (!this.Permissions.TryAddItem(perm, out string? response))
                {
                    BPLogger.Warn(response);
                }
            }

            this.Roles.MarkAsLoaded();
            this.Permissions.MarkAsLoaded();
            BPLogger.Debug($"Registered {this.Permissions.GetCount()} permissions and {this.Roles.GetCount()} roles for {this.GetType().FullName}.");
        }
        catch (Exception e)
        {
            BPLogger.Error($"Caught an exception while loading roles and permissions.");
            BPLogger.Debug($"Exception: {e}");
        }
    }

    private List<MethodInfo> GetAllMethodsWithAttribute<TItem>(Type[]? typeList = null)
        where TItem : Attribute
    {
        Type[] types = typeList ?? this.GetType().Assembly.GetTypes();
        List<MethodInfo> methodInfos = new List<MethodInfo>();
        foreach (Type type in types)
        {
            // Ensure the type isn't abstract.
            if (type.IsAbstract)
            {
                continue;
            }

            // Ensure the type is not Obsolete.
            if (type.GetCustomAttribute<ObsoleteAttribute>() is not null)
            {
                continue;
            }

            // Ensure that the server is on an allowed port.
            AllowedPortsAttribute? allowedPorts = type.GetCustomAttribute<AllowedPortsAttribute>();

            if (allowedPorts is not null && !allowedPorts.ValidPorts.Contains(ServerStatic.ServerPort))
            {
                BPLogger.Warn($"Type '{type.FullName}' skipped due to not having valid port selection.");
                continue;
            }

            // If this server doesn't use the server info system, skip this part.
            if (this is not IServerInfo<TConfig> info || info.ServerInfo is null || info.ServerInfo.PrimaryKey is null)
            {
                goto skipServerCheck;
            }

            // If this server is non debugging and the feature is a debug command, skip this part.
            if (!info.ServerInfo.PrimaryKey.EnableDebugFeatures &&
                type.GetCustomAttribute<DebugFeatureAttribute>() is not null)
            {
                BPLogger.Warn($"Type '{type.FullName}' skipped due to being on a non-debugging server.");
                continue;
            }

            List<AllowedServersAttribute> allowedServers = type.GetCustomAttributes<AllowedServersAttribute>().ToList();

            // If the command doesnt have any allowed server attributes, skip this part.
            if (allowedServers.Count < 1)
            {
                goto skipServerCheck;
            }

            // If the command isn't allowed on this server, skip this part.
            if (allowedServers.Any(x => x.ValidServers.Any(type1 => info.ServerInfo.PrimaryKey.GetType() != type1)))
            {
                goto skipServerCheck;
            }

            BPLogger.Warn($"Type '{type.FullName}' skipped because it is not enabled on this server.");
            continue;

            // Create an instance of the feature.
            skipServerCheck:
            List<MethodInfo> methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList();
            foreach (MethodInfo methodInfo in methods)
            {
                if (methodInfo.GetCustomAttributes<TItem>().Any())
                {
                    methodInfos.Add(methodInfo);
                }
            }
        }

        return methodInfos;
    }

    private List<TItem> GetAllInstancesOfItem<TItem>()
    {
        Type[] types = this.GetType().Assembly.GetTypes();
        List<TItem> items = new List<TItem>();
        foreach (Type type in types)
        {
            try
            {
                // Ensure the item is of the right type and isn't abstract.
                if (!type.IsSubclassOf(typeof(TItem)) || type.IsAbstract)
                {
                    continue;
                }

                // Ensure the type is not Obsolete.
                if (type.GetCustomAttribute<ObsoleteAttribute>() is not null)
                {
                    continue;
                }

                // Ensure that the server is on an allowed port.
                AllowedPortsAttribute? allowedPorts = type.GetCustomAttribute<AllowedPortsAttribute>();

                if (allowedPorts is not null && !allowedPorts.ValidPorts.Contains(ServerStatic.ServerPort))
                {
                    BPLogger.Warn($"Item '{type.FullName}' skipped due to not having valid port selection.");
                    continue;
                }

                // If this server doesn't use the server info system, skip this part.
                if (this is not IServerInfo<TConfig> info || info.ServerInfo is null || info.ServerInfo.PrimaryKey is null)
                {
                    goto skipServerCheck;
                }

                // If this server is non debugging and the item is a debug item, skip this part.
                if (!info.ServerInfo.PrimaryKey.EnableDebugFeatures &&
                    type.GetCustomAttribute<DebugFeatureAttribute>() is not null)
                {
                    BPLogger.Warn($"Item '{type.FullName}' skipped due to being on a non-debugging server.");
                    continue;
                }

                List<AllowedServersAttribute> allowedServers =
                    type.GetCustomAttributes<AllowedServersAttribute>().ToList();

                // If the item doesnt have any allowed server attributes, skip this part.
                if (allowedServers.Count < 1)
                {
                    goto skipServerCheck;
                }

                // If the item isn't allowed on this server, skip this part.
                if (allowedServers.Any(x => x.ValidServers.Any(type1 => info.ServerInfo.PrimaryKey.GetType() != type1)))
                {
                    goto skipServerCheck;
                }

                BPLogger.Warn($"Item '{type.FullName}' skipped because it is not enabled on this server.");
                continue;

                // Create an instance of the feature.
                skipServerCheck:
                try
                {
                    TItem feature = (TItem)Activator.CreateInstance(type, nonPublic: true);
                    if (feature is null)
                    {
                        BPLogger.Warn("Item was null after being instantiated. Skipping Item.");
                        continue;
                    }

                    items.Add(feature);
                }
                catch (Exception e)
                {
                    BPLogger.Warn($"Item '{type.FullName}' could not be instantiated because of an error.");
                    BPLogger.Warn($"Exception: \n{e}");
                }
            }
            catch (Exception e)
            {
                BPLogger.Warn($"An item could not be instantiated because of an error.");
                BPLogger.Warn($"Exception: \n{e}");
            }
        }

        return items;
    }
}