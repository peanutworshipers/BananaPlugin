namespace BananaPlugin.API.Utils;

using Exiled.Permissions.Extensions;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

/// <summary>
/// A class that handles the plugin developers.
/// </summary>
internal static class DeveloperUtils
{
    /// <summary>
    /// Contains all registered developers.
    /// </summary>
    public static readonly IReadOnlyList<DeveloperInfo> Developers = new List<DeveloperInfo>()
    {
        new ("Zereth", "76561198288227848@steam"),
        new ("Zereth", "300607485738221569@discord"),

        new ("[LFG] Red Force", "76561198151373620@steam"),
        new ("[LFG] Red Force", "274987479386292224@discord"),

        // new ("Alex", "76561199489585086@steam"),
        // new ("Alex", "1125158821598339125@discord"),

        // new ("Skillz", "76561198397906373@steam"),
        // new ("Skillz", "330362707029131265@discord"),
    };

    /// <summary>
    /// Determines if the userid belongs to a developer.
    /// </summary>
    /// <param name="userId">The userid to check.</param>
    /// <returns>A value indicating whether the userid belongs to a developer.</returns>
    public static bool IsDeveloper(string userId)
    {
        for (int i = 0; i < Developers.Count; i++)
        {
            if (userId == Developers[i])
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if the player is a developer.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>A value indicating whether the player is a developer.</returns>
    public static bool IsDeveloper(this ExPlayer player)
    {
        // ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        return player is not null && player.GameObject && IsDeveloper(player.UserId);
    }

    /// <summary>
    /// Determines if the player is a developer.
    /// </summary>
    /// <param name="player">The player to check.</param>
    /// <returns>A value indicating whether the player is a developer.</returns>
    public static bool IsDeveloper(this ReferenceHub player)
    {
        return player && player.gameObject && IsDeveloper(player.characterClassManager.UserId);
    }

    /// <summary>
    /// A struct containing developer info.
    /// </summary>
    public struct DeveloperInfo
    {
        /// <summary>
        /// The name of the developer.
        /// </summary>
        public string Name;

        /// <summary>
        /// The userid of the developer.
        /// </summary>
        public string UserId;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeveloperInfo"/> struct.
        /// </summary>
        /// <param name="name">The name of the developer.</param>
        /// <param name="userid">The userid of the developer.</param>
        public DeveloperInfo(string name, string userid)
        {
            this.Name = name;
            this.UserId = userid;
        }

        public static implicit operator string(DeveloperInfo info) => info.UserId;
    }

    // ReSharper disable  UnusedType.Local
    [HarmonyPatch(typeof(Permissions), nameof(Permissions.CheckPermission), typeof(ExPlayer), typeof(string))]
    private static class ExiledPermissionPatch
    {
        // ReSharper disable UnusedMember.Local
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            Label notDeveloperLabel = generator.DefineLabel();

            int index = newInstructions.FindNthInstruction(2, x => x.opcode == OpCodes.Ldc_I4_0);

            if (index == -1)
            {
                throw new System.Exception("Unable to locate instruction index.");
            }

            index += 2;

            List<Label> extracted = newInstructions[index].ExtractLabels();

            newInstructions[index].labels.Add(notDeveloperLabel);

#pragma warning disable SA1118 // Parameter should not span multiple lines
            newInstructions.InsertRange(index, new[]
            {
                // if (DeveloperUtils.IsDeveloper(player))
                // {
                //     return true;
                // }
                new CodeInstruction(OpCodes.Ldarg_0).WithLabels(extracted),
                new(OpCodes.Call, Method(typeof(DeveloperUtils), nameof(IsDeveloper), new[] { typeof(ExPlayer) })),
                new(OpCodes.Brfalse_S, notDeveloperLabel),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Ret),
            });
#pragma warning restore SA1118 // Parameter should not span multiple lines

            return newInstructions.FinishTranspiler();
        }
    }
}
