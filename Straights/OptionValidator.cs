// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.Collections.Generic;
using System.CommandLine;

internal static class OptionValidator
{
    public static IEnumerable<string> RequireMin(
        this Option option, int value, int minInclusive)
    {
        if (value < minInclusive)
        {
            yield return
                $"{option.Name} must be greater than or equal to {minInclusive}.";
        }
    }

    public static IEnumerable<string> RequireMax(
        this Option option,
        int value,
        int maxInclusive,
        string? when = null)
    {
        if (value > maxInclusive)
        {
            yield return
                $"{option.Name} must be less than or equal to {maxInclusive}{when}.";
        }
    }

    public static IEnumerable<string> RequireMinMax(
        this Option option,
        int value,
        (int MinInclusive, int MaxInclusive) limits,
        string? when = null)
    {
        return
        [
            .. RequireMin(option, value, limits.MinInclusive),
            .. RequireMax(option, value, limits.MaxInclusive, when)
        ];
    }
}
