namespace BananaPlugin.Extensions;

using System;
using UnityEngine;

/// <summary>
/// Consists of <see cref="Quaternion"/> extensions used for different use cases throughout the assembly.
/// </summary>
public static class QuaternionExtensions
{
    /// <summary>
    /// Converts a quaternion rotation into a pair of unsigned shorts (horizontal, vertical) for client representation.
    /// </summary>
    /// <param name="rotation">The quaternion rotation to convert.</param>
    /// <returns>A tuple containing the horizontal and vertical angles as unsigned shorts.</returns>
    public static (ushort horizontal, ushort vertical) ToClientUShorts(this Quaternion rotation)
    {
        const float ToHorizontal = ushort.MaxValue / 360f;
        const float ToVertical = ushort.MaxValue / 176f;

        float fixVertical = -rotation.eulerAngles.x;

        if (fixVertical < -90f)
        {
            fixVertical += 360f;
        }
        else if (fixVertical > 270f)
        {
            fixVertical -= 360f;
        }

        float horizontal = Mathf.Clamp(rotation.eulerAngles.y, 0f, 360f);
        float vertical = Mathf.Clamp(fixVertical, -88f, 88f) + 88f;

        return ((ushort)Math.Round(horizontal * ToHorizontal), (ushort)Math.Round(vertical * ToVertical));
    }
}
