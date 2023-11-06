namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.API.Utils.CustomWriters;
using BananaPlugin.Extensions;
using CustomPlayerEffects;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using InventorySystem;
using InventorySystem.Items;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.Spectating;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;

#pragma warning disable SA1118 // Parameter should not span multiple lines

/// <summary>
/// The main feature for handling the replacement of disconnecting players.
/// </summary>
[Obsolete]
public sealed class DisconnectReplace : PluginFeature
{
    private DisconnectReplace()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the Disconnect Replace instance.
    /// </summary>
    public static DisconnectReplace? Instance { get; private set; }

    /// <inheritdoc/>
    public override string Name => "Disconnect Replace";

    /// <inheritdoc/>
    public override string Prefix => "dcr";

    /// <inheritdoc/>
    protected override void Enable()
    {
        ExHandlers.Player.Left += this.Left;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ExHandlers.Player.Left -= this.Left;
    }

    private static bool FilterHubs(ReferenceHub toSelect)
    {
        return toSelect.roleManager.CurrentRole is SpectatorRole;
    }

    private static float GetDeathTime(ReferenceHub hub)
    {
        return hub.roleManager.CurrentRole is SpectatorRole spec
            ? spec.ActiveTime
            : 0f;
    }

    private void Left(LeftEventArgs ev)
    {
        try
        {
            // Add proper role checking here.
            // Ex: tutorials should not be replaced.
            //
            // However, custom roles should specify
            // if they can be replaced.

#warning CustomRoles look here.

            if (ev.Player.Role.Base is not IFpcRole oldRole)
            {
                ev.Player.ReferenceHub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown));
                return;
            }

