namespace BananaPlugin.API;

/// <summary>
/// Consists of all banana bungalow staff ranks.
/// </summary>
public enum BRank
{
    /// <summary>
    /// The default user rank.
    /// </summary>
    User = 0,

    /// <summary>
    /// The junior moderator rank.
    /// </summary>
    JuniorModerator = 1,

    /// <summary>
    /// The moderator rank.
    /// </summary>
    Moderator = 2,

    /// <summary>
    /// The senior moderator rank.
    /// </summary>
    SeniorModerator = 3,

    /// <summary>
    /// The junior administrator rank.
    /// </summary>
    JuniorAdministrator = 4,

    /// <summary>
    /// The administrator rank.
    /// </summary>
    Administrator = 5,

    /// <summary>
    /// The head administrator rank.
    /// </summary>
    HeadAdministrator = 6,

    /// <summary>
    /// The senior administrator rank.
    /// </summary>
    SeniorAdministrator = 7,

    /// <summary>
    /// The developer rank.
    /// </summary>
    Developer = int.MaxValue,
}
