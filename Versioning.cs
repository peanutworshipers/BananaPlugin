namespace BananaPlugin;

using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Used to represent the current version of the banana plugin assembly.
/// </summary>
public static class Versioning
{
    /// <summary>
    /// Gets the full string representation of the current assembly version.
    /// </summary>
    public const string FullVersionString = VersionString + Extension;

    /// <summary>
    /// The string representation of the current assembly version.
    /// </summary>
    public const string VersionString = "1.3.1";

    /// <summary>
    /// The extension of the current assembly version.
    /// </summary>
    public const string Extension = "";

    /// <summary>
    /// Represents the regular expression that is used to detect valid semantic versions.
    /// </summary>
    public const string SemanticVersioningRegex = @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";

    /// <summary>
    /// Specifies if the current build is a debug build.
    /// </summary>
    public const bool DebugBuild =
#if DEBUG
        true;
#else
        false;
#endif

    /// <summary>
    /// Specifies if the current build is a local build.
    /// </summary>
    /// <remarks>Local builds are intended for localhost dedicated server testing before release.</remarks>
    public const bool LocalBuild =
#if LOCAL
        true;
#else
        false;
#endif

    /// <summary>
    /// Specifies if the current build is a release build.
    /// </summary>
    public const bool ReleaseBuild = !DebugBuild;

    private const string ChangelogsResource = "BananaPlugin.changelogs.txt";

    static Versioning()
    {
        string[] split = VersionString.Split('.');

        Version = new (VersionString);

        Major = Version.Major;
        Minor = Version.Minor;
        Build = Version.Build;

        using StreamReader stream = new (typeof(Versioning).Assembly.GetManifestResourceStream(ChangelogsResource));

        HashSet<string> versions = new();
        Regex regex = new (SemanticVersioningRegex);

        string? line;
        while ((line = stream.ReadLine()) is not null)
        {
            if (regex.IsMatch(line))
            {
                versions.Add(line);
            }
        }

        AllVersions = versions;
    }

    /// <summary>
    /// Gets the major of the current assembly version.
    /// </summary>
    public static int Major { get; }

    /// <summary>
    /// Gets the minor the currrent assembly version.
    /// </summary>
    public static int Minor { get; }

    /// <summary>
    /// Gets the build of the current assembly version.
    /// </summary>
    public static int Build { get; }

    /// <summary>
    /// Gets the current assembly version.
    /// </summary>
    public static Version Version { get; }

    /// <summary>
    /// Gets all the versions of this assembly that have been built.
    /// </summary>
    public static HashSet<string> AllVersions { get; }

    /// <summary>
    /// Gets the changelog for the specified version.
    /// </summary>
    /// <param name="version">The version to get the changelog for.</param>
    /// <param name="changelog">The changelog, if found.</param>
    /// <returns>A value indicating whether the operation was a success.</returns>
    public static bool GetChangelog(string version, [NotNullWhen(true)] out string? changelog)
    {
        if (!AllVersions.Contains(version))
        {
            changelog = null;
            return false;
        }

        using StreamReader stream = new (typeof(Versioning).Assembly.GetManifestResourceStream(ChangelogsResource));

        StringBuilder strBuilder = StringBuilderPool.Shared.Rent();
        Regex regex = new (SemanticVersioningRegex);

        bool versionFound = false;
        string? line;

        while ((line = stream.ReadLine()) is not null)
        {
            if (!versionFound)
            {
                if (line == version)
                {
                    versionFound = true;
                }

                continue;
            }

            if (regex.IsMatch(line))
            {
                break;
            }

            strBuilder.AppendLine(line);
        }

        if (!versionFound)
        {
            StringBuilderPool.Shared.Return(strBuilder);
            changelog = null;
            return false;
        }

        changelog = StringBuilderPool.Shared.ToStringReturn(strBuilder);
        return true;
    }
}
