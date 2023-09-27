namespace BananaPlugin.Commands.Main;

using BananaPlugin.API;
using BananaPlugin.API.Interfaces;
using BananaPlugin.API.Main;
using BananaPlugin.Extensions;
using CommandSystem;
using Exiled.Permissions.Extensions;
using NorthwoodLib.Pools;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

/// <summary>
/// The command responsible for enabling or disabling banana features.
/// </summary>
public sealed class EnableOrDisable : ICommand, IUsageProvider, IHelpProvider, IRequiresRank
{
    private readonly string toggledText;
    private readonly string toggleText;
    private readonly bool enabling;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnableOrDisable"/> class.
    /// </summary>
    /// <param name="enable">Specifies whether the Instance is an enable or disable command.</param>
    internal EnableOrDisable(bool enable)
    {
        this.toggledText = enable ? "enabled" : "disabled";

        this.toggleText = enable ? "enable" : "disable";

        this.Command = enable ? "enable" : "disable";

        this.Aliases = enable
            ? ["en"]
            : ["dis"];

        this.Description = enable
            ? "Enables the specified banana plugin feature."
            : "Disables the specified banana plugin feature.";

        this.enabling = enable;
    }

    /// <inheritdoc/>
    public string Command { get; }

    /// <inheritdoc/>
    public string[] Aliases { get; }

    /// <inheritdoc/>
    public string Description { get; }

    /// <inheritdoc/>
    public string[] Usage { get; } =
    [
        "Feature Prefix",
    ];

    /// <inheritdoc/>
    public BRank RankRequirement => BRank.SeniorModerator;

    /// <inheritdoc/>
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [NotNull] out string? response)
    {
        // Make sure plugin is enabled.
        if (!Plugin.AssertEnabled(out response))
        {
            return false;
        }

        // Must have senior mod permissions.
        if (!sender.CheckPermission(BRank.SeniorModerator, out response))
        {
            return false;
        }

        string[] args = arguments.ToArray();

        if (args.Length == 0)
        {
            response = "You must provide a feature prefix.";
            return false;
        }

        // make sure they are distinct values
        string[] prefixes = args[0].ToLower().Split(',').Distinct().ToArray();

        // rent a string builder
        StringBuilder strBuilder = StringBuilderPool.Shared.Rent();

        strBuilder.AppendLine();

        // Append successes and failures to response.
        for (int i = 0; i < prefixes.Length; i++)
        {
            string prefix = prefixes[i];

            if (!Plugin.Features.TryGetFeature(prefix, out BananaFeature? feature))
            {
                strBuilder.AppendLine($"<color=orange>Feature with prefix '{prefix}' could not be found.</color>");
                continue;
            }

            if (this.enabling == feature.Enabled)
            {
                strBuilder.AppendLine($"<color=orange>Feature with prefix '{prefix}' is already {this.toggledText}.</color>");
                continue;
            }

            try
            {
                feature.Enabled = this.enabling;
                strBuilder.AppendLine($"<color=white>Feature with prefix '{prefix}' was {this.toggledText}.</color>");
            }
            catch (Exception ex)
            {
                strBuilder.AppendLine($"\n<color=#ff0000>Feature with prefix '{prefix}' failed to {this.toggleText}:\n{ex}</color>\n");
            }
        }

        // return built string and its value
        response = StringBuilderPool.Shared.ToStringReturn(strBuilder);
        return true;
    }

    /// <inheritdoc/>
    public string GetHelp(ArraySegment<string> arguments)
    {
        string[] lines =
        [
            $"\nProvide a feature prefix to {this.toggleText} that feature.",
            "For a list of prefixes, use the command 'bananaplugin showfeatures'",
            string.Empty,
            $"You can include multiple prefixes separated by commas to {this.toggleText} multiple features.",
            "EX: feature1,feature2,feature3",
        ];

        return this.HelpProviderFormat(string.Join("\n", lines));
    }
}
