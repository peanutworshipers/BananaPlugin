namespace BananaPlugin.Features;

using BananaPlugin.API.Main;
using BananaPlugin.API.Utils;
using BananaPlugin.Extensions;
using Exiled.API.Features;
using InventorySystem.Disarming;
using MEC;
using PlayerRoles;
using Respawning;
using System.Collections.Generic;

/// <summary>
/// The main feature that allows guards to escape as chaos insurgency.
/// </summary>
public sealed class GuardEscape : BananaFeature
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
    }

    /// <inheritdoc/>
    protected override void Disable()
    {
        ExHandlers.Server.RoundStarted -= this.OnRoundStarted;

        Timing.KillCoroutines(this.mainHandle);
    }

    private static IEnumerator<float> MainCoroutine()
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

                BPLogger.Debug("is guard!!!");

                if (!hub.inventory.IsDisarmed())
                {
                    continue;
                }

                hub.roleManager.ServerSetRole(RoleTypeId.ChaosConscript, RoleChangeReason.Escaped);
                RespawnTokensManager.GrantTokens(SpawnableTeamType.ChaosInsurgency, Plugin.Instance!.Config.GuardEscapeTokens);

                BPLogger.Debug($"Cuffed guard escaped. ({hub.nicknameSync.MyNick})");
            }

            yield return Timing.WaitForOneFrame;
        }
    }

    private void OnRoundStarted()
    {
        this.mainHandle.AssignNew(MainCoroutine, Segment.Update);
    }
}
