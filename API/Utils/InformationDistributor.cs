namespace BananaPlugin.API.Utils;

using BananaPlugin.Extensions;

/// <summary>
/// The main class responsible for distributing information to specified players.
/// </summary>
/// <remarks>Information is usually provided to players of specified rank, status, or global flags; ex: Global Moderators.</remarks>
public static class InformationDistributor
{
    /// <summary>
    /// Broadcasts an admin message to the players with a specified rank.
    /// </summary>
    /// <param name="rank">The rank requirement.</param>
    /// <param name="message">The message to send.</param>
    public static void AdminBroadcastRankedPlayers(this BRank rank, string message)
    {
        message = "@" + message;

        foreach (ExPlayer ply in ExPlayer.List)
        {
            if (!ply.Sender.CheckPermission(rank, out _))
            {
                continue;
            }

            ply.Sender.RaReply(message, true, false, string.Empty);
        }
    }
}