            if (ev.Player.ReferenceHub.roleManager.CurrentRole.RoleTypeId == RoleTypeId.Tutorial)
            {
                ev.Player.ReferenceHub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown));
                return;
            }

            ReferenceHub left = ev.Player.ReferenceHub;
            ReferenceHub? available =
                PlayerListUtils.VerifiedHubs
                .OrderByDescending(GetDeathTime)
                .FirstOrDefault(FilterHubs);

            // Nobody available to replace
            // the disconnected player.
            if (available == null)
            {
                BPLogger.Debug("Available player: (null)");
                left.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown));
                return;
            }

            // Apply replacement here.

            BPLogger.Debug($"Available player: {available.nicknameSync._firstNickname}");

            available.roleManager.ServerSetRole(left.roleManager.CurrentRole.RoleTypeId, RoleChangeReason.None, RoleSpawnFlags.None);
            IFpcRole newRole = (IFpcRole)available.roleManager.CurrentRole;
            newRole.FpcModule.ServerOverridePosition(left.transform.position, Vector3.zero);

            // Set rotation late.
            FpcMouseLook oldMouse = oldRole.FpcModule.MouseLook;
            MECExtensions.RunAfterFrames(5, MEC.Segment.Update, available.SetHubRotation, oldMouse._prevSyncH, oldMouse._prevSyncV);

            //left.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown));
        }
        catch
        {
            ev.Player.ReferenceHub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Unknown));
            throw;
        }
    }

    private readonly unsafe struct DisconnectInfo
    {
        public readonly ushort Horizontal;

        public readonly ushort Vertical;

        public readonly Vector3 Position;

        public readonly RoleTypeId RoleTypeId;

        public readonly StatBase[] PlayerStats;

        public readonly byte[] EffectIntensity;

        public readonly float[] EffectTime;

        public readonly InventoryInfo InventoryInfo;

#warning customroles look here. add custom role field here for checking.

        public DisconnectInfo(ReferenceHub hub)
        {
            int i;

            if (hub == null)
            {
                throw new NullReferenceException("Hub cannot be null.");
            }

            if (hub.roleManager.CurrentRole is not FpcStandardRoleBase fpcRole)
            {
                throw new InvalidCastException("Role must be an instance of PlayerRoles.FirstPersonControl.FpcStandardRoleBase.");
            }

            if (fpcRole.FpcModule?.MouseLook is not FpcMouseLook mouseLook)
            {
                throw new NullReferenceException("FpcModule cannot have a null MouseLook instance.");
            }

            this.Horizontal = mouseLook._prevSyncH;
            this.Vertical = mouseLook._prevSyncV;
            this.Position = hub.transform.position;
            this.RoleTypeId = fpcRole.RoleTypeId;
            this.PlayerStats = hub.playerStats._statModules;

            PlayerEffectsController effects = hub.playerEffectsController;
            int effectsLength = effects.EffectsLength;

            this.EffectIntensity = new byte[effectsLength];
            this.EffectTime = new float[effectsLength];

            for (i = 0; i < effectsLength; i++)
            {
                StatusEffectBase effect = effects.AllEffects[i];

                this.EffectIntensity[i] = effect._intensity;
                this.EffectTime[i] = effect._timeLeft;
            }

            this.InventoryInfo = hub.inventory.UserInventory;
        }

        public void ApplyInfo(ReferenceHub hub)
        {
            if (hub == null)
            {
                throw new NullReferenceException("Hub cannot be null.");
            }

            hub.roleManager.ServerSetRole(this.RoleTypeId, RoleChangeReason.None, RoleSpawnFlags.None);

            if (hub.roleManager.CurrentRole is not FpcStandardRoleBase fpcRole)
            {
                throw new InvalidCastException("Role must be an instance of PlayerRoles.FirstPersonControl.FpcStandardRoleBase.");
            }

            fpcRole.FpcModule.ServerOverridePosition(this.Position, Vector3.zero);
            MECExtensions.RunAfterFrames(2, MEC.Segment.Update, hub.SetHubRotation, this.Horizontal, this.Vertical);
        }

        public void ApplyStatBase(ReferenceHub hub, StatBase stat)
        {
            // typeof(HealthStat),
            // typeof(AhpStat),
            // typeof(StaminaStat),
            // typeof(AdminFlagsStat),
            // typeof(HumeShieldStat),
            // typeof(Scp106Vigor)
            switch (stat)
            {
                case HealthStat curStat when hub.playerStats.TryGetModule(out HealthStat newStat):
                    return;

                case AhpStat curStat when hub.playerStats.TryGetModule(out AhpStat newStat):
                    return;

                case StaminaStat curStat when hub.playerStats.TryGetModule(out AhpStat newStat):
                    return;

                case AdminFlagsStat curStat when hub.playerStats.TryGetModule(out AhpStat newStat):
                    return;

                case HumeShieldStat curStat when hub.playerStats.TryGetModule(out AhpStat newStat):
                    return;

                case Scp106Vigor curStat when hub.playerStats.TryGetModule(out AhpStat newStat):
                    return;

                default:
                    throw new Exception($"Could not apply StatBase instance for: {stat.GetType().FullName}");
            }
        }
    }

    [HarmonyPatch(typeof(CustomNetworkManager), nameof(CustomNetworkManager.OnServerDisconnect))]
    private static class RemoveDisonnectDropPatch
    {
        // This patch skips default disconnectDrop if the feature is enabled.
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            Label dontSkipLabel = generator.DefineLabel();

            FieldInfo disconnectDropField = Field(typeof(CustomNetworkManager), nameof(CustomNetworkManager._disconnectDrop));

            int index = newInstructions.FindIndex(x => x.LoadsField(disconnectDropField));

            newInstructions[0].labels.Add(dontSkipLabel);

            Label skipDisconnectDropLabel = (Label)newInstructions[index + 1].operand;

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                // if (DisconnectReplace.Instance?.Enabled ?? false)
                //     goto skipDisconnectDropLabel;
                new(OpCodes.Call, PropertyGetter(typeof(DisconnectReplace), nameof(Instance))),
                new(OpCodes.Brfalse_S, dontSkipLabel),
                new(OpCodes.Call, PropertyGetter(typeof(DisconnectReplace), nameof(Instance))),
                new(OpCodes.Call, PropertyGetter(typeof(PluginFeature), nameof(Enabled))),
                new(OpCodes.Brtrue_S, skipDisconnectDropLabel),
            });

            return newInstructions.FinishTranspiler();
        }
    }
}
