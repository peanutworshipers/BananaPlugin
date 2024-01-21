namespace BananaPlugin.API.Collections;

using Main;
using System.Collections.Generic;
using Interfaces;

/// <summary>
/// Used to contain all <see cref="ServerInfo"/> instances for a <see cref="BananaPlugin{TConfig}"/>.
/// </summary>
// ReSharper disable UnusedParameter.Local
public sealed class ServerInfoCollection : Collection<ServerInfo>, ICollectionPrimaryKey<ServerInfo>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServerInfoCollection"/> class.
    /// </summary>
    /// <param name="primaryKey">The primary server.</param>
    /// <param name="values">The servers to add.</param>
    public ServerInfoCollection(ServerInfo primaryKey, List<ServerInfo> values)
    {
        this.PrimaryKey = primaryKey;
    }

    /// <inheritdoc />
    public ServerInfo PrimaryKey { get; }
}
