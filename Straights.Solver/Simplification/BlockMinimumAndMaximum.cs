// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using Straights.Solver.Data;

/// <summary>
/// Determines the maximum possible range of a
/// <see cref="SolverBlock"/> from the minimum of the maximum
/// and the maximum of the minimum admissible values of the fields,
/// and eliminates all values outside that range.
/// </summary>
/// <seealso cref="BlockAnalyzer.GetMaxRange"/>
public sealed class BlockMinimumAndMaximum
    : ISimplify<SolverBlock>
{
    public void Simplify(SolverBlock item)
    {
        var (newMin, newMax) = item.GetMaxRange();
        var removeRange = new IntRun(newMax + 1, item.GridSize());
        if (!removeRange.IsEmpty)
        {
            item.Remove(removeRange);
        }

        removeRange = new IntRun(1, newMin - 1);
        if (!removeRange.IsEmpty)
        {
            item.Remove(removeRange);
        }
    }
}
