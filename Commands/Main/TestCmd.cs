namespace BananaPlugin.Commands.Main;

#if DEBUG
using BananaPlugin.API;
using BananaPlugin.Extensions;
using CommandSystem;
using System;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The command responsible for testing new features.
/// </summary>
/// <remarks><b>The contents of this command are changed when testing and should not be in release builds.</b>b>.</remarks>
public sealed class TestCmd : ICommand
{
    /// <inheritdoc/>
    public string Command => "test";

    /// <inheritdoc/>
    public string[] Aliases => Array.Empty<string>();

    /// <inheritdoc/>
    public string Description => "A testing command.";

    /// <inheritdoc/>
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [NotNull] out string? response)
    {
        if (!Plugin.AssertEnabled(out response))
        {
            return false;
        }

        if (!sender.CheckPermission(BRank.Developer, out response))
        {
            return false;
        }

        // testing stuff here.
        response = "Done.";
        return true;
    }
}
#endif
