namespace BananaPlugin.Extensions;

using PlayerRoles.FirstPersonControl.NetworkMessages;
using UnityEngine;

#warning add docs
public static class QuaternionExtensions
{
    public static (ushort horizontal, ushort vertical) ToClientUShorts(this Quaternion rotation)
    {
        if (rotation.eulerAngles.z != 0f)
        {
            rotation = Quaternion.LookRotation(rotation * Vector3.forward, Vector3.up);
        }

        float outfHorizontal = rotation.eulerAngles.y;
        float outfVertical = -rotation.eulerAngles.x;

        if (outfVertical < -90f)
        {
            outfVertical += 360f;
        }
        else if (outfVertical > 270f)
        {
            outfVertical -= 360f;
        }

        return (ToHorizontal(outfHorizontal), ToVertical(outfVertical));
    }

    private static ushort ToHorizontal(float horizontal)
    {
        const float ToHorizontal = 65535f / 360f;

        horizontal = Mathf.Clamp(0f, 360f, horizontal);

        return (ushort)Mathf.RoundToInt(horizontal * ToHorizontal);
    }

    private static ushort ToVertical(float vertical)
    {
        const float ToVertical = 65535f / 176f;

        vertical = Mathf.Clamp(-88f, 88f, vertical) + 88f;

        return (ushort)Mathf.RoundToInt(vertical * ToVertical);
    }
}
