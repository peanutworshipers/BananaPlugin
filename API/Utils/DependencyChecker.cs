namespace BananaPlugin.API.Utils;

using System;

/// <summary>
/// A utility class for checking if dependencies are loaded.
/// </summary>
public static class DependencyChecker
{
    /// <summary>
    /// An enumeration that specifies dependencies.
    /// </summary>
    [Flags]
    public enum Dependency
    {
        /// <summary>
        /// Represents the Push dependency.
        /// </summary>
        Push = 1 << 0,

        /// <summary>
        /// Represents the SCP294 dependency.
        /// </summary>
        SCP294 = 1 << 1,

        /// <summary>
        /// Represents the MapEditorReborn dependency.
        /// </summary>
        MapEditorReborn = 1 << 2,
    }

    /// <summary>
    /// Checks if the specified dependencies are loaded.
    /// </summary>
    /// <param name="dependency">The dependencies to check.</param>
    /// <returns>A value indicating whether all the specified dependencies are loaded.</returns>
    public static bool CheckDependencies(Dependency dependency)
    {
        try
        {
            return InternalCheckDependency(dependency);
        }
        catch
        {
            return false;
        }
    }

    private static bool InternalCheckDependency(Dependency dependency)
    {
        bool result = true;

        if ((dependency & Dependency.Push) != 0)
        {
            result &= CheckPush();
        }

        if ((dependency & Dependency.SCP294) != 0)
        {
            result &= CheckSCP294();
        }

        if ((dependency & Dependency.MapEditorReborn) != 0)
        {
            result &= CheckMapEditorReborn();
        }

        return result;
    }

    private static bool CheckSCP294() => typeof(Scp294Plugin) is not null;

    private static bool CheckPush() => typeof(Push.Push) is not null;

    private static bool CheckMapEditorReborn() => typeof(MapEditorReborn.MapEditorReborn) is not null;
}
