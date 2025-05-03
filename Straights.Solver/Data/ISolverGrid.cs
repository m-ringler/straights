// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

using Straights.Solver.Simplification;

public interface ISolverGrid
    : IGetSnapshot<int>
{
    ImmutableArray<SolverColumn> Columns { get; }

    ImmutableArray<SolverColumn> Rows { get; }

    /// <summary>
    /// Gets the total number of admissible values in all fields.
    /// </summary>
    /// <returns>The total number of admissible values.</returns>
    int TotalCount()
    {
        return this.Rows.Sum(x => x.TotalCount());
    }

    bool IsSolved()
    {
        return this.Rows.All(f => f.IsSolved());
    }
}
