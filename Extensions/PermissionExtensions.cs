namespace BananaPlugin.Extensions;

using BananaPlugin.API;
using BananaPlugin.API.Utils;
using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Consists of permission extensions for checking banana plugin based permissions.
/// </summary>
public static class PermissionExtensions
{
    /// <summary>
    /// Checks if a player has the specified banana bungalow staff rank.
    /// </summary>
    /// <param name="sender">The command sender.</param>
    /// <param name="rank">The rank to check for.</param>
    /// <param name="response">The error response.</param>
    /// <returns>A value indicating whether the sender has that rank permission.</returns>
    public static bool CheckPermission(this ICommandSender sender, BRank rank, [NotNullWhen(false)] out string? response)
    {
        if (sender is CommandSender cSender && cSender.FullPermissions)
        {
            response = null;
            return true;
        }

        if (rank == 0)
        {
            response = null;
            return true;
        }

        if (rank == BRank.Developer)
        {
            if (sender is not PlayerCommandSender pSender)
            {
                response = "You must be a player to use this command.";
                return false;
            }

            if (!DeveloperUtils.IsDeveloper(pSender.SenderId))
            {
                response = $"You dont have access to this command. Missing rank: Developer";
                return false;
            }

            response = null;
            return true;
        }
        else if (sender is PlayerCommandSender pSender && DeveloperUtils.IsDeveloper(pSender.SenderId))
        {
            response = null;
            return true;
        }

        foreach (string perm in rank.GetPossiblePermissions())
        {
            if (sender.CheckPermission(perm))
            {
                response = null;
                return true;
            }
        }

        response = $"You dont have access to this command. Missing rank: {rank}";
        return false;
    }

    /// <summary>
    /// Gets all possible string permissions for the specified banana bungalow staff rank.
    /// </summary>
    /// <param name="rank">The rank to get string permissions for.</param>
    /// <returns>An enumerable containing the collection of string permissions.</returns>
    public static IEnumerable<string> GetPossiblePermissions(this BRank rank)
    {
        BRank[] values = (BRank[])Enum.GetValues(typeof(BRank));

        // If the rank is below or equal to the
        // listed one, the listed permission will be added.
        //
        // This allows for higher ranked staff to
        // have access to lower rank permissions.
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == 0 || values[i] == BRank.Developer)
            {
                continue;
            }

            if (rank <= values[i])
            {
                yield return GetPermission(values[i]);
            }
        }
    }

    /// <summary>
    /// Gets the exact permission string of the specified rank.
    /// </summary>
    /// <param name="rank">The rank to retrieve.</param>
    /// <returns>A string representing the rank's permission.</returns>
    /// <exception cref="ArgumentOutOfRangeException">ArgumentOutOfRangeException.</exception>
    public static string GetPermission(this BRank rank)
    {
        return rank switch
        {
            BRank.JuniorModerator => "bananaplugin.juniormod",
            BRank.Moderator => "bananaplugin.moderator",
            BRank.SeniorModerator => "bananaplugin.seniormod",
            BRank.JuniorAdministrator => "bananaplugin.jradmin",
            BRank.Administrator => "bananaplugin.admin",
            BRank.HeadAdministrator => "bananaplugin.headadmin",
            BRank.SeniorAdministrator => "bananaplugin.senioradmin",
            _ => throw new ArgumentOutOfRangeException(nameof(rank)),
        };
    }
}
