namespace BananaPlugin.Commands.Main;

using BananaPlugin.API.Main;
using BananaPlugin.Extensions;
using CommandSystem;
using NorthwoodLib.Pools;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

/// <summary>
/// The command responsible for listing features, their prefix, and their enabled status to users.
/// </summary>
public sealed class ShowFeatures : ICommand, IUsageProvider, IHelpProvider
{
    /// <inheritdoc/>
    public string Command => "showfeatures";

    /// <inheritdoc/>
    public string[] Aliases => new string[]
    {
        "sf",
        "showf",
        "features",
    };

    /// <inheritdoc/>
    public string Description => "Shows the list of features, their prefix, and their enabled status.";

    /// <inheritdoc/>
    public string[] Usage => Array.Empty<string>();

    /// <inheritdoc/>
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [NotNull] out string? response)
    {
        const string ENABLED_COLOR = "<color=#00FF00>";
        const string DISABLED_COLOR = "<color=#FF0000>";

        // Make sure plugin is enabled.
        if (!Plugin.AssertEnabled(out response))
        {
            return false;
        }

        StringBuilder strBuilder = StringBuilderPool.Shared.Rent();

        strBuilder.AppendLine("\n<size=30><b><u>Features list:</u></b></size>\n");

        PluginFeature[] arr = Plugin.Features.OrderByDescending(x => x.Enabled).ToArray();

        if (arr.Length == 0)
        {
            response = "No features?";
            return false;
        }

        int index = -1;

        // [0] disabled <-- 0
        // [1] disabled

        // [0] enabled
        // [1] dis      <-- 1

        // <-- -1
        // [0] enabled
        // [1] enabled
        for (int i = 0; i < arr.Length; i++)
        {
            if (!arr[i].Enabled)
            {
                index = i;
                break;
            }
        }

        if (index == -1)
        {
            strBuilder.Append("Enabled:\n" + ENABLED_COLOR);

            for (int i = 0; i < arr.Length; i++)
            {
                strBuilder.AppendLine(this.GetFeatureString(arr[i]));
            }

            strBuilder.Append("</color>");

            response = StringBuilderPool.Shared.ToStringReturn(strBuilder);
            return true;
        }

        if (index == 0)
        {
            strBuilder.Append("Disabled:\n" + DISABLED_COLOR);

            for (int i = 0; i < arr.Length; i++)
            {
                strBuilder.AppendLine(this.GetFeatureString(arr[i]));
            }

            strBuilder.Append("</color>");

            response = StringBuilderPool.Shared.ToStringReturn(strBuilder);
            return true;
        }

        for (int i = 0; i < arr.Length; i++)
        {
            if (i == 0)
            {
                strBuilder.Append("Enabled:\n" + ENABLED_COLOR);
            }
            else if (index == i)
            {
                strBuilder.AppendLine("</color>");
                strBuilder.Append("Disabled:\n" + DISABLED_COLOR);
            }

            strBuilder.AppendLine(this.GetFeatureString(arr[i]));
        }

        strBuilder.Append("</color>");

        response = StringBuilderPool.Shared.ToStringReturn(strBuilder);
        return true;
    }

    /// <inheritdoc/>
    public string GetHelp(ArraySegment<string> arguments)
    {
        return this.HelpProviderFormat("Run the command to list all features, their prefix, and their enabled status.");
    }

    private string GetFeatureString(PluginFeature feature)
    {
        return $"{feature.Name} [{feature.Prefix}]";
    }
}
