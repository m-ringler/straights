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
    ISimplify<SolverGrid> gridSimplifier
) : ISolver, ISolverWithCancellation
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="RecursiveTrialAndErrorSolver"/> class
    /// that uses the default grid simplifier.
    /// </summary>
    public RecursiveTrialAndErrorSolver()
        : this(
            GridSimplifierFactory.BuildIterativeSimplifier(
                SimplifierStrength.DefaultStrength
            )
        ) { }

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
    public SolverGrid Solve(
        SolverGrid data,
        CancellationToken cancellationToken
    )
    {
        var pool = new SolverGridPool();

        SolverGrid workingData = pool.GetCopyOf(data);
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
                pool,
                cancellationToken,
                ref remainingRecursions
            );
        }
        catch (Exception ex)
            when (ex
                    is SolvingAttemptFailedException
                        or OperationCanceledException
            )
        {
            return workingData;
        }
    }

    private static WhiteFieldData? GetField(SolverGrid grid, FieldIndex index)
    {
        return grid.Grid.GetField(index).GetWhiteFieldData();
    }

    private static List<FieldIndex> GetUnsolvedIndices(SolverGrid data)
    {
        var result =
            from index in data.Grid.AllFieldIndices()
            where GetField(data, index)?.IsSolved == false
            select index;

        return [.. result];
    }

    private SolverGrid GuessAndSimplify(
        SolverGrid dataIn,
        SolverGridPool pool,
        CancellationToken cancellationToken,
        ref int remainingNumRecurse
    )
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

        var data = pool.GetCopyOf(dataIn);

        var fieldData = GetField(data, fieldIndex);
        if (fieldData is null)
        {
            var msg = $"""
                The field at index {fieldIndex} is
                unsolved in {nameof(
                    dataIn
                )}, so we expect it to be a white field.
                However, it is not a white field in the {nameof(data)} copy:
                dataIn: {dataIn.Grid.GetField(fieldIndex)}.
                data: {data.Grid.GetField(fieldIndex)}.
                """;
            throw new InvalidOperationException(msg);
        }

        var guessValues = fieldData.ToArray();
        this.RandomNumberGenerator.Shuffle(guessValues);

        for (int iguess = 0; iguess < guessValues.Length; iguess++)
        {
            var trialData = pool.GetCopyOf(data);

            var trialGuess = guessValues[iguess];
            var trialFieldData = GetField(trialData, fieldIndex);
            if (trialFieldData is null)
            {
                var msg = $"""
                    The field at index {fieldIndex} is
                    unsolved in {nameof(
                        dataIn
                    )}, so we expect it to be a white field.
                    However, it is not a white field in the {nameof(trialData)} copy:
                    dataIn: {dataIn.Grid.GetField(fieldIndex)}.
                    trialData: {trialData.Grid.GetField(fieldIndex)}.
                    """;
                throw new InvalidOperationException(msg);
            }

            trialFieldData.Solve(trialGuess);

            try
            {
                this.GridSimplifier.Simplify(trialData);
                if (trialData.IsSolved)
                {
                    return trialData;
                }

                var result = this.GuessAndSimplify(
                    trialData,
                    pool,
                    cancellationToken,
                    ref remainingNumRecurse
                );

                if (result.IsSolved)
                {
                    return result;
                }
            }
            catch (NotSolvableException)
            {
                // The current guess has resulted in an unsolvable grid.
            }

            pool.Release(trialData);

            try
            {
                // The current guess has resulted in an unsolvable grid.
                // Remove the value from the field and simplify.
                var fieldData1 = GetField(data, fieldIndex);
                if (fieldData1 is null)
                {
                    var msg = $"""
                The field at index {fieldIndex} is
                unsolved in {nameof(
                    dataIn
                )}, so we expect it to be a white field.
                However, it is not a white field in the {nameof(data)} copy:
                dataIn: {dataIn.Grid.GetField(fieldIndex)}.
                data: {data.Grid.GetField(fieldIndex)}.
                """;
                    throw new InvalidOperationException(msg);
                }

                _ = fieldData1.Remove(trialGuess);
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

        pool.Release(data);

        // We failed to solve the grid. Return the unsolved grid.
        return dataIn;
    }

    private sealed class SolvingAttemptFailedException : Exception { }
}
