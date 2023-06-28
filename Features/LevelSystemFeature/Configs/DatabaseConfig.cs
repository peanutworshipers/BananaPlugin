namespace BananaPlugin.Features.LevelSystemFeature.Configs;

using System.ComponentModel;

/// <summary>
/// The class used to represent database configurations for the level system.
/// </summary>
/// <remarks>Credits to <see href="https://github.com/creepycats"/> for original source.</remarks>
public sealed class DatabaseConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether players' levels should stored without the use of HarperDB.
    /// </summary>
    [Description("Should Players' Levels be stored without the use of HarperDB? Either way, a Local Database file will be used")]
    public bool Serverless { get; set; } = true;

    /// <summary>
    /// Gets or sets where the database file is saved.
    /// </summary>
    [Description("Where the Database file is saved")]
    public string LocalDatabasePath { get; set; } = $@"{ExFeatures.Paths.Configs}\SCPLeveling";

    /// <summary>
    /// Gets or sets the name of the database file.
    /// </summary>
    [Description("The name of the Database File")]
    public string LocalDatabaseFileName { get; set; } = "Levels.db";

    /// <summary>
    /// Gets or sets the HarperDB server database url.
    /// </summary>
    [Description("HarperDB Server Database Url")]
    public string HarperDatabaseURL { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HarperDB server database username.
    /// </summary>
    [Description("HarperDB Server Database Username")]
    public string HarperDatabaseUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HarperDB server database password.
    /// </summary>
    [Description("HarperDB Server Database Password")]
    public string HarperDatabasePassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HarperDB server database Schema and Table (ex: scpsl.levels).
    /// </summary>
    [Description("HarperDB Server Database Schema and Table (example: scpsl.levels)")]
    public string HarperDatabaseSchemaTable { get; set; } = string.Empty;
}
