namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.Extensions;
using BananaPlugin.Features.Configs;
using Exiled.API.Features;
using InventorySystem.Disarming;
using MEC;
using PlayerRoles;
using Respawning;
using System.Collections.Generic;

/// <summary>
/// The main feature that allows guards to escape as chaos insurgency.
/// </summary>
public sealed class GuardEscape : BananaFeatureConfig<CfgGuardEscape>
{
    private CoroutineHandle mainHandle;

    /// <summary>
    /// Initializes a new instance of the <see cref="GuardEscape"/> class.
    /// </summary>
    private GuardEscape()
    {
        if (!Plugin.AssertEnabled())
        {
            throw new System.InvalidOperationException();
        }

        this.LocalConfig = Plugin.Instance.Config.GuardEscape;
        Config.GuardEscapeUpdated = this.OnConfigUpdated;
    }

    /// <inheritdoc/>
    public override string Name => "Guard Escape";

    /// <inheritdoc/>
    public override string Prefix => "guardesc";

    /// <inheritdoc/>
    public override CfgGuardEscape LocalConfig { get; protected set; }

    /// <inheritdoc/>
    protected override void Enable()
    {
        ExHandlers.Server.RoundStarted += this.OnRoundStarted;

        if (Round.IsStarted)
        {
            this.OnRoundStarted();
        }
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ExHandlers.Server.RoundStarted -= this.OnRoundStarted;

        Timing.KillCoroutines(this.mainHandle);
    }

    private void OnRoundStarted()
    {
        this.mainHandle.AssignNew(this.MainCoroutine, Segment.Update);
    }

    private IEnumerator<float> MainCoroutine()
    {
        BPLogger.IdentifyMethodAs("GuardEscape", "MainCoroutine");

        while (Round.IsStarted)
        {
            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub.roleManager.CurrentRole is not HumanRole hRole)
                {
                    continue;
                }

                if (hRole.RoleTypeId != RoleTypeId.FacilityGuard)
                {
                    continue;
                }

                if ((hRole.FpcModule.Position - Escape.WorldPos).sqrMagnitude > Escape.RadiusSqr)
                {
                    continue;
                }

                if (hRole.ActiveTime < 10f)
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
}
