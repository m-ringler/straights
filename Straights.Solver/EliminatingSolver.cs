// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver;

using Straights.Solver.Data;
using Straights.Solver.Simplification;

/// <summary>
/// A solver that solves straights puzzles by eliminating
/// possible values.
/// </summary>
/// <param name="gridSimplifier">
/// The simplifier to use.
/// </param>
/// <remarks>
/// Use this solver for grids that have a unique solution
/// and that cannot be solved with just a simplifier.
/// </remarks>
public sealed class EliminatingSolver(
        ISimplify<SolverGrid> gridSimplifier)
    : ISolver
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EliminatingSolver"/> class
    /// that uses the default simplifier of the <see cref="GridSimplifierFactory" />.
    /// </summary>
    public EliminatingSolver()
    : this(GridSimplifierFactory.BuildIterativeSimplifier(SimplifierStrength.DefaultStrength))
    {
    }

    public ISimplify<SolverGrid> GridSimplifier { get; } = gridSimplifier;

    /// <summary>
    /// Attempts to solve the specified grid.
    /// </summary>
    /// <param name="data">The data to solve, will not be modified.</param>
    /// <returns>
    /// The processed data, <see cref="SolverGrid.IsSolved"/> indicates whether
    /// solving was successful.
    /// </returns>
    public SolverGrid Solve(SolverGrid data)
    {
        var workingData = this.GridSimplifier.ToSolver().Solve(data);
        if (workingData.IsSolved)
        {
            return workingData;
        }

        var unsolvedFieldsIndices = GetUnsolvedIndices(workingData);

        var guessFieldsIndices = new Queue<FieldIndex>(unsolvedFieldsIndices);
        while (guessFieldsIndices.TryDequeue(out FieldIndex guessFieldIndex))
        {
            var currentGuessField = GetField(workingData, guessFieldIndex);
            if (currentGuessField.IsSolved)
            {
                continue;
            }

            this.GuessAndSimplify(
                guessFieldIndex,
                new Queue<int>(currentGuessField),
                ref workingData);

            if (workingData.IsSolved)
            {
                return workingData;
            }
        }

        return workingData;
    }

    private static WhiteFieldData GetField(SolverGrid grid, FieldIndex index)
    {
        return grid.Grid.GetField(index).GetWhiteFieldData()!;
    }

    private static List<FieldIndex> GetUnsolvedIndices(SolverGrid data)
    {
        var result = from index in data.Grid.AllFieldIndices()
                     where GetField(data, index)?.IsSolved == false
                     select index;

        return [.. result];
    }

    private void GuessAndSimplify(
        FieldIndex fieldIndex,
        Queue<int> guessValues,
        ref SolverGrid data)
    {
        while (guessValues.TryDequeue(out int currentGuess))
        {
            var backup = data.CreateCopy();
            GetField(data, fieldIndex).Solve(currentGuess);
            try
            {
                this.GridSimplifier.Simplify(data);
            }
            catch (NotSolvableException)
            {
                // The current guess has resulted in an unsolvable grid.
                // Restore the backup and remove the value from the field.
                data = backup;
                _ = GetField(data, fieldIndex).Remove(currentGuess);

                // Try to simplify-solve.
                this.GridSimplifier.Simplify(data);
                if (data.IsSolved)
                {
                    return;
                }
            }

            // The grid is still not solved, restore the backup (if we have not already
            // done so in the exception handler) and continue with the next guess.
            data = backup;
        }
    }
}