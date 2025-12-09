// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using Straights.Solver.Data;

/// <summary>
/// Simplifies a grid by first simplifying its columns
/// and then simplifying its rows.
/// </summary>
/// <param name="columnSimplifier">
/// The column simplifier to use (for both rows and columns).
/// </param>
/// <seealso cref="ParallelGridSimplifier"/>
public class GridSimplifier(ISimplify<SolverColumn> columnSimplifier)
    : ISimplify<ISolverGrid>
{
    public ISimplify<SolverColumn> ColumnSimplifier { get; } = columnSimplifier;

    public void Simplify(ISolverGrid item)
    {
        this.ColumnSimplifier.SimplifyMany(item.Columns);
        this.ColumnSimplifier.SimplifyMany(item.Rows);
    }
}
