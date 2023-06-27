namespace BananaPlugin.API;

/// <summary>
/// The enum used to identify the priority of an event change (Used internally).
/// </summary>
public enum EventChangePriority
{
    /// <summary>
    /// Indicates that no modifications have been made.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates that a feature has made a modification.
    /// </summary>
    Feature,

    /// <summary>
    /// Indicates that a feature has made a modification of higher priority.
    /// </summary>
    HigherFeature,

    /// <summary>
    /// Indicates that a command has made a modification.
    /// </summary>
    AdminCommand,

    /// <summary>
    /// Indicates that a change has been made of full priority, and should not be changed further.
    /// </summary>
    FullPriority = int.MaxValue,
}
