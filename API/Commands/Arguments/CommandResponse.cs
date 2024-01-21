// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         CommandResponse.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 6:13 PM
//    Created Date:     11/08/2023 6:13 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands.Arguments;

/// <summary>
/// Encapsulates the response for a command.
/// </summary>
public sealed class CommandResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandResponse"/> class.
    /// </summary>
    /// <param name="args">The argument collection result.</param>
    public CommandResponse(ArgumentResultCollection args)
    {
        this.Arguments = args;
    }

    /// <summary>
    /// Gets a <see cref="ArgumentResultCollection"/> with the provided command information.
    /// </summary>
    public ArgumentResultCollection Arguments { get; }

    /// <summary>
    /// Gets or sets the value of the resulting command.
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether or not the command was successful.
    /// </summary>
    public bool Success { get; set; } = false;
}
