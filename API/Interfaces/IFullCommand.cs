namespace BananaPlugin.API.Interfaces;

using CommandSystem;

/// <summary>
/// An interface that fully implements a command.
/// </summary>
public interface IFullCommand : ICommand, IUsageProvider, IHelpProvider
{
}
