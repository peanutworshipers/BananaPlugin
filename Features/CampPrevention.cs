namespace BananaPlugin.Features;

using BananaPlugin.API.Attributes;
using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.Extensions;
using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using Hints;
using InventorySystem.Items.Armor;
using MapGeneration;
using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;

/// <summary>
/// The main feature responsible for preventing camping.
/// </summary>
[AllowedPorts(ServerPorts.TestServer)]
public sealed class CampPrevention : BananaFeature
{
    private CoroutineHandle nukeSiloHandle;
    private NukeSiloHelper? nukeSiloHelper;

    private CampPrevention()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the Camp Prevention instance.
    /// </summary>
    public static CampPrevention? Instance { get; private set; }

    /// <inheritdoc/>
    public override string Name => "Camp Prevention";

    /// <inheritdoc/>
    public override string Prefix => "camp";

    /// <summary>
    /// Checks if a player is silo poisoned.
    /// </summary>
    /// <param name="hub">The player to check.</param>
    /// <returns>A value indicating whether the player is silo poisoned.</returns>
    public static bool IsSiloPoisoned(ReferenceHub hub)
    {
        NukeSiloHelper? silo = Instance?.nukeSiloHelper;

        return silo is not null
            && silo.TimeSpent is not null
            && silo.TimeSpent.TryGetValue(hub.PlayerId, out float time)
            && time >= NukeSiloHelper.TimeTillDamage;
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        this.nukeSiloHelper = new();

        ExHandlers.Server.RoundStarted += this.RoundStarted;
        ExHandlers.Map.Generated += this.MapGenerated;
        ExHandlers.Player.Hurting += this.Hurting;

        if (SeedSynchronizer.MapGenerated)
        {
            this.MapGenerated();
        }

        if (Round.IsStarted)
        {
            this.RoundStarted();
        }
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        this.nukeSiloHelper!.Reset();
        this.nukeSiloHelper = null;

        ExHandlers.Server.RoundStarted -= this.RoundStarted;
        ExHandlers.Map.Generated -= this.MapGenerated;
        ExHandlers.Player.Hurting -= this.Hurting;

        Timing.KillCoroutines(this.nukeSiloHandle);
    }

    private void RoundStarted()
    {
        this.nukeSiloHandle.KillAssignNew(this.nukeSiloHelper!.Coroutine, Segment.Update);
    }

    private void MapGenerated()
    {
        this.nukeSiloHelper!.Reset();
        this.nukeSiloHelper!.Init();
    }

    private void Hurting(HurtingEventArgs ev)
    {
        if (ev.DamageHandler.Type != ExEnums.DamageType.Poison
            || ev.Player.IsGodModeEnabled
            || ev.DamageHandler.Base is NukeSiloPoisoned)
        {
            return;
        }

        if (IsSiloPoisoned(ev.Player.ReferenceHub))
        {
            ev.IsAllowed = false;

            ev.Player.Hurt(new NukeSiloPoisoned(ev.Amount));
        }
    }

    /// <summary>
    /// Patch responsible for making the nuke silo radiation do extra damage to SCPs.
    /// </summary>
    [HarmonyPatch(typeof(Poisoned), nameof(Poisoned.OnTick))]
    private static class PoisonedEffectPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
#pragma warning disable SA1118 // Parameter should not span multiple lines
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            Label originalLabel = generator.DefineLabel();
            LocalBuilder hub = generator.DeclareLocal(typeof(ReferenceHub));

            int index = newInstructions.FindIndex(x => x.LoadsField(Field(typeof(Poisoned), nameof(Poisoned.damagePerTick)))) + 1;

            newInstructions[index].labels.Add(originalLabel);

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                // if (CampPrevention.Instance is null || !CampPrevention.Instance.Enabled)
                // {
                //     goto originalLabel;
                // }
                new(OpCodes.Call, PropertyGetter(typeof(CampPrevention), nameof(Instance))),
                new(OpCodes.Brfalse_S, originalLabel),
                new(OpCodes.Call, PropertyGetter(typeof(CampPrevention), nameof(Instance))),
                new(OpCodes.Call, PropertyGetter(typeof(BananaFeature), nameof(Enabled))),
                new(OpCodes.Brfalse_S, originalLabel),

                // if (!PlayerRolesUtils.IsSCP(this.Hub, includeZombies: false))
                // {
                //     goto originalLabel;
                // }
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, PropertyGetter(typeof(StatusEffectBase), nameof(StatusEffectBase.Hub))),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Call, Method(typeof(PlayerRolesUtils), nameof(PlayerRolesUtils.IsSCP))),
                new(OpCodes.Brfalse_S, originalLabel),

                // if (!CampPrevention.IsSiloPoisoned(this.Hub))
                // {
                //     goto originalLabel;
                // }
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, PropertyGetter(typeof(StatusEffectBase), nameof(StatusEffectBase.Hub))),
                new(OpCodes.Call, Method(typeof(CampPrevention), nameof(IsSiloPoisoned))),
                new(OpCodes.Brfalse_S, originalLabel),

                // [onstack] damagePerTick *= 6f;
                new(OpCodes.Ldc_R4, 6f),
                new(OpCodes.Mul),
            });

            return newInstructions.FinishTranspiler();
