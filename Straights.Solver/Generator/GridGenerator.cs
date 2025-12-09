// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator;

using Straights.Solver.Builder;
using Straights.Solver.Converter;
using Straights.Solver.Data;

/// <summary>
/// Generates straights grids by attempting to solve
/// empty grids.
/// </summary>
/// <param name="solver">
/// The solver to use.
/// </param>
/// <param name="emptyGridGenerator">
/// The strategy to use to generate empty grids.
/// </param>
public class GridGenerator(
    ISolver solver,
    IEmptyGridGenerator emptyGridGenerator
) : IGridGenerator
{
    public ISolver Solver { get; } = solver;

    public IEmptyGridGenerator EmptyGridGenerator { get; } = emptyGridGenerator;

    public int MaximumNumberOfAttempts { get; init; } = 10;

    public GridBuilder? GenerateGrid()
    {
        for (int i = 0; i < this.MaximumNumberOfAttempts; i++)
        {
            BuilderField?[][] emptyGrid =
                this.EmptyGridGenerator.GenerateGrid();
            var solverGrid = this.Solver.Solve(ToSolverGrid(emptyGrid));
            if (solverGrid.IsSolved)
            {
                return solverGrid.Convert().Builder;
            }
        }

        return null;
    }

    internal static SolverGrid ToSolverGrid(BuilderField?[][] unsolvedGrid)
    {
        // Circumnavigate the validation in unsolvedGrid.Convert()
        return SolverGrid.FromFieldGrid(
            BuilderToSolverGridConverter.ToSolverFields(unsolvedGrid)
        );
    }
}
