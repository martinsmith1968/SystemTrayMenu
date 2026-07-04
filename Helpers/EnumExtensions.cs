using System.Linq;

namespace SystemTrayMenu.Helpers;

using System;
using System.Collections.Generic;

public static class EnumExtensions
{
    public static string GetFlagsText<T>(this T enumValue, params T[] ignoreValues)
        where T : struct, Enum
    {
        var flags = new List<string>();

        var values = Enum.GetValues<T>();
        foreach (var value in values.Where(v => !ignoreValues.Contains(v)))
        {
            if (enumValue.HasFlag(value))
            {
                flags.Add(value.ToString());
            }
        }

        return string.Join(" | ", flags);
    }
}
