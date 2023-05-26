namespace BananaPlugin.API.Utils;

using BananaPlugin.API.Main;
using Discord;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Diagnostics;
#if !LOCAL
using System.Reflection;
#endif

/// <summary>
/// A class used to sort logs by their feature and type.
/// </summary>
public sealed class BPLogger
{
    private static readonly List<BPLogger> Loggers = new ();

    private static readonly Dictionary<string, (string, string)> Identifiers = new ();

#if !DEBUG
    private static readonly Assembly Assembly = typeof(BPLogger).Assembly;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="BPLogger"/> class.
    /// </summary>
    /// <param name="feature">The feature to use as a parent.</param>
    internal BPLogger(BananaFeature feature)
    {
        this.Feature = feature;
        Loggers.Add(this);
    }

    /// <summary>
    /// Gets the feature associated with this logger.
    /// </summary>
    public BananaFeature Feature { get; }

    /// <summary>
    /// Gets the log name associated with this logger.
    /// </summary>
    public string LogName => this.Feature.Name;

    /// <summary>
    /// Forces the logger to identify the calling method as the specified method and declaring type.
    /// </summary>
    /// <param name="typeName">The type name to identify as.</param>
    /// <param name="methodName">The method name to identify as.</param>
    internal static void IdentifyMethodAs(string typeName, string methodName)
    {
        MethodBase method = GetCallingMethod();

        Identifiers[GetFullMethodName(method)] = (typeName, methodName);
    }

    /// <summary>
    /// Logs an info message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal static void Info(string message)
    {
        LogMessage($"[BP:{GetCallerString()}] {message}", LogLevel.Info);
    }

    /// <summary>
    /// Logs a warn message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal static void Warn(string message)
    {
        LogMessage($"[BP:{GetCallerString()}] {message}", LogLevel.Warn);
    }

    /// <summary>
    /// Logs an error message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal static void Error(string message)
    {
        LogMessage($"[BP:{GetCallerString()}] {message}", LogLevel.Error);
    }

    /// <summary>
    /// Logs a debug message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal static void Debug(string message)
    {
        LogMessage($"[BP:{GetCallerString()}] {message}", LogLevel.Debug);
    }

    /// <summary>
    /// Logs an info message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal void FeatureInfo(string message)
    {
        LogMessage($"[BP+{this.LogName}-{GetCallerString(false)}] {message}", LogLevel.Info, this);
    }

    /// <summary>
    /// Logs a warn message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal void FeatureWarn(string message)
    {
        LogMessage($"[BP+{this.LogName}-{GetCallerString(false)}] {message}", LogLevel.Warn, this);
    }

    /// <summary>
    /// Logs an error message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal void FeatureError(string message)
    {
        LogMessage($"[BP+{this.LogName}-{GetCallerString(false)}] {message}", LogLevel.Error, this);
    }

    /// <summary>
    /// Logs a debug message to the console.
    /// </summary>
    /// <param name="message">The message to log.</param>
    internal void FeatureDebug(string message)
    {
        LogMessage($"[BP+{this.LogName}-{GetCallerString(false)}] {message}", LogLevel.Debug, this);
    }

    private static void LogMessage(string message, LogLevel logType, BPLogger? feature = null)
    {
        ConsoleColor color = logType switch
        {
            LogLevel.Error => ConsoleColor.DarkRed,
            LogLevel.Warn => ConsoleColor.Magenta,
            LogLevel.Info => ConsoleColor.Cyan,
            LogLevel.Debug => ConsoleColor.Green,
            _ => throw new ArgumentOutOfRangeException(nameof(logType)),
        };

#if !LOCAL
        if (logType != LogLevel.Debug || Log.DebugEnabled.Contains(Assembly))
        {
            Log.SendRaw(message, color);
        }
#else
        Log.SendRaw(message, color);
#endif
    }

    private static string GetCallerString(bool includeType = true)
    {
        MethodBase method = GetCallingMethod(1);

        string result = !Identifiers.TryGetValue(GetFullMethodName(method), out (string, string) identifier)
            ? includeType
                ? $"{method.DeclaringType.Name}::{method.Name}"
                : $"{method.Name}]"
            : includeType
                ? $"{identifier.Item1}::{identifier.Item2}"
                : $"{identifier.Item2}";

        return result;
    }

    private static MethodBase GetCallingMethod(int skip = 0)
    {
        StackTrace stack = new (2 + skip);

        return stack.GetFrame(0).GetMethod();
    }

    private static string GetFullMethodName(MethodBase methodBase)
    {
        return methodBase.DeclaringType.FullName + "::" + methodBase.Name;
    }

    /// <summary>
    /// A log struct containing basic log information.
    /// </summary>
    public readonly struct LogInfo
    {
        /// <summary>
        /// The log level of this log.
        /// </summary>
        public readonly byte Level;

        /// <summary>
        /// The message of this log.
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// The feature name of this log.
        /// </summary>
        public readonly string? FeatureName;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogInfo"/> struct.
        /// </summary>
        /// <param name="level">The level of the log.</param>
        /// <param name="message">The message of the log.</param>
        /// <param name="featureName">The feature name of this log.</param>
        public LogInfo(LogLevel level, string message, string? featureName)
        {
            this.Level = (byte)level;
            this.Message = message;
            this.FeatureName = featureName;
        }
    }
}
