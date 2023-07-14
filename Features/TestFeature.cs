namespace BananaPlugin.Features;

using BananaPlugin.API;
using BananaPlugin.API.Interfaces;

#if DEBUG
using BananaPlugin.API.Main;
using BananaPlugin.Extensions;
using CommandSystem;
using System;

/// <summary>
/// The test feature used for debugging.
/// </summary>
public sealed class TestFeature : BananaFeature
{
    private TestFeature()
    {
        this.Commands = new ICommand[]
        {
            new TestCommand(this),
        };
    }

    /// <inheritdoc/>
    public override string Name => "Test Feature";

    /// <inheritdoc/>
    public override string Prefix => "test";

    /// <inheritdoc/>
    public override ICommand[] Commands { get; }

    /// <inheritdoc/>
    protected override void Enable()
    {
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
    }

    private sealed class TestCommand : IFeatureSubcommand<TestFeature>, IRequiresRank, IHiddenCommand
    {
        public TestCommand(TestFeature parent)
        {
            this.Parent = parent;
        }

        public TestFeature Parent { get; }

        public string Command => "test";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "A command used for testing.";

        public string[] Usage => Array.Empty<string>();

        public BRank RankRequirement => BRank.Developer;

        public string GetHelp(ArraySegment<string> arguments)
        {
            return this.HelpProviderFormat("Time to test!");
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string? response)
        {
            if (!sender.CheckPermission(BRank.Developer, out response))
            {
                return false;
            }

            response = "Testing...";
            return true;
        }
    }
}
#endif
