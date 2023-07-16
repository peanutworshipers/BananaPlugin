namespace BananaPlugin.Features;

using BananaPlugin.API;
using BananaPlugin.API.Interfaces;
using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.Extensions;
using BananaPlugin.Features.Configs;
using CommandSystem;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

/// <summary>
/// The main feature responsible for allowing pink candy to be obtained from SCP-330.
/// </summary>
public sealed class PinkCandyBowl : BananaFeatureConfig<CfgPinkCandyBowl>
{
    private PinkCandyBowl()
    {
        Instance = this;

        this.Commands = new ICommand[]
        {
            new SetPinkCandyWeight(this),
            new GetPinkCandyWeight(this),
            new GetWeightFromChance(),
        };
    }

    /// <summary>
    /// Gets the PinkCandyBowl instance.
    /// </summary>
    public static PinkCandyBowl? Instance { get; private set; }

    /// <inheritdoc/>
    public override string Name => "Pink Candy Bowl";

    /// <inheritdoc/>
    public override string Prefix => "pinkc";

    /// <inheritdoc/>
    public override ICommand[] Commands { get; }

    /// <summary>
    /// Gets the current pink candy weight.
    /// </summary>
    /// <returns>The weight value retrieved from the check.</returns>
    public static float GetCandyWeight()
    {
        return (Instance && Instance.Enabled) ? Instance.LocalConfig.PinkCandyWeight : 0f;
    }

    /// <summary>
    /// Converts a specified weight value to the percent chance of obtaining a pink candy.
    /// </summary>
    /// <param name="weight">The weight value to convert.</param>
    /// <returns>The weight represented in percent form. Returns <see cref="float.NaN"/> if the weight is less than zero.</returns>
    public static float ConvertWeightToPercent(float weight)
    {
        if (weight < 0f)
        {
            return float.NaN;
        }

        float totalWeights = 0f;

        for (int i = 0; i < Scp330Candies.AllCandies.Length; i++)
        {
            ICandy candy = Scp330Candies.AllCandies[i];

            if (candy.Kind == CandyKindID.Pink)
            {
                continue;
            }

            totalWeights += candy.SpawnChanceWeight;
        }

        return weight / (totalWeights + weight);
    }

    /// <summary>
    /// Converts a specified percent chance to the weight value for obtaining a pink candy.
    /// </summary>
    /// <param name="percent">The percent chance to convert.</param>
    /// <returns>The percent chance represented in weight form. Returns <see cref="float.NaN"/> if the percent exceeds the valid range (0-1).</returns>
    public static float ConvertPercentToWeight(float percent)
    {
        if (percent >= 1f || percent < 0f)
        {
            return float.NaN;
        }

        float totalWeights = 0f;

        for (int i = 0; i < Scp330Candies.AllCandies.Length; i++)
        {
            ICandy candy = Scp330Candies.AllCandies[i];

            if (candy.Kind == CandyKindID.Pink)
            {
                continue;
            }

            totalWeights += candy.SpawnChanceWeight;
        }

        return -(totalWeights * percent / (percent - 1f));
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
    }

    /// <inheritdoc/>
    protected override CfgPinkCandyBowl RetrieveLocalConfig(Config config) => config.PinkCandyBowl;

    [HarmonyPatch]
    private static class Patches
    {
        [HarmonyPatch(typeof(CandyPink), nameof(CandyPink.SpawnChanceWeight), MethodType.Getter)]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            LocalBuilder weight = generator.DeclareLocal(typeof(float).MakeByRefType());

            newInstructions.Clear();

#pragma warning disable SA1118 // Parameter should not span multiple lines
            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                // return PinkCandyBowl.GetCandyWeight();
                new(OpCodes.Call, Method(typeof(PinkCandyBowl), nameof(GetCandyWeight))),
                new(OpCodes.Ret),
            });
#pragma warning restore SA1118 // Parameter should not span multiple lines

            return newInstructions.FinishTranspiler();
        }
    }

    private sealed class SetPinkCandyWeight : IFeatureSubcommand<PinkCandyBowl>, IRequiresRank
    {
        public SetPinkCandyWeight(PinkCandyBowl parent)
        {
            this.Parent = parent;
        }

        public PinkCandyBowl Parent { get; }

        public string Command => "setweight";

        public string[] Aliases { get; } = new string[]
        {
            "sw",
            "sweight",
        };

        public string Description => "Sets the weight for pink candy.";

        public string[] Usage { get; } = new string[]
        {
            "weight",
        };

        public BRank RankRequirement => BRank.JuniorAdministrator;

        public string GetHelp(ArraySegment<string> arguments)
        {
            return this.HelpProviderFormat("Enter the weight value you want to set pink candy to.");
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, [NotNullWhen(true)] out string? response)
        {
            if (!sender.CheckPermission(this.RankRequirement, out response))
            {
                return false;
            }

            if (!this.Parent.Enabled)
            {
                response = "This feature is currently disabled.";
                return false;
            }

            if (arguments.Count == 0)
            {
                response = "You must provide one argument.";
                return false;
            }

            string input = arguments.At(0);

            float result;
            float percent;

            if (input.ToLower() == "default")
            {
                result = CfgPinkCandyBowl.DefaultPinkCandyWeight;
                percent = ConvertWeightToPercent(CfgPinkCandyBowl.DefaultPinkCandyWeight);
            }
            else if (!float.TryParse(input, out result) || (percent = ConvertWeightToPercent(result)) == float.NaN)
            {
                response = "Input is invalid. Must be a number greater than or equal to zero.";
                return false;
            }

            this.Parent.LocalConfig.PinkCandyWeight = result;

            response = $"Pink candy weight set to {result}. ({percent * 100f}% chance)";
            return true;
        }
    }

    private sealed class GetPinkCandyWeight : IFeatureSubcommand<PinkCandyBowl>
    {
        public GetPinkCandyWeight(PinkCandyBowl parent)
        {
            this.Parent = parent;
        }

        public PinkCandyBowl Parent { get; }

        public string Command => "getweight";

        public string[] Aliases { get; } = new string[]
        {
            "gw",
            "gweight",
        };

        public string Description => "Gets the weight for pink candy.";

        public string[] Usage => Array.Empty<string>();

        public string GetHelp(ArraySegment<string> arguments)
        {
            return this.HelpProviderFormat("Execute the command to get the current pink candy weight.");
        }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            float weight = GetCandyWeight();

            response = $"The current pink candy weight is {weight}. ({ConvertWeightToPercent(weight) * 100f}% chance)";
            return true;
        }
    }

    private sealed class GetWeightFromChance : ICommand
    {
        public string Command => "convertpercent";

        public string[] Aliases { get; } = new string[]
        {
            "convertp",
            "convp",
            "cp",
        };

        public string Description => "Converts a percent value to a weight value.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count == 0)
            {
                response = "You must provide one argument.";
                return false;
            }

            string input = arguments.At(0);

            float weight;

            if (!float.TryParse(input, out float result) || (weight = ConvertPercentToWeight(result)) == float.NaN)
            {
                response = "Input is invalid. Must be a number between zero and one.";
                return false;
            }

            response = $"The weight value for the provided percent is {weight}.";
            return true;
        }
    }
}
