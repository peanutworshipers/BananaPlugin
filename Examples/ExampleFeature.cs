// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         Feature.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 4:02 PM
//    Created Date:     11/08/2023 4:02 PM
// -----------------------------------------

namespace BananaPlugin.Examples;

using API.Commands;
using API.Commands.Arguments;
using API.Main;
using Exiled.API.Features;

/// <inheritdoc />
internal sealed class ExampleFeature : BananaFeature
{
    /// <inheritdoc/>
    public override string Name => "Example Command";

    /// <inheritdoc/>
    public override string Prefix => "ExCmd";

    /// <inheritdoc/>
    protected override void Enable()
    {
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
    }

    // ReSharper disable UnusedMember.Local
    [BananaCommand("Nickname2", "Gives a player a nickname.")]
    [CommandArgument<string>("Player", "The player to apply the new nickname to.", usagePath: 1)]
    [CommandArgument<string>("Nickname", "The new nickname.", usagePath: 1)]
    [CommandArgument<string>("Nickname", "The new nickname.", usagePath: 2)]
    private void BananaCommand(ArgumentResultCollection result)
    {
        string nickname = result["Nickname"].Result<string>();
        Player ply = result.GetCount() > 1 ? result["Player"].Result<ExPlayer>() : Player.Get(result.Sender);
        ply.DisplayNickname = nickname;
    }
}