// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using Straights.Solver.Data;

/// <summary>
/// A simplifier for <see cref="SolverBlock"/>s that detects
/// disjunct subsets of admissible values and eliminates those
/// subsets whose element count is less than the length of the block.
/// </summary>
public sealed class BlockDisjunctSubsets : ISimplify<SolverBlock>
{
    public void Simplify(SolverBlock item)
    {
        var range = item.GetMaxRange();
        var allValues = item.Union()!;
        var splitters = range.Except(allValues);

        int currentMin = range.Min;
        List<int>? tooShortSplits = null;
        using var e = splitters.Append(range.Max + 1).GetEnumerator();
        while (e.MoveNext())
        {
            var splitter = e.Current;
            var splitRange = new IntRun(currentMin, splitter - 1);

            if (!splitRange.IsEmpty && splitRange.Count < item.Count)
            {
                (tooShortSplits ??= []).AddRange(splitRange);
            }

            currentMin = splitter + 1;
        }

        if (tooShortSplits != null)
        {
            item.Remove(tooShortSplits);
        }
    }
}
