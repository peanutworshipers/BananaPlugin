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
using System;
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

    private Dictionary<PlayerRoleBase, EffectInfo[]>? EffectsToApply { get; set; }

    /// <inheritdoc/>
    protected override void Enable()
    {
        this.EffectsToApply = new();

        ExHandlers.Server.RoundStarted += this.OnRoundStarted;

        if (Round.IsStarted)
        {
            this.OnRoundStarted();
        }
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        this.EffectsToApply = null;

        ExHandlers.Server.RoundStarted -= this.OnRoundStarted;

        Timing.KillCoroutines(this.guardHandle);
    }

    /// <inheritdoc/>
    protected override CfgBetterEscape RetrieveLocalConfig(Config config) => config.BetterEscape;

    private static EffectInfo[] GetEffectInfos(ReferenceHub player)
    {
        EffectInfo[] effectInfos = new EffectInfo[EffectArrayLength];

        StatusEffectBase[] allEffects = player.playerEffectsController.AllEffects;

        for (int i = 0; i < allEffects.Length; i++)
        {
            effectInfos[i] = new EffectInfo()
            {
                Type = allEffects[i].GetType(),
                Intensity = allEffects[i].Intensity,
                TimeLeft = allEffects[i].TimeLeft,
            };
        }

        return effectInfos;
    }

    private void OnRoundStarted()
    {
        this.guardHandle.KillAssignNew(this.GuardCoroutine, Segment.Update);
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

    private void ApplyEffects(PlayerRoleBase role)
    {
        if (this.EffectsToApply is null)
        {
            return;
        }

        if (!role || role.ServerSpawnReason != RoleChangeReason.Escaped || role.Pooled)
        {
            this.EffectsToApply.Remove(role);
            return;
        }

        if (!this.EffectsToApply.TryGetValue(role, out EffectInfo[] infos))
        {
            return;
        }

        Dictionary<Type, StatusEffectBase> effects = role._lastOwner.playerEffectsController._effectsByType;

        for (int i = 0; i < infos.Length; i++)
        {
            EffectInfo info = infos[i];

            if (effects.TryGetValue(info.Type, out StatusEffectBase effect))
            {
                effect.ServerSetState(info.Intensity, info.TimeLeft, false);
            }
        }

        this.EffectsToApply.Remove(role);
    }

    /// <summary>
    /// A struct containing info for an effect.
    /// </summary>
    public struct EffectInfo
    {
        /// <summary>
        /// The type of the effect.
        /// </summary>
        public Type Type;

        /// <summary>
        /// The intensity of the effect.
        /// </summary>
        public byte Intensity;

        /// <summary>
        /// The time left for the effect.
        /// </summary>
        public float TimeLeft;
    }

    [HarmonyPatch(typeof(PlayerEffectsController), nameof(PlayerEffectsController.OnRoleChanged))]
    private static class PlayerEffContPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
#pragma warning disable SA1118 // Parameter should not span multiple lines
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Ret) + 1;

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                // PlayerEffContPatch.OnRoleChanged(targetHub, oldRole, newRole);
                new CodeInstruction(OpCodes.Ldarg_1)
                    .MoveLabelsFrom(newInstructions[index]),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Ldarg_3),
                new(OpCodes.Call, Method(typeof(PlayerEffContPatch), nameof(OnRoleChanged))),
            });

            return newInstructions.FinishTranspiler();
#pragma warning restore SA1118 // Parameter should not span multiple lines
        }

        private static void OnRoleChanged(ReferenceHub userHub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
        {
            if (!Instance || !Instance.Enabled)
            {
                return;
            }

            Instance.EffectsToApply!.Remove(prevRole);

            if (newRole.ServerSpawnReason != RoleChangeReason.Escaped)
            {
                return;
            }

            Instance.EffectsToApply[newRole] = GetEffectInfos(userHub);
            MECExtensions.RunAfterFrames(10, Segment.Update, Instance.ApplyEffects, newRole);
        }
    }
}
