// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver;

using Straights.Solver.Data;
using Straights.Solver.Simplification;

/// <summary>
/// A recursive trial-and-error solver that uses the specified
/// simplifier.
/// </summary>
/// <param name="gridSimplifier">
/// The simplifier to use.
/// </param>
/// <remarks>
/// Use this solver for grids that do not
/// necessarily have a unique solution, e. g. when
/// generating puzzles.
/// </remarks>.
public sealed class RecursiveTrialAndErrorSolver(
        ISimplify<SolverGrid> gridSimplifier)
    : ISolver, ISolverWithCancellation
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="RecursiveTrialAndErrorSolver"/> class
    /// that uses the default grid simplifier.
    /// </summary>
    public RecursiveTrialAndErrorSolver()
    : this(
        GridSimplifierFactory.BuildIterativeSimplifier(SimplifierStrength.DefaultStrength))
    {
    }

    /// <summary>
    /// Gets the random number generator
    /// used by the current instance.
    /// </summary>
    public required IRandom RandomNumberGenerator { get; init; }

    /// <summary>
    /// Gets the grid simplifier used
    /// by the current instance.
    /// </summary>
    public ISimplify<SolverGrid> GridSimplifier { get; } = gridSimplifier;

    /// <summary>
    /// Gets the maximum number of recursive solving attempts
    /// before the solver gives up.
    /// </summary>
    /// <remarks>
    /// This property can be controlled to terminate otherwise
    /// very long-running solution attempts in fairly
    /// unconstrained grids (e. g. when the solver is used
    /// by the generator).
    /// <para/>
    /// The default value of this property is
    /// <see cref="int.MaxValue"/>.
    /// </remarks>
    public int MaximumNumberOfRecursions { get; init; } = int.MaxValue;

    /// <inheritdoc/>
    public SolverGrid Solve(SolverGrid data)
    {
        return this.Solve(data, CancellationToken.None);
    }

    /// <inheritdoc/>
    public SolverGrid Solve(SolverGrid data, CancellationToken cancellationToken)
    {
        SolverGrid workingData = data.CreateCopy();
        try
        {
            this.GridSimplifier.Simplify(workingData);
        }
        catch (NotSolvableException)
        {
            return workingData;
        }

        cancellationToken.ThrowIfCancellationRequested();
        int remainingRecursions = this.MaximumNumberOfRecursions;
        try
        {
            return this.GuessAndSimplify(
                    workingData,
                    cancellationToken,
                    ref remainingRecursions);
        }
        catch (Exception ex)
        when (ex is SolvingAttemptFailedException or OperationCanceledException)
        {
            return workingData;
        }
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

    private SolverGrid GuessAndSimplify(
        SolverGrid dataIn,
        CancellationToken cancellationToken,
        ref int remainingNumRecurse)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (remainingNumRecurse-- <= 0)
        {
            throw new SolvingAttemptFailedException();
        }

        List<FieldIndex> unsolvedIndices = GetUnsolvedIndices(dataIn);
        var n = unsolvedIndices.Count;
        if (n == 0)
        {
            // No more unsolved index.
            return dataIn;
        }

        int i = this.RandomNumberGenerator.NextInt32(0, n);
        var fieldIndex = unsolvedIndices[i];

        var data = dataIn.CreateCopy();
        var guessValues = GetField(data, fieldIndex).ToArray();
        this.RandomNumberGenerator.Shuffle(guessValues);

        for (int iguess = 0; iguess < guessValues.Length; iguess++)
        {
            var trialData = data.CreateCopy();

            var trialGuess = guessValues[iguess];
            GetField(trialData, fieldIndex).Solve(trialGuess);

            try
            {
                this.GridSimplifier.Simplify(trialData);
                if (trialData.IsSolved)
                {
                    return trialData;
                }

                var result = this.GuessAndSimplify(
                    trialData,
                    cancellationToken,
                    ref remainingNumRecurse);

                if (result.IsSolved)
                {
                    return result;
                }
            }
            catch (NotSolvableException)
            {
                // The current guess has resulted in an unsolvable grid.
            }

            try
            {
                // The current guess has resulted in an unsolvable grid.
                // Remove the value from the field and simplify.
                _ = GetField(data, fieldIndex).Remove(trialGuess);
                this.GridSimplifier.Simplify(data);
            }
            catch (NotSolvableException)
            {
                // The dataIn grid is not solvable.
                // Terminate the loop and return to caller.
                break;
            }

            if (data.IsSolved)
            {
                return data;
            }
        }

        // We failed to solve the grid. Return the unsolved grid.
        return dataIn;
    }

    private sealed class SolvingAttemptFailedException : Exception
    {
    }
}
