// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator;

using Straights.Solver.Builder;
using Straights.Solver.Data;

/// <summary>
/// Adjusts the difficulty of a grid by adding or removing white numbers.
/// </summary>
/// <remarks>
/// The difficulty-adjusted grid will be solvable with the provided solver.
/// If you remove any of the remaining white numbers,
/// the grid will no longer be solvable with the <paramref name="solver"/>.
/// </remarks>
/// <param name="solver">
/// The solver to use for checking whether a grid is (still) solvable.
/// </param>
public class DifficultyAdjuster(ISolver solver)
{
    public required IRandom RandomNumberGenerator { get; init; }

    public ISolver Solver { get; } = solver;

    public GridBuilder AdjustDifficulty(GridBuilder grid)
    {
        var solvableGrid = this.AddWhiteNumbersIfNecessary(grid);
        return this.RemoveUnnecessaryWhiteNumbers(solvableGrid);
    }

    public IGridGenerator Decorate(IGridGenerator gridGenerator)
    {
        return new GridGeneratorDecorator(gridGenerator, this);
    }

    private GridBuilder AddWhiteNumbersIfNecessary(GridBuilder grid)
    {
        var solverGrid = grid.Convert().SolverGrid;
        if (this.CanSolve(solverGrid))
        {
            return grid;
        }

        solverGrid = new EliminatingSolver().Solve(solverGrid);
        if (!solverGrid.IsSolved)
        {
            throw new ArgumentException("Cannot solve the grid.");
        }

        return solverGrid.Convert().Builder;
    }

    private GridBuilder RemoveUnnecessaryWhiteNumbers(GridBuilder builder)
    {
        var result = builder.GetFields().Convert().Builder;
        var whiteFields =
            from row in result.GetFields()
            from field in row
            where field != null && field.IsWhite && field.Value != null
            select field!;

        var candidates = whiteFields.ToArray();
        this.RandomNumberGenerator.Shuffle(candidates);
        foreach (var candidate in candidates)
        {
            result.Clear(candidate!.Location);
            var grid = result.Convert().SolverGrid;
            var solvedGrid = this.Solver.Solve(grid);
            if (!solvedGrid.IsSolved)
            {
                result.Add(candidate);
            }
        }

        return result;
    }

    private bool CanSolve(SolverGrid grid)
    {
        return this.Solver.Solve(grid).IsSolved;
    }

    private sealed class GridGeneratorDecorator(
        IGridGenerator core,
        DifficultyAdjuster hintRemover
    ) : IGridGenerator
    {
        public IGridGenerator Core { get; } = core;

        public DifficultyAdjuster HintRemover { get; } = hintRemover;

        public GridBuilder? GenerateGrid()
        {
            var result = this.Core.GenerateGrid();
            if (result != null)
            {
                result = this.HintRemover.AdjustDifficulty(result);
            }

            return result;
        }
    }
}
