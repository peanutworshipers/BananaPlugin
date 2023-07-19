namespace BananaPlugin.API.Utils;

using Exiled.Events.EventArgs.Player;
using System.Collections.Generic;

/// <summary>
/// A utility class for accessing the player list.
/// </summary>
/// <remarks>Lists are 2-4x faster than HashSets and Dictionaries.</remarks>
public static class PlayerListUtils
{
    static PlayerListUtils()
    {
        VerifiedHubs = new(100);
        AllHubs = new(100);
        VerifiedPlayers = new(100);
        AllPlayers = new(100);

        foreach (ReferenceHub hub in ReferenceHub.AllHubs)
        {
            if (!ExPlayer.TryGet(hub, out ExPlayer player))
            {
                continue;
            }

            if (hub.Mode == ClientInstanceMode.ReadyClient)
            {
                VerifiedHubs.Add(hub);
                VerifiedPlayers.Add(player);
            }

            AllHubs.Add(hub);
            AllPlayers.Add(player);
        }

        ExHandlers.Player.Joined += Joined;
        ExHandlers.Player.Destroying += Destroying;
        ExHandlers.Player.Verified += Verified;
    }

    public static List<ReferenceHub> VerifiedHubs { get; }

    public static List<ReferenceHub> AllHubs { get; }

    public static List<ExPlayer> VerifiedPlayers { get; }

    public static List<ExPlayer> AllPlayers { get; }

    private static void Joined(JoinedEventArgs ev)
    {
        AllHubs.Add(ev.Player.ReferenceHub);
        AllPlayers.Add(ev.Player);
        BPLogger.Debug("AllHubs Add");
    }

    private static void Destroying(DestroyingEventArgs ev)
    {
        if (AllHubs.Remove(ev.Player.ReferenceHub))
        {
            BPLogger.Debug("AllHubs Remove");
        }

        if (AllPlayers.Remove(ev.Player))
        {
            BPLogger.Debug("AllPlayers Remove");
        }

        if (VerifiedHubs.Remove(ev.Player.ReferenceHub))
        {
            BPLogger.Debug("VerifiedHubs Remove");
        }

        if (VerifiedPlayers.Remove(ev.Player))
        {
            BPLogger.Debug("VerifiedPlayers Remove");
        }
    }

    private static void Verified(VerifiedEventArgs ev)
    {
        VerifiedHubs.Add(ev.Player.ReferenceHub);
        VerifiedPlayers.Add(ev.Player);
        BPLogger.Debug("VerifiedHubs Add");
    }
}
