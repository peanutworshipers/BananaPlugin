namespace BananaPlugin.API.Utils;

using System;

/// <summary>
/// A utility class for retreiving the current time.
/// </summary>
public static class TimeHelper
{
    /// <summary>
    /// Gets the current time in US EST form.
    /// </summary>
    public static DateTime EstNow
    {
        get
        {
            DateTime x = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(-5d), DateTimeKind.Unspecified);

            if (x.IsDaylightSavingTime())
            {
                x = x.AddHours(1d);
            }

            return x;
        }
    }

    /// <summary>
    /// Checks if its the specified day of week in US EST form.
    /// </summary>
    /// <param name="day">The day to check for.</param>
    /// <returns>A value indicating whether it is the current day in US EST form.</returns>
    public static bool IsDayOfWeek(this DayOfWeek day) => EstNow.DayOfWeek == day;
}