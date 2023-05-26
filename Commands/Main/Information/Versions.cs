namespace BananaPlugin.Commands.Main.Information;

using CommandSystem;
using NorthwoodLib.Pools;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

/// <summary>
/// The command responsible for listing banana plugin versions.
/// </summary>
public sealed class Versions : ICommand
{
    /// <inheritdoc/>
    public string Command => "versions";

    /// <inheritdoc/>
    public string[] Aliases => Array.Empty<string>();

    /// <inheritdoc/>
    public string Description => "Lists the current and previous versions of banana plugin.";

    /// <inheritdoc/>
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [NotNull] out string? response)
    {
        if (!Plugin.AssertEnabled(out response))
        {
            return false;
        }

        StringBuilder strBuilder = StringBuilderPool.Shared.Rent();

        strBuilder.AppendLine("\nBanana Plugin versions:\n");

        foreach (string version in Versioning.AllVersions)
        {
            strBuilder.AppendLine(version);
        }

        response = StringBuilderPool.Shared.ToStringReturn(strBuilder);
        return true;
    }
}
