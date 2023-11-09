namespace BananaPlugin.API.Attributes;

using BananaPlugin.API.Main;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// An attribute that specifies a <see cref="BananaFeature"/> must only enable on certain server instances.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class AllowedServersAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedServersAttribute"/> class.
    /// </summary>
    /// <param name="servers">A list of <see cref="ServerInfo"/> instances that are allowed to use the feature.</param>
    public AllowedServersAttribute(params Type[] servers)
    {
        List<Type> info = new ();
        foreach (Type server in servers)
        {
            if (server == typeof(ServerInfo))
            {
                continue;
            }

            Type prevType = server;
            while (true)
            {
                if (prevType.BaseType is null)
                {
                    break;
                }

                if (prevType.BaseType != typeof(ServerInfo))
                {
                    prevType = prevType.BaseType;
                    continue;
                }

                info.Add(server);
                break;
            }
        }

        this.ValidServers = info.ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AllowedServersAttribute"/> class.
    /// </summary>
    /// <param name="server">The <see cref="ServerInfo"/> that is allowed to use the feature.</param>
    public AllowedServersAttribute(Type server)
    {
        this.ValidServers = Array.Empty<Type>();
        if (server == typeof(ServerInfo))
        {
            return;
        }

        Type prevType = server;
        while (true)
        {
            if (prevType.BaseType is null)
            {
                break;
            }

            if (prevType.BaseType != typeof(ServerInfo))
            {
                prevType = prevType.BaseType;
                continue;
            }

            this.ValidServers = new[] { server };
            break;
        }
    }

    /// <summary>
    /// Gets the valid ports associated with this attribute.
    /// </summary>
    public Type[] ValidServers { get; }
}
