namespace BananaPlugin.Features;

using AFK;
using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.Extensions;
using Exiled.Events.EventArgs.Interfaces;
using HarmonyLib;
using MEC;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using RelativePositioning;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;

/// <summary>
/// The main feature responsible for kicking afk players.
/// </summary>
public sealed class AfkDetector : BananaFeature
{
    /// <summary>
    /// The maximum time a player can be AFK before they are kicked after spawning.
    /// </summary>
    public const float AfkSpawnKickTime = 90f;

    /// <summary>
    /// The maximum time a player can be AFK before they are kicked while actively playing.
    /// </summary>
    public const float AfkActiveKickTime = 180f;

    private static readonly Vector3 TutorialTowerPos = new Vector3(40f, 1014.2f, -32f);
    private static readonly float TutorialTowerSqrDist = 64f;
    private CoroutineHandle mainHandle;

    private AfkDetector()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the AfkDetector instance.
    /// </summary>
    public static AfkDetector? Instance { get; private set; }

    /// <inheritdoc/>
    public override string Name => "AFK Detector";

    /// <inheritdoc/>
    public override string Prefix => "afk";

    private Dictionary<ReferenceHub, AfkInfo> HashedAfkInformation { get; } = new(100);

    private List<AfkInfo> AfkInformation { get; } = new(100);

    /// <inheritdoc/>
    protected override void Enable()
    {
        ExHandlers.Player.ChangingItem += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.DroppingItem += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.DroppingAmmo += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.DroppingNothing += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.VoiceChatting += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.TogglingNoClip += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.UsingItem += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.Shooting += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.DryfiringWeapon += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.AimingDownSight += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.ChangingRadioPreset += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.ChangingMoveState += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.Interacted += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.TogglingFlashlight += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.TogglingWeaponFlashlight += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.ReloadingWeapon += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.UnloadingWeapon += this.NotAfkPlayerEventHandler;
        ExHandlers.Player.ThrowingRequest += this.NotAfkPlayerEventHandler;

        PlayerRoleManager.OnRoleChanged += this.ChangingRole;
        ExHandlers.Server.WaitingForPlayers += this.WaitingForPlayers;

        MECExtensions.RunAfterFrames(1, Segment.EndOfFrame, AFKManager.ConfigReloaded);
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ExHandlers.Player.ChangingItem -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.DroppingItem -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.DroppingAmmo -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.DroppingNothing -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.VoiceChatting -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.TogglingNoClip -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.UsingItem -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.Shooting -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.DryfiringWeapon -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.AimingDownSight -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.ChangingRadioPreset -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.ChangingMoveState -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.Interacted -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.TogglingFlashlight -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.TogglingWeaponFlashlight -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.ReloadingWeapon -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.UnloadingWeapon -= this.NotAfkPlayerEventHandler;
        ExHandlers.Player.ThrowingRequest -= this.NotAfkPlayerEventHandler;

        PlayerRoleManager.OnRoleChanged -= this.ChangingRole;
        ExHandlers.Server.WaitingForPlayers -= this.WaitingForPlayers;

        Timing.KillCoroutines(this.mainHandle);

        foreach (AfkInfo info in this.AfkInformation)
        {
            info.Kill();
        }

        MECExtensions.RunAfterFrames(1, Segment.EndOfFrame, AFKManager.ConfigReloaded);
    }

    private void NotAfkTrigger(ReferenceHub hub)
    {
        if (!this.HashedAfkInformation.TryGetValue(hub, out AfkInfo info))
        {
            return;
        }

        info.NotAfkTrigger();
    }

    private void NotAfkPlayerEventHandler(IPlayerEvent ev)
    {
        this.NotAfkTrigger(ev.Player.ReferenceHub);
    }

    private void WaitingForPlayers()
    {
        this.AfkInformation.Clear();
        this.mainHandle.KillAssignNew(this.AfkCoroutine, Segment.Update);
    }

