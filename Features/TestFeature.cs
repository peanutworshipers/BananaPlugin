namespace BananaPlugin.Features;

using BananaPlugin.API.Main;

#if DEBUG
/// <summary>
/// The test feature used for debugging.
/// </summary>
public sealed class TestFeature : BananaFeature
{
    private TestFeature()
    {
    }

    /// <inheritdoc/>
    public override string Name => "Test Feature";

    /// <inheritdoc/>
    public override string Prefix => "test";

    /// <inheritdoc/>
    protected override void Enable()
    {
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
    }
}
#endif
