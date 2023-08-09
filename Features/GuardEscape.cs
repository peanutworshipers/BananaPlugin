namespace BananaPlugin.Features;

using BananaPlugin.API.Attributes;
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
[AllowedPorts(ServerPorts.ServerOne, ServerPorts.ServerTwo, ServerPorts.ServerThree)]
public sealed class GuardEscape : BananaFeatureConfig<CfgBetterEscape>
{
    private CoroutineHandle mainHandle;

    /// <inheritdoc/>
    public override string Name => "Guard Escape";

    /// <inheritdoc/>
    public override string Prefix => "guardesc";

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

    /// <inheritdoc/>
    protected override CfgBetterEscape RetrieveLocalConfig(Config config) => config.BetterEscape;

    private void OnRoundStarted()
    {
        this.mainHandle.AssignNew(this.MainCoroutine, Segment.Update);
    }

    private IEnumerator<float> MainCoroutine()
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
}
