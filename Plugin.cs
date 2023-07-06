namespace BananaPlugin;

using BananaPlugin.API.Collections;
using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.Extensions;
using Exiled.API.Features;
using HarmonyLib;
using MEC;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

/// <summary>
/// The main plugin class for this assembly.
/// </summary>
public sealed class Plugin : Plugin<Config>
{
    private bool enabled = false;

    static Plugin()
    {
        Harmony = new Harmony("com.zereth.bananaplugin");

        MECExtensions.Run(ApplyPatches, Segment.EndOfFrame);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    public Plugin()
    {
        if (Instance is not null)
        {
            throw new InvalidOperationException("Only one plugin instance can be created.");
        }
    }

    /// <summary>
    /// Gets the plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <summary>
    /// Gets the feature collection.
    /// </summary>
    public static FeatureCollection? Features { get; private set; }

    /// <summary>
    /// Gets the harmony instance for this assembly.
    /// </summary>
    public static Harmony Harmony { get; }

    /// <inheritdoc/>
    public override string Author => "Zereth";

    /// <inheritdoc/>
    public override string Name => "BananaPlugin";

    /// <inheritdoc/>
    public override string Prefix => "banana_plugin";

    /// <inheritdoc/>
    public override Version RequiredExiledVersion => new (7, 0, 0);

    /// <inheritdoc/>
    public override Version Version => Versioning.Version;

    /// <summary>
    /// Checks that the static instances are not null.
    /// </summary>
    /// <returns>A value indicating whether the instances are not null.</returns>
    [MemberNotNullWhen(true, nameof(Instance), nameof(Features))]
    public static bool CheckInstances()
    {
        if (Instance == null)
        {
            return false;
        }

        Instance.EnsureInstances();
        return true;
    }

    /// <summary>
    /// Checks if the current plugin Instance is assigned and enabled.
    /// </summary>
    /// <returns>A value indicating whether the plugin is assigned and enabled.</returns>
    [MemberNotNullWhen(true, nameof(Instance), nameof(Features))]
    public static bool AssertEnabled()
    {
        return CheckInstances() && Instance.enabled;
    }

    /// <summary>
    /// Checks if the current plugin Instance is assigned and enabled.
    /// </summary>
    /// <param name="response">The error response.</param>
    /// <returns>A value indicating whether the plugin is assigned and enabled.</returns>
    [MemberNotNullWhen(true, nameof(Instance), nameof(Features))]
    public static bool AssertEnabled([NotNullWhen(false)] out string? response)
    {
        if (!CheckInstances() || !Instance.enabled)
        {
            response = "This plugin is currently disabled.";
            return false;
        }

        response = null;
        return true;
    }

    /// <inheritdoc/>
    public override void OnEnabled()
    {
        if (this.enabled)
        {
            return;
        }

        this.enabled = true;

        if (Features is not null)
        {
            return;
        }

        this.EnsureInstances();

        Type[] types = this.Assembly.GetTypes();

        foreach (Type type in types)
        {
            if (!type.IsSubclassOf(typeof(BananaFeature)) || type.IsAbstract)
            {
                continue;
            }

            BananaFeature feature = (BananaFeature)Activator.CreateInstance(type, nonPublic: true);

            if (!Features.TryAddFeature(feature, out string? response))
            {
                BPLogger.Error(response);
            }
            else
            {
                feature.Enabled = true;
            }
        }

        Features.MarkAsLoaded();

        base.OnEnabled();
    }

    /// <inheritdoc/>
    public override void OnDisabled()
    {
        if (!this.enabled || Features is null)
        {
            return;
        }

        foreach (BananaFeature feature in Features)
        {
            feature.Enabled = false;
        }

        this.enabled = false;

        base.OnDisabled();
    }

    private static IEnumerator<float> ApplyPatches()
    {
        BPLogger.IdentifyMethodAs(nameof(Plugin), nameof(ApplyPatches));

        Type[] types = AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly());

        int failCount = 0;

        for (int i = 0; i < types.Length; i++)
        {
            Type type = types[i];

            try
            {
                Harmony.CreateClassProcessor(type).Patch();
            }
            catch (HarmonyException e)
            {
                failCount++;

                BPLogger.Error($"Failed to apply harmony patch: [{type.FullName}]\n{e}");
            }
        }

        if (failCount > 0)
        {
            BPLogger.Error($"Failed to apply {failCount} patches.");
        }

        yield break;
    }

    [MemberNotNull(nameof(Instance), nameof(Features))]
    private void EnsureInstances()
    {
        Instance ??= this;
        Features ??= new ();
        return;
    }
}