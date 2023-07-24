namespace BananaPlugin.API.Utils;

using System.Collections.Generic;

/// <summary>
/// A class that handles the plugin developers.
/// </summary>
internal static class DeveloperUtils
{
    /// <summary>
    /// Contains all registered developers.
    /// </summary>
    public static readonly IReadOnlyList<DeveloperInfo> Developers = new List<DeveloperInfo>()
    {
        new ("Zereth", "76561198288227848@steam"),
        new ("Zereth", "300607485738221569@discord"),

        new ("Skillz", "76561198397906373@steam"),
        new ("Skillz", "330362707029131265@discord"),
    };

    /// <summary>
    /// Determines if the userid belongs to a developer.
    /// </summary>
    /// <param name="userId">The userid to check.</param>
    /// <returns>A value indicating whether the userid belongs to a developer.</returns>
    public static bool IsDeveloper(string userId)
    {
        for (int i = 0; i < Developers.Count; i++)
        {
            if (userId == Developers[i])
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// A struct containing developer info.
    /// </summary>
    public struct DeveloperInfo
    {
        /// <summary>
        /// The name of the developer.
        /// </summary>
        public string Name;

        /// <summary>
        /// The userid of the developer.
        /// </summary>
        public string UserId;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeveloperInfo"/> struct.
        /// </summary>
        /// <param name="name">The name of the developer.</param>
        /// <param name="userid">The userid of the developer.</param>
        public DeveloperInfo(string name, string userid)
        {
            this.Name = name;
            this.UserId = userid;
        }

        public static implicit operator string(DeveloperInfo info) => info.UserId;
    }
}
