namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.Features.Configs;
using System;

/// <summary>
/// The main feature responsible for levels.
/// </summary>
[Obsolete]
public sealed class LevelSystem : BananaFeatureConfig<CfgLevelSystem>
{
    /// <inheritdoc/>
    public override string Name => "Level System";

    /// <inheritdoc/>
    public override string Prefix => "lvl";

    /// <inheritdoc/>
    protected override void Enable()
    {
    }

    /// <inheritdoc/>
    protected override void Disable() => throw new System.NotSupportedException();

    /// <inheritdoc/>
    protected override CfgLevelSystem RetrieveLocalConfig(Config config) => config.LevelSystem;
}
