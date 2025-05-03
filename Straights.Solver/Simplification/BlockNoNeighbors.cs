// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using Straights.Solver.Data;

/// <summary>
/// A simplifier for <see cref="SolverBlock"/>s that detects
/// admissible values that have no neighbors (plus or minus one)
/// in other cells, and eliminates them.
/// </summary>
public sealed class BlockNoNeighbors : ISimplify<SolverBlock>
{
    public void Simplify(SolverBlock item)
    {
        if (item.Count < 2)
        {
            return;
        }

        foreach (var field in item.Fields)
        {
            var otherFields = item.Fields.Where(f => f != field).ToList();
            var otherFieldsValues = otherFields.Union()!;
            foreach (var value in field.Clone())
            {
                if (!otherFieldsValues.Contains(value + 1) && !otherFieldsValues.Contains(value - 1))
                {
                    _ = field.Remove(value);
                }
            }
        }
    }
}
