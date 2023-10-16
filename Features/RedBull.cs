namespace BananaPlugin.Features;

using BananaPlugin.API;
using BananaPlugin.API.Interfaces;
using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.Extensions;
using CommandSystem;
using CustomPlayerEffects;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp914;
using HarmonyLib;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using Scp914;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static HarmonyLib.AccessTools;
using UsableAntiScp207 = InventorySystem.Items.Usables.AntiScp207;
using UsableScp207 = InventorySystem.Items.Usables.Scp207;

/// <summary>
/// The main feature responsible for custom cola items.
/// </summary>
public sealed class CustomCola : BananaFeature
{
    /// <summary>
    /// An array of all cola types.
    /// </summary>
    public static readonly ColaType[] ColaTypes = (ColaType[])Enum.GetValues(typeof(ColaType));

    private HashSet<ushort>? redBullSerials;

    static CustomCola()
    {
        PlayerEffectsController cont = NetworkManager.singleton.playerPrefab.GetComponent<PlayerEffectsController>();

        GameObject redBullEffect = new GameObject("RedBull", typeof(RedBull));
        redBullEffect.transform.SetParent(cont.effectsGameObject.transform);
        redBullEffect.SetActive(true);
        redBullEffect.GetComponent<RedBull>().enabled = true;
        redBullEffect.transform.localPosition = Vector3.zero;
        redBullEffect.transform.localRotation = Quaternion.identity;
    }

    private CustomCola()
    {
        Instance = this;
        this.Commands =
        [
            new GiveRedBull(this),
        ];
    }

    /// <summary>
    /// An enumeration that specifies a type of cola.
    /// </summary>
    public enum ColaType
    {
        /// <summary>
        /// Specifies no cola type.
        /// </summary>
        None,

        /// <summary>
        /// Specifies the SCP-207 cola.
        /// </summary>
        Scp207,

        /// <summary>
        /// Specifies the Anti SCP-207 cola.
        /// </summary>
        AntiScp207,

        /// <summary>
        /// Specifies the Red Bull cola.
        /// </summary>
        RedBull,

        /// <summary>
        /// Specifies a custom SCP-294 cola.
        /// </summary>
        Scp294,
    }

    /// <summary>
    /// Gets the Custom Cola instance.
    /// </summary>
    public static CustomCola? Instance { get; private set; }

    /// <inheritdoc/>
    public override string Name => "Custom Colas";

    /// <inheritdoc/>
    public override string Prefix => "cola";

    /// <inheritdoc/>
    public override ICommand[] Commands { get; }

    /// <summary>
    /// Gets the next cola type for a <see cref="Scp914KnobSetting.OneToOne"/> upgrade depending on the current cola type.
    /// </summary>
    /// <param name="curType">The current cola type.</param>
    /// <returns>A new cola type representing the upgrade output.</returns>
    /// <exception cref="NotImplementedException">Thrown when something is not implemented correctly.</exception>
    public static ColaType GetNextCola(ColaType curType)
    {
        if (curType == ColaType.None || curType == ColaType.Scp294)
        {
            return ColaType.None;
        }

        bool randomValue = UnityEngine.Random.value <= 0.5f;

        return curType switch
        {
            ColaType.Scp207 => randomValue ? ColaType.AntiScp207 : ColaType.RedBull,
            ColaType.AntiScp207 => randomValue ? ColaType.Scp207 : ColaType.RedBull,
            ColaType.RedBull => randomValue ? ColaType.Scp207 : ColaType.AntiScp207,
            _ => throw new NotImplementedException("Something is not implemented correctly.")
        };
    }

