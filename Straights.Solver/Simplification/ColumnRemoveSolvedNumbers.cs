// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using Straights.Solver.Data;

/// <summary>
/// A simplifier, that finds the solved fields in a <see cref="SolverColumn"/>
/// and eliminates their values from the unsolved fields in the column.
/// </summary>
public sealed class ColumnRemoveSolvedNumbers : ISimplify<SolverColumn>
{
    public void Simplify(SolverColumn item)
    {
        var solvedFields = item.Solved().ToArray();
        var allFields = item.Fields;

        foreach (var field in solvedFields)
        {
            var solvedNumber = field.Min;
            foreach (var otherField in allFields.Where(f => f != field))
            {
                _ = otherField.Remove(solvedNumber);
            }
        }
    }
}