#pragma warning restore SA1118 // Parameter should not span multiple lines
        }
    }

    /// <summary>
    /// Damage handler that bypasses hume and AHP.
    /// </summary>
    private class NukeSiloPoisoned : CustomReasonDamageHandler
    {
        public NukeSiloPoisoned(float damage)
            : base("Died to nuke silo radiation.", damage, "TERMINATED BY WARHEAD RADIATION")
        {
        }

        public override HandlerOutput ApplyDamage(ReferenceHub ply)
        {
            if (this.Damage <= 0f)
            {
                return HandlerOutput.Nothing;
            }

            HealthStat module = ply.playerStats.GetModule<HealthStat>();

            module.CurValue -= this.Damage;

            return module.CurValue > 0f
                ? HandlerOutput.Damaged
                : HandlerOutput.Death;
        }
    }

    /// <summary>
    /// Helper class to manage the time players have been in the silo and their damage.
    /// </summary>
    private sealed class NukeSiloHelper
    {
        public const float TimeTillDamage = 120f;
        public const float TimeTillHint = 90f;

        private readonly List<FpcStandardRoleBase> validPlayers = new(50);
        private readonly Dictionary<int, float> timeSpent = new();

        private RoomIdentifier? nukeSilo;

        public RoomIdentifier? NukeSilo => this.nukeSilo;

        public Dictionary<int, float>? TimeSpent => this.timeSpent;

        internal void Init()
        {
            RoomIdUtils.TryFindRoom(RoomName.HczWarhead, FacilityZone.HeavyContainment, RoomShape.Straight, out this.nukeSilo);

            PlayerRoleManager.OnRoleChanged += this.OnRoleChanged;
        }

        internal void Reset()
        {
            this.nukeSilo = null;
            this.timeSpent.Clear();
            this.validPlayers.Clear();

            PlayerRoleManager.OnRoleChanged -= this.OnRoleChanged;
        }

        internal IEnumerator<float> Coroutine()
        {
            while (Round.IsStarted)
            {
                for (int i = 0; i < this.validPlayers.Count; i++)
                {
                    if (!this.ValidEntry(ref i, out FpcStandardRoleBase role))
                    {
                        continue;
                    }

                    bool gainingTime = this.InSilo(role);

                    // 2.5 times as long to regain time
                    float timeToAdd = gainingTime ? Timing.DeltaTime : (-Timing.DeltaTime * 0.4f);

                    if (gainingTime
                        && role._lastOwner.inventory.TryGetBodyArmor(out BodyArmor armor)
                        && armor.ItemTypeId == ItemType.ArmorHeavy)
                    {
                        timeToAdd *= 1f / 3f;
                    }

                    float curTime = this.AddTime(role._lastOwner, timeToAdd);
                    this.HandlePlayerTime(role._lastOwner, curTime, gainingTime);
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        private float GetCurTime(ReferenceHub hub)
        {
            if (!hub)
            {
                return 0f;
            }

            if (!this.timeSpent.TryGetValue(hub.PlayerId, out float time))
            {
                time = this.timeSpent[hub.PlayerId] = 0f;
            }

            return time;
        }

        private float AddTime(ReferenceHub hub, float time)
        {
            return !hub
                ? 0f
                : (this.timeSpent![hub.PlayerId] = Mathf.Clamp(time + this.GetCurTime(hub), 0f, TimeTillDamage + 2f));
        }

        private void OnRoleChanged(ReferenceHub hub, PlayerRoleBase oldRole, PlayerRoleBase newRole)
        {
            if (hub.characterClassManager.InstanceMode != ClientInstanceMode.ReadyClient)
            {
                return;
            }

            this.timeSpent[hub.PlayerId] = 0f;

            if (newRole is FpcStandardRoleBase fpc)
            {
                this.validPlayers.Add(fpc);
            }

            if (oldRole is FpcStandardRoleBase oldFpc)
            {
                this.validPlayers.Remove(oldFpc);
            }
        }

        private bool ValidEntry(ref int i, out FpcStandardRoleBase role)
        {
            role = this.validPlayers[i];

            if (role.Pooled)
            {
                this.validPlayers.RemoveAt(i--);
                return false;
            }

            return true;
        }

        private bool InSilo(FpcStandardRoleBase role)
        {
            Vector3 pos = role.FpcModule.Position;

            return pos.y >= -750f
                && pos.y <= -700
                && this.nukeSilo!.OccupiedCoords.Contains(RoomIdUtils.PositionToCoords(pos));
        }

        private void HandlePlayerTime(ReferenceHub hub, float curTime, bool gainingTime)
        {
            bool showHint = Time.frameCount % 30 == 0;

            if (curTime >= TimeTillDamage)
            {
                Poisoned effect = hub.playerEffectsController.GetEffect<Poisoned>();

                if (effect.Intensity == 0 || (effect.TimeLeft < 1f && effect.Duration != 0f))
                {
                    effect.ServerSetState(1, 1f, true);
                }

                if (showHint)
                {
                    const string message = "You feel a surging sickness flow through you...";

                    hub.hints.Show(new TextHint(message, new HintParameter[1] { new StringHintParameter(message) }));
                }

                return;
            }
            else if (showHint && gainingTime && curTime >= TimeTillHint)
            {
                const string message = "You are starting to feel weird...";

                hub.hints.Show(new TextHint(message, new HintParameter[1] { new StringHintParameter(message) }));
            }
        }
    }
}
