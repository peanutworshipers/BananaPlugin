namespace BananaPlugin.API.Utils;

using System.Collections.Generic;

/// <summary>
/// A utility class for retreiving server ports.
/// </summary>
public static class ServerPorts
{
    /// <summary>
    /// The port of server one.
    /// </summary>
    public const ushort ServerOne = 7777;

    /// <summary>
    /// The port of server two.
    /// </summary>
    public const ushort ServerTwo = 7780;

    /// <summary>
    /// The port of server three.
    /// </summary>
    public const ushort ServerThree = 7781;

    /// <summary>
    /// The port of the testing server.
    /// </summary>
    public const ushort TestServer = 7778;

    /// <summary>
    /// Gets the name of the server by its port.
    /// </summary>
    public static readonly Dictionary<ushort, string> ServerNames;

    static ServerPorts()
    {
        ServerNames = new()
        {
            { ServerOne, "Server #1" },
            { ServerTwo, "Server #2" },
            { ServerThree, "Server #3" },
            { TestServer, "Test Server" },
        };
    }
}
