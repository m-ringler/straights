// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator;

using Straights.Solver.Builder;
using Straights.Solver.Converter;
using Straights.Solver.Data;

/// <summary>
/// Generates straights grids by attempting to solve
/// empty grids and using the guesses made as
/// hints in the result grid.
/// </summary>
/// <param name="solver">
/// The solver to use.
/// </param>
/// <param name="emptyGridGenerator">
/// The strategy to use to generate empty grids.
/// </param>
public class GridGenerator(
        ISolver solver,
        IEmptyGridGenerator emptyGridGenerator)
    : IGridGenerator
{
    public ISolver Solver { get; } = solver;

    public IEmptyGridGenerator EmptyGridGenerator { get; } = emptyGridGenerator;

    public int MaxNumTries { get; init; } = 10;

    public GridBuilder? GenerateGrid()
    {
        for (int i = 0; i < this.MaxNumTries; i++)
        {
            try
            {
                BuilderField?[][] emptyGrid = this.EmptyGridGenerator.GenerateGrid();
                var solverGrid = this.Solver.Solve(ToSolverGrid(emptyGrid));
                if (solverGrid.IsSolved)
                {
                    return solverGrid.Convert().Builder;
                }
            }
            catch (OperationCanceledException ex) when (ex.Message == "MaxNumRecursions")
            {
                // start over
            }
        }

        return null;
    }

    private static SolverGrid ToSolverGrid(BuilderField?[][] unsolvedGrid)
    {
        // Circumnavigate the validation in unsolvedGrid.Convert()
        return SolverGrid.FromFieldGrid(
            BuilderToSolverGridConverter.ToSolverFields(unsolvedGrid));
    }
}