    /// <summary>
    /// Gets the cola type of the specified item.
    /// </summary>
    /// <param name="item">The item to evaluate.</param>
    /// <returns>An enumeration specifying the cola type of the item.</returns>
    public ColaType GetColaType(ItemBase? item)
    {
#pragma warning disable IDE0046 // Convert to conditional expression (for readability)
        if (item == null || !item.gameObject)
        {
            return ColaType.None;
        }

        ushort serial = item.ItemSerial;

        if (Type.GetType("SCP294.SCP294") is not null && InternalCheckSCP294(item))
        {
            return ColaType.Scp294;
        }

        if (this.redBullSerials?.Contains(serial) ?? false)
        {
            return ColaType.RedBull;
        }

        return item.ItemTypeId switch
        {
            ItemType.SCP207 => ColaType.Scp207,
            ItemType.AntiSCP207 => ColaType.AntiScp207,
            _ => ColaType.None,
        };

        static bool InternalCheckSCP294(ItemBase? item)
        {
            return Scp294Plugin.Instance?.CustomDrinkItems.ContainsKey(item!.ItemSerial) ?? false;
        }
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    /// <summary>
    /// Gets the cola type of the specified pickup.
    /// </summary>
    /// <param name="pickup">The pickup to evaluate.</param>
    /// <returns>An enumeration specifying the cola type of the item.</returns>
    public ColaType GetColaType(ItemPickupBase? pickup)
    {
#pragma warning disable IDE0046 // Convert to conditional expression
        if (pickup == null || !pickup.gameObject)
        {
            return ColaType.None;
        }

        ushort serial = pickup.Info.Serial;

        if (Scp294Plugin.Instance?.CustomDrinkItems.ContainsKey(serial) ?? false)
        {
            return ColaType.Scp294;
        }

        if (this.redBullSerials?.Contains(serial) ?? false)
        {
            return ColaType.RedBull;
        }

        return pickup.Info.ItemId switch
        {
            ItemType.SCP207 => ColaType.Scp207,
            ItemType.AntiSCP207 => ColaType.AntiScp207,
            _ => ColaType.None,
        };
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    /// <summary>
    /// Changes the cola type of the specified serial.
    /// </summary>
    /// <param name="serial">The serial to change.</param>
    /// <param name="type">The new type of cola to change to.</param>
    /// <remarks>This only works for custom colas.</remarks>
    public void ChangeColaType(ushort serial, ColaType type)
    {
        switch (type)
        {
            case ColaType.RedBull:
                this.redBullSerials!.Add(serial);
                return;

            default:
                this.redBullSerials!.Remove(serial);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void Enable()
    {
        this.redBullSerials = new();

        ExHandlers.Scp914.UpgradingPickup += this.UpgradingPickup;
        ExHandlers.Player.ChangedItem += this.ChangedItem;
        ExHandlers.Player.Hurting += this.Hurting;
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        this.redBullSerials = null;

        ExHandlers.Scp914.UpgradingPickup -= this.UpgradingPickup;
        ExHandlers.Player.ChangedItem -= this.ChangedItem;
        ExHandlers.Player.Hurting -= this.Hurting;
    }

    private void UpgradingPickup(UpgradingPickupEventArgs ev)
    {
        ColaType colaType = this.GetColaType(ev.Pickup.Base);

        if (colaType == ColaType.None)
        {
            return;
        }

        if (ev.KnobSetting != Scp914KnobSetting.OneToOne)
        {
            return;
        }

        Quaternion itemRot = ev.Pickup.Rotation;

        ev.Pickup.Destroy();
        ev.IsAllowed = false;

        colaType = GetNextCola(colaType);

        if (colaType == ColaType.None)
        {
            ev.Pickup.Destroy();
            ev.IsAllowed = false;
            return;
        }

        switch (colaType)
        {
            case ColaType.Scp207:
                Pickup.CreateAndSpawn(ItemType.SCP207, ev.OutputPosition, itemRot);
                break;

            case ColaType.AntiScp207:
                Pickup.CreateAndSpawn(ItemType.AntiSCP207, ev.OutputPosition, itemRot);
                break;

            case ColaType.RedBull:
                Pickup pickup = Pickup.CreateAndSpawn(ItemType.AntiSCP207, ev.OutputPosition, itemRot);

                this.redBullSerials!.Add(pickup.Serial);
                break;
        }
    }

    private void ChangedItem(ChangedItemEventArgs ev)
    {
        ColaType colaType = this.GetColaType(ev.Item?.Base);

        if (colaType <= ColaType.AntiScp207 || colaType == ColaType.Scp294)
        {
            return;
        }

        string message = colaType switch
        {
            ColaType.RedBull => "You equipped a red bull.",
            _ => throw new NotImplementedException("Something is not implemented correctly."),
        };

        ev.Player.ShowHint(message);
    }

    private void Hurting(HurtingEventArgs ev)
    {
        if (!ev.IsAllowed)
        {
            return;
        }

        if (ev.Amount >= 10f && ev.Player.ReferenceHub.playerEffectsController.TryGetEffect(out RedBull redBull))
        {
            redBull.StallAhp(10f);
        }
    }

    /// <remarks>Used by <see cref="ColaPatch"/> class.</remarks>
    private void OnCustomEffectsActivated(ColaType cola, ItemBase item)
    {
        try
        {
            switch (cola)
            {
                case ColaType.RedBull:
                    if (item.Owner.playerEffectsController.TryGetEffect(out RedBull playerEffect))
                    {
                        playerEffect.Intensity++;
                    }

                    return;
            }
        }
        catch (Exception ex)
        {
            BPLogger.Error(ex.ToString());
        }
    }

    /// <summary>
    /// The command responsible for giving players a redbull.
    /// </summary>
    public sealed class GiveRedBull : IFeatureSubcommand<CustomCola>, IRequiresRank
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GiveRedBull"/> class.
        /// </summary>
        /// <param name="parent">The parent of this command.</param>
        public GiveRedBull(CustomCola parent)
        {
            this.Parent = parent;
        }

        /// <inheritdoc/>
        public CustomCola Parent { get; }

        /// <inheritdoc/>
        public string Command => "gredbull";

        /// <inheritdoc/>
        public string[] Aliases => Array.Empty<string>();

        /// <inheritdoc/>
        public string Description => "Gives you a red bull.";

        /// <inheritdoc/>
        public string[] Usage => Array.Empty<string>();

        /// <inheritdoc/>
        public BRank RankRequirement => BRank.Administrator;

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string? response)
        {
            if (!sender.CheckPermission(BRank.Administrator, out response))
            {
                return false;
            }

            if (!this.Parent.Enabled)
            {
                response = "The feature associated with this command is disabled.";
                return false;
            }

            if (!ExPlayer.TryGet(sender, out ExPlayer player))
            {
                response = "You must be a player to run this command.";
                return false;
            }

            if (player.Inventory.UserInventory.Items.Count >= 8)
            {
                response = "You cannot use this command when your inventory is full.";
                return false;
            }

            ExItem item = player.AddItem(ItemType.AntiSCP207);
            this.Parent.redBullSerials!.Add(item.Serial);
            response = "Gave you a red bull.";
            return true;
        }

        /// <inheritdoc/>
        public string GetHelp(ArraySegment<string> arguments)
        {
            return this.HelpProviderFormat("Run the command to give yourself a red bull cola.");
        }
    }

    /// <summary>
    /// The custom player effect for the red bull cola.
    /// </summary>
    public sealed class RedBull : CokeBase, IStaminaModifier
    {
        /// <summary>
        /// The AHP decay after the redbull effect has been removed.
        /// </summary>
        public const float NotActiveDecay = 2f;

        /// <summary>
        /// The efficacy of the redbull effect AHP.
        /// </summary>
        public const float Efficacy = 0.85f;

        /// <summary>
        /// Gets the AHP gain based on the intensity of the red bull effect.
        /// </summary>
        public static readonly float[] AHPGain = [0f, 1.5f, 2f, 2.5f, 3.5f];

        /// <summary>
        /// Gets the AHP limit based on the intensity of the red bull effect.
        /// </summary>
        public static readonly float[] Limit = [0f, 70f, 75f, 85f, 100f];

        private static int? invigoratedIndex;
        private static int? hemorrhageIndex;

        private Invigorated? invigoratedEffect;
        private Hemorrhage? hemorrhageEffect;
        private AhpStat? ahpStat;
        private AhpStat.AhpProcess? ahpProcess;
        private float stallTime;
        private bool isSprinting;

        /// <inheritdoc/>
        public override EffectClassification Classification => EffectClassification.Mixed;

        /// <inheritdoc/>
        public override byte MaxIntensity => 4;

        /// <inheritdoc/>
        public override float MovementSpeedMultiplier => 1f;

        /// <inheritdoc/>
        public bool StaminaModifierActive => this.IsEnabled;

        /// <inheritdoc/>
        public float StaminaUsageMultiplier => 0f;

        /// <inheritdoc/>
        public float StaminaRegenMultiplier => 1f;

        /// <inheritdoc/>
        public bool SprintingDisabled => false;

        /// <inheritdoc/>
        public override void OnAwake()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            try
            {
                this.StackMultipliers = [new() { DamageMultiplier = 1f, PostProcessIntensity = 0f, SpeedMultiplier = 1f }];

                base.OnAwake();

                this.ahpProcess = null;
                this.stallTime = 0f;

                StatusEffectBase[] effects = this.Hub.playerEffectsController.AllEffects;

                if (invigoratedIndex == null || hemorrhageIndex == null)
                {
                    invigoratedIndex = -1;
                    hemorrhageIndex = -1;

                    for (int i = 0; i < effects.Length; i++)
                    {
                        if (effects[i] is Invigorated)
                        {
                            invigoratedIndex = i;
                        }
                        else if (effects[i] is Hemorrhage)
                        {
                            hemorrhageIndex = i;
                        }
                    }
                }

                this.invigoratedEffect = (Invigorated)effects[invigoratedIndex.Value];
                this.hemorrhageEffect = (Hemorrhage)effects[hemorrhageIndex.Value];
            }
            catch (Exception ex)
            {
                BPLogger.Error(ex.ToString());
            }
        }

        /// <inheritdoc/>
        public override void OnEffectUpdate()
        {
            base.OnEffectUpdate();

            this.Hub.playerEffectsController._syncEffectsIntensity[invigoratedIndex!.Value] = 1;
            this.Hub.playerEffectsController._syncEffectsIntensity[hemorrhageIndex!.Value] = 1;

            if (this.Hub.roleManager.CurrentRole is IFpcRole fpcRole)
            {
                this.isSprinting = fpcRole.FpcModule.CurrentMovementState == PlayerMovementState.Sprinting;
            }

            if (this.ahpProcess is null)
            {
                return;
            }

            if (this.stallTime > 0f)
            {
                this.stallTime -= Time.deltaTime;
            }

            this.ahpProcess.DecayRate = this.stallTime > 0f
                ? 0f
                : -AHPGain[this._intensity];

            this.ahpProcess.Limit = Limit[this._intensity];
        }

        /// <inheritdoc/>
        public override void OnTick()
        {
            if (this.isSprinting)
            {
                this.Hub.playerStats.DealDamage(new RedBullDamage(this._intensity / 1.7f));
            }
        }

        /// <inheritdoc/>
        public override void Enabled()
        {
            try
            {
                base.Enabled();

                this.EnsureFields();

                this.stallTime = 0f;

                float initialAmount = 0f;

                if (this.ahpProcess is not null)
                {
                    initialAmount = this.ahpProcess.CurrentAmount;
                    this.ahpStat!.ServerKillProcess(this.ahpProcess.KillCode);
                }

                this.ahpProcess = this.ahpStat.ServerAddProcess(initialAmount, Limit[this._intensity], -AHPGain[this._intensity], Efficacy, 0f, true);

                this.Hub.playerEffectsController._syncEffectsIntensity[invigoratedIndex!.Value] = 1;
                this.Hub.playerEffectsController._syncEffectsIntensity[hemorrhageIndex!.Value] = 1;
            }
            catch (Exception ex)
            {
                BPLogger.Error(ex.ToString());
            }
        }

        /// <inheritdoc/>
        public override void Disabled()
        {
            base.Disabled();

            this.EnsureFields();

            this.stallTime = 0f;

            if (!this.invigoratedEffect!.IsEnabled)
            {
                this.Hub.playerEffectsController._syncEffectsIntensity[invigoratedIndex!.Value] = 0;
            }

            if (!this.hemorrhageEffect!.IsEnabled)
            {
                this.Hub.playerEffectsController._syncEffectsIntensity[hemorrhageIndex!.Value] = 0;
            }

            if (this.ahpProcess is null)
            {
                return;
            }

            float amount = this.ahpProcess.CurrentAmount;
            this.ahpStat.ServerKillProcess(this.ahpProcess.KillCode);
            this.ahpProcess = this.ahpStat.ServerAddProcess(amount, Limit[this.MaxIntensity], NotActiveDecay, Efficacy, 0, false);
        }

        /// <inheritdoc/>
        public override void IntensityChanged(byte prevState, byte newState)
        {
            if (newState > this.MaxIntensity || newState == 0 || prevState == 0 || this.ahpProcess is null)
            {
                return;
            }

            this.ahpProcess.DecayRate = -AHPGain[newState];
        }

        /// <summary>
        /// Stalls the effect's AHP gain for a specified time.
        /// </summary>
        /// <param name="time">The time to stall for.</param>
        public void StallAhp(float time)
        {
            this.stallTime = time;
        }

        [MemberNotNull(nameof(ahpStat))]
        private void EnsureFields()
        {
            this.ahpStat ??= this.Hub.playerStats.GetModule<AhpStat>();
        }

        /// <summary>
        /// Damage handler that bypasses hume and AHP.
        /// </summary>
        private class RedBullDamage : CustomReasonDamageHandler
        {
            public RedBullDamage(float damage)
                : base("Got too high on red bull...", damage, "TERMINATED BY G FUEL")
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
    }

    [HarmonyPatch]
    private static class ColaPatch
    {
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            yield return Method(typeof(UsableScp207), nameof(UsableScp207.OnEffectsActivated));
            yield return Method(typeof(UsableAntiScp207), nameof(UsableAntiScp207.OnEffectsActivated));
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            instructions.BeginTranspiler(out List<CodeInstruction> newInstructions);

            Label allowLabel = generator.DefineLabel();

            newInstructions[0].labels.Add(allowLabel);

            LocalBuilder colaType = generator.DeclareLocal(typeof(ColaType));

#pragma warning disable SA1118 // Parameter should not span multiple lines
            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                // if (CustomCola.Instance is null || !CustomCola.Instance.Enabled)
                // {
                //     goto allow;
                // }
                new(OpCodes.Call, PropertyGetter(typeof(CustomCola), nameof(Instance))),
                new(OpCodes.Brfalse_S, allowLabel),
                new(OpCodes.Call, PropertyGetter(typeof(CustomCola), nameof(Instance))),
                new(OpCodes.Call, PropertyGetter(typeof(BananaFeature), nameof(Enabled))),
                new(OpCodes.Brfalse_S, allowLabel),

                // ColaType colaType = CustomCola.Instance.GetColaType(this);
                // if (colaType <= ColaType.AntiScp207)
                // {
                //     goto allow;
                // }
                // CustomCola.Instance.OnCustomEffectsActivated(colaType, this);
                // return;
                new(OpCodes.Call, PropertyGetter(typeof(CustomCola), nameof(Instance))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, Method(typeof(CustomCola), nameof(GetColaType), new Type[] { typeof(ItemBase) })),
                new(OpCodes.Stloc_S, colaType),
                new(OpCodes.Ldloc_S, colaType),
                new(OpCodes.Ldc_I4, (int)ColaType.AntiScp207),
                new(OpCodes.Ble_S, allowLabel),
                new(OpCodes.Call, PropertyGetter(typeof(CustomCola), nameof(Instance))),
                new(OpCodes.Ldloc_S, colaType),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, Method(typeof(CustomCola), nameof(OnCustomEffectsActivated))),
                new(OpCodes.Ret),

            });
#pragma warning restore SA1118 // Parameter should not span multiple lines

            return newInstructions.FinishTranspiler();
        }
    }
}
