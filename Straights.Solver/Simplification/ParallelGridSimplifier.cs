// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using Straights.Solver.Data;

/// <summary>
/// Simplifies a grid by first simplifying its columns
/// parallelized over columns,
/// and then simplifying its rows
/// parallelized over rows.
/// </summary>
/// <param name="columnSimplifier">
/// The column simplifier to use (for both rows and columns).
/// </param>
/// <seealso cref="ParallelGridSimplifier"/>
public class ParallelGridSimplifier(ISimplify<SolverColumn> columnSimplifier)
: ISimplify<ISolverGrid>
{
    public ISimplify<SolverColumn> ColumnSimplifier { get; } = columnSimplifier;

    public void Simplify(ISolverGrid item)
    {
        this.ColumnSimplifier.SimplifyManyParallel(item.Columns);
        this.ColumnSimplifier.SimplifyManyParallel(item.Rows);
    }
}
