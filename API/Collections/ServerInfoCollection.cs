namespace BananaPlugin.API.Collections;

using BananaPlugin.API.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Exiled.Events.Handlers;
using Interfaces;
using JetBrains.Annotations;

/// <summary>
/// Used to contain all <see cref="ServerInfo"/> instances for a <see cref="BpPlugin"/>.
/// </summary>
public sealed class ServerInfoCollection : Collection<ServerInfo>, ICollectionPrimaryKey<ServerInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerInfoCollection"/> class.
    /// </summary>
    public ServerInfoCollection(ServerInfo? primaryKey, List<ServerInfo>? values)
    {
        this.PrimaryKey = primaryKey;
    }

    /// <inheritdoc />
    public ServerInfo? PrimaryKey { get; }

}
