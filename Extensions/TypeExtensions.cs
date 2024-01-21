// thank you stack overflow :)
namespace BananaPlugin.Extensions;

using System;
using System.Linq;

/// <summary>
/// Consists of type extensions used throughout the assembly.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Returns the type name. If this is a generic type, appends
    /// the list of generic type arguments between angle brackets.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>System.String.</returns>
    public static string GetGenericName(this Type type)
    {
        if (type.IsGenericType)
        {
            string genericArguments = type.GetGenericArguments()
                                .Select(GetGenericName)
                                .Aggregate((x1, x2) => $"{x1}, {x2}");

            return $"{type.Name.Substring(0, type.Name.IndexOf("`", StringComparison.Ordinal))}"
                 + $"<{genericArguments}>";
        }

        return type.Name;
    }
}
