// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using Straights.Solver.Data;

/// <summary>
/// Methods to analyze data in a <see cref="SolverBlock"/>.
/// </summary>
public static class BlockAnalyzer
{
    public static IntRun FindCertainRange(this SolverBlock b)
    {
        var (min, max) = b.GetMaxRange();
        var fullRange = new IntRun(1, b.GridSize());
        var lowerRange = new IntRun(min, min + b.Count - 1);
        var upperRange = new IntRun(max - b.Count + 1, max);
        var result = lowerRange.Intersect(upperRange).Intersect(fullRange);
        return result;
    }

    public static IntRun GetMaxRange(this SolverBlock item)
    {
        if (item.Count == 0)
        {
            return new IntRun(0, -1);
        }

        var maxOfMins = item.Max(f => f.Min);
        var minOfMaxs = item.Min(f => f.Max);

        var min = maxOfMins - item.Count + 1;
        var max = minOfMaxs + item.Count - 1;

        var allValues = item.Union()!;
        var allValueRange = new IntRun(allValues.Min, allValues.Max);

        return new IntRun(min, max).Intersect(allValueRange);
    }

    public static IEnumerable<IntRun> GetAllRanges(this SolverBlock item)
    {
        var maxRange = item.GetMaxRange();
        int n = item.Count;
        for (
            IntRun range = new(maxRange.Min, maxRange.Min + n - 1);
            range.Max <= maxRange.Max;
            range = new(range.Min + 1, range.Max + 1)
        )
        {
            yield return range;
        }
    }
}
