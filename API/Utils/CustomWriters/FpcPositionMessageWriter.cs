namespace BananaPlugin.API.Utils.CustomWriters;

using BananaPlugin.Extensions;
using Exiled.API.Features;
using Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using RelativePositioning;
using System;
using UnityEngine;

/// <summary>
/// A class used to override the <see cref="FpcPositionMessage"/> writer to allow forced client values.
/// </summary>
public static class FpcPositionMessageWriter
{
    private static AppliedValues valuesToApply;

    private static (ushort horizontal, ushort vertical) appliedMouseLook;
    private static RelativePosition appliedPosition;
    private static PlayerMovementState appliedMovementState;

    static FpcPositionMessageWriter()
    {
        Writer<FpcPositionMessage>.write = WriteFpcPositionMessage;
    }

    /// <summary>
    /// The byte flags used by <see cref="FpcSyncData"/> for bools.
    /// </summary>
    [Flags]
    public enum FpcSyncDataByteFlags : byte
    {
        /// <summary>
        /// The value used to specify the <see cref="PlayerMovementState.Crouching"/> state.
        /// </summary>
        /// <remarks>This flag is only viewable when all bit values represented beyond 1^2 are excluded.</remarks>
        Crouching = 0,

        /// <summary>
        /// The value used to specify the <see cref="PlayerMovementState.Sneaking"/> state.
        /// </summary>
        Sneaking = 1,

        /// <summary>
        /// The value used to specify the <see cref="PlayerMovementState.Walking"/> state.
        /// </summary>
        Walking = 2,

        /// <summary>
        /// The value used to specify the <see cref="PlayerMovementState.Sprinting"/> state.
        /// </summary>
        Sprinting = 3,

        /// <summary>
        /// The custom bit. Used for isGrounded.
        /// </summary>
        BitCustom = 1 << 7,

        /// <summary>
        /// The position bit. Used to determine if the message contains position information.
        /// </summary>
        BitPosition = 1 << 6,

        /// <summary>
        /// The mouseLook bit. Used to determine if the message contains mouseLook information.
        /// </summary>
        BitMouseLook = 1 << 5,
    }

    [Flags]
    private enum AppliedValues : byte
    {
        ApplyMouseLook = 1,
        ApplyPosition = 1 << 1,
        ApplyMovementState = 1 << 2,
    }

    /// <summary>
    /// Sets a specified player's client camera rotation.
    /// </summary>
    /// <param name="player">The player to set the rotation of.</param>
    /// <param name="forward">The forward direction of the rotation.</param>
    public static void SetRotationForward(this Player player, Vector3 forward)
        => player.ReferenceHub.SetHubRotationForward(forward);

    /// <summary>
    /// Sets a specified player's client camera rotation.
    /// </summary>
    /// <param name="player">The player to set the rotation of.</param>
    /// <param name="horizontal">The horizontal value of the rotation.</param>
    /// <param name="vertical">The vertical value of the rotation.</param>
    public static void SetRotation(this Player player, ushort horizontal, ushort vertical)
        => player.ReferenceHub.SetHubRotation(horizontal, vertical);

    /// <summary>
    /// Sets a specified player's client camera rotation.
    /// </summary>
    /// <param name="hub">The player to set the rotation of.</param>
    /// <param name="forward">The forward direction of the rotation.</param>
    public static void SetHubRotationForward(this ReferenceHub hub, Vector3 forward)
    {
        (ushort horizontal, ushort vertical) = Quaternion.LookRotation(forward, Vector3.up).ToClientUShorts();

        hub.SetHubRotation(horizontal, vertical);
    }

    /// <summary>
    /// Sets a specified player's client camera rotation.
    /// </summary>
    /// <param name="hub">The player to set the rotation of.</param>
    /// <param name="horizontal">The horizontal value of the rotation.</param>
    /// <param name="vertical">The vertical value of the rotation.</param>
    public static void SetHubRotation(this ReferenceHub hub, ushort horizontal, ushort vertical)
    {
        if (hub.roleManager.CurrentRole is not IFpcRole)
        {
            return;
        }

        appliedMouseLook = (horizontal, vertical);
        valuesToApply |= AppliedValues.ApplyMouseLook;
        hub.connectionToClient.Send(new FpcPositionMessage(hub));
    }

    /// <summary>
    /// Sets a specified player's client movement state.
    /// </summary>
    /// <param name="player">The player to set the movement state of.</param>
    /// <param name="movementState">The movement state to apply to the player.</param>
    public static void SetMovementState(this Player player, PlayerMovementState movementState)
        => player.ReferenceHub.SetHubMovementState(movementState);

    /// <summary>
    /// Sets a specified player's client movement state.
    /// </summary>
    /// <param name="hub">The player to set the movement state of.</param>
    /// <param name="movementState">The movement state to apply to the player.</param>
    public static void SetHubMovementState(this ReferenceHub hub, PlayerMovementState movementState)
    {
        if (hub.roleManager.CurrentRole is not IFpcRole)
        {
            return;
        }

        appliedMovementState = movementState;
        valuesToApply |= AppliedValues.ApplyMovementState;
        hub.connectionToClient.Send(new FpcPositionMessage(hub));
    }

    private static void WriteFpcPositionMessage(NetworkWriter writer, FpcPositionMessage message)
    {
        if (valuesToApply > 0)
        {
            WriteCustomFpcPositionMessage(writer, message, valuesToApply);
            valuesToApply = 0;
        }
        else
        {
            FpcServerPositionDistributor.WriteAll(message._receiver, writer);
        }
    }

    private static void WriteCustomFpcPositionMessage(NetworkWriter writer, FpcPositionMessage message, AppliedValues appliedValues)
    {
        ReferenceHub receiver = message._receiver;
        FirstPersonMovementModule fpcModule = ((IFpcRole)receiver.roleManager.CurrentRole).FpcModule;

        writer.Write((ushort)1);
        writer.Write(receiver._playerId);

        bool applyMouseLook = (appliedValues & AppliedValues.ApplyMouseLook) != 0;
        bool applyPosition = (appliedValues & AppliedValues.ApplyPosition) != 0;
        bool applyMovementState = (appliedValues & AppliedValues.ApplyMovementState) != 0;

        (ushort horizontal, ushort vertical) = applyMouseLook
            ? appliedMouseLook
            : default;

        PlayerMovementState movementState = applyMovementState
            ? (appliedMovementState & (PlayerMovementState)3)
            : fpcModule.CurrentMovementState;

        RelativePosition position = applyPosition
            ? appliedPosition
            : default;

        byte fpcSyncByte = (byte)movementState;

        if (applyMouseLook)
        {
            fpcSyncByte |= (byte)FpcSyncDataByteFlags.BitMouseLook;
        }

        if (applyPosition)
        {
            fpcSyncByte |= (byte)FpcSyncDataByteFlags.BitPosition;
        }

        if (fpcModule.IsGrounded)
        {
            fpcSyncByte |= (byte)FpcSyncDataByteFlags.BitCustom;
        }

        writer.Write(fpcSyncByte);

        if (applyPosition)
        {
            writer.WriteRelativePosition(position);
        }

        if (applyMouseLook)
        {
            writer.WriteUShort(horizontal);
            writer.WriteUShort(vertical);
        }
    }
}
