namespace BananaPlugin.Extensions;

using BananaPlugin.API.Interfaces;
using CommandSystem;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Consists of command extensions used for different use cases throughout the assembly.
/// </summary>
public static class CommandExtensions
{
    /// <summary>
    /// Converts a string to a help provider format for usage in <see cref="IHelpProvider"/>.
    /// </summary>
    /// <param name="command">The command used for formatting.</param>
    /// <param name="helpProvider">The string to format.</param>
    /// <returns>A formatted string for usage with <see cref="IHelpProvider"/>.</returns>
    public static string HelpProviderFormat(this ICommand command, string helpProvider)
    {
        if (command is IRequiresRank requiresRank)
        {
            helpProvider = string.Concat("<b><u>Requires rank: ", requiresRank.RankRequirement.ToString(), "</u></b>\n", helpProvider);
        }

        return string.Concat("HELP PROVIDER:\n\n> ", command.Description, "\n\n", helpProvider, "\n");
    }

    /// <summary>
    /// Gets a response string representing an invalid subcommand input.
    /// </summary>
    /// <param name="handler">The command handler instance.</param>
    /// <param name="response">The outputted response.</param>
    /// <returns><see langword="false"/>.</returns>
    public static bool InvalidSubcommandFormat(this ICommandHandler handler, out string response)
    {
        IEnumerable<string> commands = handler.AllCommands.Select(SelectCommand);

        response = $"\nInvalid subcommand. Valid subcommands:\n- {string.Join("\n- ", commands)}";
        return false;
    }

    private static string SelectCommand(ICommand command) => command.Command;
}
