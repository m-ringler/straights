// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

using System.Collections.Immutable;

using Straights.Solver.Simplification;

public record class SolverGrid : ISolverGrid
{
    public required Grid<SolverField> Grid { get; init; }

    public required ImmutableArray<SolverColumn> Columns { get; init; }

    public required ImmutableArray<SolverColumn> Rows { get; init; }

    public int TotalCount => ((ISolverGrid)this).TotalCount();

    public bool IsSolved => ((ISolverGrid)this).IsSolved();

    int IGetSnapshot<int>.GetSnapshot()
    {
        return this.TotalCount;
    }

    /// <summary>
    /// Creates a deep copy of the current instance.
    /// </summary>
    /// <returns>A new solver grid, completely independent of the current instance.</returns>
    public SolverGrid CreateCopy()
    {
        var clonedFields = from r in this.Grid.GetRows()
                           from field in r
                           select field.Clone();
        var clonedGrid = new Grid<SolverField>([.. clonedFields]);
        return FromFieldGrid(clonedGrid);
    }

    public static SolverGrid FromFieldGrid(Grid<SolverField> grid)
    {
        var builder = new SolverColumnBuilder();
        var columns = builder.CreateMany(grid.GetColumns());
        var rows = builder.CreateMany(grid.GetRows());

        return new SolverGrid
        {
            Grid = grid,
            Columns = columns,
            Rows = rows,
        };
    }
}
