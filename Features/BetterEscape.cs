namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.Extensions;
using BananaPlugin.Features.Configs;
using CustomPlayerEffects;
using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Disarming;
using MEC;
using Mirror;
using PlayerRoles;
using Respawning;
using System.Collections.Generic;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

/// <summary>
/// The main feature that improves escape functionality.
/// </summary>
public sealed class BetterEscape : BananaFeatureConfig<CfgBetterEscape>
{
    private CoroutineHandle guardHandle;

    static BetterEscape()
    {
        EffectArrayLength = NetworkManager.singleton.playerPrefab.GetComponentsInChildren<StatusEffectBase>().Length;
        BPLogger.Debug($"Effect array length: {EffectArrayLength}");
    }

    private BetterEscape()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the better escape instance.
    /// </summary>
    public static BetterEscape? Instance { get; private set; }

    /// <inheritdoc/>
    public override string Name => "Better Escape";

    /// <inheritdoc/>
    public override string Prefix => "besc";

    private static int EffectArrayLength { get; }

    /// <inheritdoc/>
    protected override void Enable()
    {
        ExHandlers.Server.RoundStarted += this.OnRoundStarted;
        PlayerRoleManager.OnRoleChanged += this.OnRoleChanged;

        if (Round.IsStarted)
        {
            this.OnRoundStarted();
        }
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ExHandlers.Server.RoundStarted -= this.OnRoundStarted;
        PlayerRoleManager.OnRoleChanged -= this.OnRoleChanged;

        Timing.KillCoroutines(this.guardHandle);
    }

    /// <inheritdoc/>
    protected override CfgBetterEscape RetrieveLocalConfig(Config config) => config.BetterEscape;

    private void OnRoundStarted()
    {
        this.guardHandle.KillAssignNew(this.GuardCoroutine, Segment.Update);
    }

    private void OnRoleChanged(ReferenceHub userHub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
    {
        if (newRole.ServerSpawnReason != RoleChangeReason.Escaped)
        {
            return;
        }

        MECExtensions.Run(this.ValidateEffectsSync, Segment.Update, userHub.playerEffectsController);
    }

    private IEnumerator<float> ValidateEffectsSync(PlayerEffectsController controller)
    {
        if (!controller || !controller.gameObject)
        {
            yield break;
        }

        for (int i = 0; i < controller.EffectsLength; i++)
        {
            controller._syncEffectsIntensity[i] = 0;
        }

        // Race conditions require this wait.
        yield return Timing.WaitForSeconds(0.1f);

        for (int i = 0; i < controller.EffectsLength; i++)
        {
            controller._syncEffectsIntensity[i] = controller.AllEffects[i].Intensity;
        }
    }

    private IEnumerator<float> GuardCoroutine()
    {
        BPLogger.IdentifyMethodAs("GuardEscape", "MainCoroutine");

        while (Round.IsStarted)
        {
            foreach (ReferenceHub hub in PlayerListUtils.VerifiedHubs)
            {
                if (hub.roleManager.CurrentRole.RoleTypeId != RoleTypeId.FacilityGuard)
                {
                    continue;
                }

                if ((hub.transform.position - Escape.WorldPos).sqrMagnitude > Escape.RadiusSqr)
                {
                    continue;
                }

                if (!hub.inventory.IsDisarmed())
                {
                    continue;
                }

                hub.roleManager.ServerSetRole(RoleTypeId.ChaosConscript, RoleChangeReason.Escaped);
                RespawnTokensManager.GrantTokens(SpawnableTeamType.ChaosInsurgency, this.LocalConfig.EscapeTokens);

                BPLogger.Debug($"Cuffed guard escaped. ({hub.nicknameSync.MyNick})");
            }

            yield return Timing.WaitForOneFrame;
        }
    }

    [HarmonyPatch(typeof(StatusEffectBase), nameof(StatusEffectBase.OnRoleChanged))]
    private static class StatusEffBasePatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            Label allowLabel = generator.DefineLabel();

            newInstructions[0].labels.Add(allowLabel);

#pragma warning disable SA1118 // Parameter should not span multiple lines
            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                // if (newRole._spawnReason == RoleChangeReason.Escaped)
                // {
                //     return;
                // }
                new(OpCodes.Ldarg_2),
                new(OpCodes.Ldfld, Field(typeof(PlayerRoleBase), nameof(PlayerRoleBase._spawnReason))),
                new(OpCodes.Ldc_I4, (int)RoleChangeReason.Escaped),
                new(OpCodes.Bne_Un_S, allowLabel),
                new(OpCodes.Ret),
            });
#pragma warning restore SA1118 // Parameter should not span multiple lines

            return newInstructions.FinishTranspiler();
        }
    }
}
