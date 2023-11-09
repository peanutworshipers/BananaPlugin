// Copyright (c) Redforce04. All rights reserved.
// </copyright>
// -----------------------------------------
//    Solution:         BananaPlugin
//    Project:          BananaPlugin
//    FileName:         UsageProviderCommandImplementation.cs
//    Author:           Redforce04#4091
//    Revision Date:    11/08/2023 7:33 PM
//    Created Date:     11/08/2023 7:33 PM
// -----------------------------------------

namespace BananaPlugin.API.Commands.Arguments;

using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Main;

#pragma warning disable SA1648
/// <inheritdoc cref="CommandImplementation" />
public class UsageProviderCommandImplementation : CommandImplementation, IUsageProvider
{

    /// <inheritdoc />
    public UsageProviderCommandImplementation(BananaCommandAttribute cmd, List<BananaPermission>? requiredPermissions = null) : base(cmd, requiredPermissions)
    {
        List<string> args = new List<string>();
        foreach(List<CommandArgument> path in cmd.CommandArguments.Values)
        {
            if (path.Count < 1)
            {
                continue;
            }

            string concat = path[0];
            for (int i = 1; i < list.Count; i++)
            {
                concat += $" / {list[i]}";
            }

            values.Add(concat);
            
            args.Add();
        }

        List<string> values = new();
        foreach(List<string> list in args)
        {
            
        }
    }

    public string[] Usage { get; }
}