    private void ChangingRole(ReferenceHub userHub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
    {
        if (this.HashedAfkInformation.TryGetValue(userHub, out AfkInfo info))
        {
            info.Kill();
            this.AfkInformation.Remove(info);
        }

        this.AfkInformation.Add(new AfkInfo(userHub, UnityEngine.Time.timeSinceLevelLoadAsDouble + 1f));
    }

    private IEnumerator<float> AfkCoroutine()
    {
        while (true)
        {
            yield return Timing.WaitForOneFrame;

            for (int i = 0; i < this.AfkInformation.Count; i++)
            {
                AfkInfo info = this.AfkInformation[i];

                // If gameObject is null
                // or they are not alive
                // we remove them from the list.
                if (!info.Hub || !info.Hub.gameObject || info.Hub.roleManager.CurrentRole.Team == Team.Dead)
                {
                    info.Kill();
                    this.AfkInformation.RemoveAt(i--);
                    continue;
                }

                // Don't kick global moderators ever.
                if (info.Hub.serverRoles.RaEverywhere)
                {
                    info.NotAfkTrigger();
                    continue;
                }

                // If player is in tutorial tower, we reset their timer.
                if ((info.Hub.transform.position - TutorialTowerPos).sqrMagnitude < TutorialTowerSqrDist)
                {
                    info.NotAfkTrigger();
                    continue;
                }

                if (info.TimeAfk == 0f)
                {
                    info.EnsureStarted();
                    continue;
                }

                // Dont kick players with afk immunity.
                // We still allow AFK time to add up however.
                if ((info.Hub.serverRoles.Permissions & (ulong)PlayerPermissions.AFKImmunity) != 0)
                {
                    continue;
                }

                float timeTillKick = (info.IsActive ? AfkActiveKickTime : AfkSpawnKickTime) - info.TimeAfk;

                if (timeTillKick <= 0f)
                {
                    info.Hub.characterClassManager.DisconnectClient(info.Hub.connectionToClient, "<b>Disconnected for being AFK.\nThis is an automated feature.\nIf you believe this is wrong, contact server administrators or 'o5zereth' on discord.</b>");
                }
                else if (timeTillKick <= 30f && (Time.frameCount % 20) == 0)
                {
                    Broadcast.Singleton.TargetClearElements(info.Hub.connectionToClient);
                    Broadcast.Singleton.TargetAddElement(info.Hub.connectionToClient, $"<b>You are being detected as AFK!\nYou will be kicked in {Mathf.FloorToInt(timeTillKick)} seconds!</b>", 3, Broadcast.BroadcastFlags.Normal);
                }
            }
        }
    }

    /// <summary>
    /// A class used to manage the time a player has been afk.
    /// </summary>
    public sealed class AfkInfo
    {
        private readonly Stopwatch watch;
        private readonly double allowedStartTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="AfkInfo"/> class.
        /// </summary>
        /// <param name="hub">The reference hub associated with this instance.</param>
        /// <param name="allowedStartTime">The time this instance's stopwatch is allowed to start.</param>
        public AfkInfo(ReferenceHub hub, double allowedStartTime)
        {
            this.Hub = hub;
            this.watch = new();
            this.allowedStartTime = allowedStartTime;

            if (Instance)
            {
                Instance.HashedAfkInformation[hub] = this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the player has moved after spawning.
        /// </summary>
        public bool IsActive { get; private set; } = false;

        /// <summary>
        /// Gets the amount of time this player has been detected as AFK.
        /// </summary>
        public float TimeAfk => (float)this.watch.Elapsed.TotalSeconds;

        /// <summary>
        /// Gets the hub associated with this AFK info.
        /// </summary>
        public ReferenceHub Hub { get; }

        /// <summary>
        /// Triggers the time for being AFK to reset.
        /// </summary>
        internal void NotAfkTrigger()
        {
            if (UnityEngine.Time.timeSinceLevelLoadAsDouble < this.allowedStartTime)
            {
                return;
            }

            // Player has moved after spawning.
            this.IsActive = true;

            this.watch.Restart();
        }

        /// <summary>
        /// Ensures the stopwatch for this player is running.
        /// </summary>
        internal void EnsureStarted()
        {
            if (UnityEngine.Time.timeSinceLevelLoadAsDouble < this.allowedStartTime)
            {
                return;
            }

            if (!this.watch.IsRunning)
            {
                this.watch.Start();
            }
        }

        /// <summary>
        /// Kills the current instance.
        /// </summary>
        internal void Kill()
        {
            this.watch.Reset();

            if (Instance && this.Hub != null)
            {
                Instance.HashedAfkInformation.Remove(this.Hub);
            }
        }
    }

#pragma warning disable SA1118 // Parameter should not span multiple lines
#pragma warning disable SA1120 // Comments should contain text

    // This patch detects client fpc data changes
    // to determine that a player is moving the character or mouse.
    [HarmonyPatch(typeof(FpcSyncData), nameof(FpcSyncData.TryApply))]
    private static class FpcSyncDataTryApplyPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            const int offset = 1;
            int index = newInstructions.FindLastIndex(x => x.opcode == OpCodes.Stind_Ref) + offset;

            Label notAfkLabel = generator.DefineLabel();
            Label skipLabel = generator.DefineLabel();

            newInstructions[index].WithLabels(skipLabel);

            // if (AfkDetector.Instance is not null && AfkDetector.Instance.Enabled)
            // {
            //     if (!this._position.Equals(module.Motor.ReceivedPosition)
            //         || this._rotH != module.MouseLook._prevSyncH
            //         || this._rotV != module.MouseLook._prevSyncV)
            //     {
            //         AfkDetector.Instance.NotAfkTrigger(hub);
            //     }
            // }
            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                // if (AfkDetector.Instance is null || !AfkDetector.Instance.Enabled)
                // {
                //     goto skipLabel;
                // }
                new(OpCodes.Call, PropertyGetter(typeof(AfkDetector), nameof(Instance))),
                new(OpCodes.Brfalse_S, skipLabel),
                new(OpCodes.Call, PropertyGetter(typeof(AfkDetector), nameof(Instance))),
                new(OpCodes.Call, PropertyGetter(typeof(BananaFeature), nameof(Enabled))),
                new(OpCodes.Brfalse_S, skipLabel),

                // if (!this._position.Equals(module.Motor.ReceivedPosition))
                // {
                //     goto notAfkLabel;
                // }
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldflda, Field(typeof(FpcSyncData), nameof(FpcSyncData._position))),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Ldind_Ref),
                new(OpCodes.Callvirt, PropertyGetter(typeof(FirstPersonMovementModule), nameof(FirstPersonMovementModule.Motor))),
                new(OpCodes.Callvirt, PropertyGetter(typeof(FpcMotor), nameof(FpcMotor.ReceivedPosition))),
                new(OpCodes.Call, Method(typeof(RelativePosition), nameof(RelativePosition.Equals), new Type[] { typeof(RelativePosition) })),
                new(OpCodes.Brfalse_S, notAfkLabel),

                // if (this._rotH != module.MouseLook._prevSyncH)
                // {
                //     goto notAfkLabel;
                // }
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, Field(typeof(FpcSyncData), nameof(FpcSyncData._rotH))),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Ldind_Ref),
                new(OpCodes.Callvirt, PropertyGetter(typeof(FirstPersonMovementModule), nameof(FirstPersonMovementModule.MouseLook))),
                new(OpCodes.Ldfld, Field(typeof(FpcMouseLook), nameof(FpcMouseLook._prevSyncH))),
                new(OpCodes.Ceq),
                new(OpCodes.Brfalse_S, notAfkLabel),

                // if (this._rotV != module.MouseLook._prevSyncV)
                // {
                //     goto notAfkLabel;
                // }
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, Field(typeof(FpcSyncData), nameof(FpcSyncData._rotV))),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Ldind_Ref),
                new(OpCodes.Callvirt, PropertyGetter(typeof(FirstPersonMovementModule), nameof(FirstPersonMovementModule.MouseLook))),
                new(OpCodes.Ldfld, Field(typeof(FpcMouseLook), nameof(FpcMouseLook._prevSyncV))),
                new(OpCodes.Ceq),
                new(OpCodes.Brfalse_S, notAfkLabel),
                new(OpCodes.Br_S, skipLabel),

                // notAfkLabel:
                // AfkDetector.Instance.NotAfkTrigger(hub);
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(AfkDetector), nameof(Instance))).WithLabels(notAfkLabel),
                new(OpCodes.Ldarg_1),
                new(OpCodes.Call, Method(typeof(AfkDetector), nameof(NotAfkTrigger))),
            });

            return newInstructions.FinishTranspiler();
        }
    }

    //
    // All this jank is to ensure remote admin commands and
    // game console commands count as not being afk.
    //
    // Remote Admin sends frequent queries from client to server to display
    // player list information, class information, playerids, etc.
    //
    // This makes sure that those default queries are ignored as the remote
    // admin panel is open, while still accepting regular remote admin commands.
    //
    [HarmonyPatch]
    private static class QuerySentPatch
    {
        private static QueryProcessor? activeProcessor;

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ProcessGameConsoleQuery))]
        private static IEnumerable<CodeInstruction> GameConsoleTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            instructions.BeginTranspiler(out List<CodeInstruction> newinstructions);

            Label skipLabel = generator.DefineLabel();
            newinstructions[0].labels.Add(skipLabel);

            newinstructions.InsertRange(0, new CodeInstruction[]
            {
                // if (AfkDetector.Instance is not null && AfkDetector.Instance.Enabled)
                // {
                //     AfkDetector.Instance.NotAfkTrigger(this._hub);
                // }
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(AfkDetector), nameof(Instance))),
                new(OpCodes.Brfalse_S, skipLabel),
                new(OpCodes.Call, PropertyGetter(typeof(AfkDetector), nameof(Instance))),
                new(OpCodes.Call, PropertyGetter(typeof(BananaFeature), nameof(Enabled))),
                new(OpCodes.Brfalse_S, skipLabel),
                new(OpCodes.Call, PropertyGetter(typeof(AfkDetector), nameof(Instance))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, Field(typeof(QueryProcessor), nameof(QueryProcessor._hub))),
                new(OpCodes.Call, Method(typeof(AfkDetector), nameof(NotAfkTrigger))),
            });

            return newinstructions.FinishTranspiler();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
        private static IEnumerable<CodeInstruction> CommandProcessorTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            instructions.BeginTranspiler(out List<CodeInstruction> newinstructions);

            Label? notServerCommunicationNullable = null;
            Label skipLabel = generator.DefineLabel();

            int index = newinstructions.FindIndex(x => x.opcode == OpCodes.Ldstr && x.operand is "$");
            index = newinstructions.FindIndex(index, x => x.Branches(out notServerCommunicationNullable));

            if (notServerCommunicationNullable is not Label notServerCommunication)
            {
                throw new NullReferenceException("Could not obtain 'notServerCommunication' label.");
            }

            newinstructions.InsertRange(index + 1, new CodeInstruction[]
            {
                // QuerySentPatch.activeProcessor = null;
                new(OpCodes.Ldnull),
                new(OpCodes.Stsfld, Field(typeof(QuerySentPatch), nameof(activeProcessor))),
            });

            index = newinstructions.FindIndex(x => x.labels.Contains(notServerCommunication));

            newinstructions[index].labels.Remove(notServerCommunication);
            newinstructions[index].labels.Add(skipLabel);

            newinstructions.InsertRange(index, new CodeInstruction[]
            {
                // if (AfkDetector.Instance is not null && AfkDetector.Instance.Enabled && QuerySentPatch.activeProcessor is not null)
                // {
                //     AfkDetector.Instance.NotAfkTrigger(QuerySentPatch.activeProcessor._hub);
                //     QuerySentPatch.activeProcessor = null;
                // }
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(AfkDetector), nameof(Instance))).WithLabels(notServerCommunication),
                new(OpCodes.Brfalse_S, skipLabel),
                new(OpCodes.Call, PropertyGetter(typeof(AfkDetector), nameof(Instance))),
                new(OpCodes.Call, PropertyGetter(typeof(BananaFeature), nameof(Enabled))),
                new(OpCodes.Brfalse_S, skipLabel),
                new(OpCodes.Ldsfld, Field(typeof(QuerySentPatch), nameof(activeProcessor))),
                new(OpCodes.Brfalse_S, skipLabel),
                new(OpCodes.Call, PropertyGetter(typeof(AfkDetector), nameof(Instance))),
                new(OpCodes.Ldsfld, Field(typeof(QuerySentPatch), nameof(activeProcessor))),
                new(OpCodes.Ldfld, Field(typeof(QueryProcessor), nameof(QueryProcessor._hub))),
                new(OpCodes.Call, Method(typeof(AfkDetector), nameof(NotAfkTrigger))),
                new(OpCodes.Ldnull),
                new(OpCodes.Stsfld, Field(typeof(QuerySentPatch), nameof(activeProcessor))),
            });

            return newinstructions.FinishTranspiler();
        }

        [HarmonyPatch]
        private static class RemoteAdminQueryPatch
        {
            private static IEnumerable<MethodInfo> TargetMethods()
            {
                yield return Method(typeof(QueryProcessor), "UserCode_CmdSendEncryptedQuery__Byte[]");
                yield return Method(typeof(QueryProcessor), "UserCode_CmdSendQuery__String__Int32__Byte[]");
            }

            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                instructions.BeginTranspiler(out List<CodeInstruction> newinstructions);

                Label skipLabel = generator.DefineLabel();

                newinstructions[0].labels.Add(skipLabel);

                newinstructions.InsertRange(0, new CodeInstruction[]
                {
                // if (AfkDetector.Instance is not null && AfkDetector.Instance.Enabled)
                // {
                //     QuerySentPatch.activeProcessor = this;
                // }
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(AfkDetector), nameof(Instance))),
                new(OpCodes.Brfalse_S, skipLabel),
                new(OpCodes.Call, PropertyGetter(typeof(AfkDetector), nameof(Instance))),
                new(OpCodes.Call, PropertyGetter(typeof(BananaFeature), nameof(Enabled))),
                new(OpCodes.Brfalse_S, skipLabel),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Stsfld, Field(typeof(QuerySentPatch), nameof(activeProcessor))),
                });

                return newinstructions.FinishTranspiler();
            }
        }
    }

    // This just makes sure default AFK kicking is disabled.
    [HarmonyPatch(typeof(AFKManager), nameof(AFKManager.ConfigReloaded))]
    private static class AFKManagerPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            Label assignLabel = generator.DefineLabel();
            Label assignDefaultLabel = generator.DefineLabel();

            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Ldstr && x.operand is "afk_time") - 1;

            float defaultValue = (float)newInstructions[index + 2].operand;

            newInstructions.RemoveRange(index, 5);

            newInstructions.InsertRange(index, new CodeInstruction[]
            {
                // AFKManager._kickTime = AfkDetector.Instance is not null && AfkDetector.Instance.Enabled
                //     ? 0f
                //     : defaultValue;
                new(OpCodes.Call, PropertyGetter(typeof(AfkDetector), nameof(Instance))),
                new(OpCodes.Brfalse_S, assignDefaultLabel),
                new(OpCodes.Call, PropertyGetter(typeof(AfkDetector), nameof(Instance))),
                new(OpCodes.Call, PropertyGetter(typeof(BananaFeature), nameof(Enabled))),
                new(OpCodes.Brfalse_S, assignDefaultLabel),
                new(OpCodes.Ldc_R4, 0f),
                new(OpCodes.Br_S, assignLabel),
                new CodeInstruction(OpCodes.Ldc_R4, defaultValue)
                    .WithLabels(assignDefaultLabel),
                new CodeInstruction(OpCodes.Stsfld, Field(typeof(AFKManager), nameof(AFKManager._kickTime)))
                    .WithLabels(assignLabel),
            });

            return newInstructions.FinishTranspiler();
        }
    }
#pragma warning restore SA1120 // Comments should contain text
#pragma warning restore SA1118 // Parameter should not span multiple lines
}
