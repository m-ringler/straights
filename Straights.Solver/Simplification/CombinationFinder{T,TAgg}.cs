// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

/// <summary>
/// Finds eligible unordered combinations of a certain length
/// among a set of values.
/// </summary>
/// <typeparam name="T">The type of the values.</typeparam>
/// <typeparam name="TAgg">
/// An aggregate type used to compute <paramref name="isEligible"/>
/// for a partial combination.
/// </typeparam>
/// <param name="aggregate">
/// The function used to combine the values of a combination
/// for eligibility testing.
/// </param>
/// <param name="isEligible">
/// The function that determines from the aggregate whether
/// a partial combination is eligible for the result or not.
/// </param>
public sealed class CombinationFinder<T, TAgg>(
    Func<TAgg, T, TAgg> aggregate,
    Func<TAgg, bool> isEligible
)
{
    /// <summary>
    /// Gets all eligible combinations of <paramref name="numberOfItemsToCombine"/> items
    /// among the values.
    /// </summary>
    /// <remarks>
    /// The returned combinations will have the same order
    /// as the <paramref name="values"/>.
    /// </remarks>
    /// <param name="values">The values to combine.</param>
    /// <param name="seed">The seed for the aggregate function.</param>
    /// <param name="numberOfItemsToCombine">The number of items to combine.</param>
    /// <returns>
    /// All eligible combinations as a deferred-evaluation enumerable.
    /// </returns>
    public IEnumerable<IEnumerable<T>> EnumerateEligibleCombinations(
        IReadOnlyCollection<T> values,
        TAgg seed,
        int numberOfItemsToCombine
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(numberOfItemsToCombine, 1);
        return this.EnumerateAllCombinations(
            values,
            values.Count,
            seed,
            numberOfItemsToCombine
        );
    }

    private IEnumerable<IEnumerable<T>> EnumerateAllCombinations(
        IEnumerable<T> values,
        int valuesCount,
        TAgg seed,
        int numberOfItemsToCombine
    )
    {
        if (valuesCount < numberOfItemsToCombine)
        {
            yield break;
        }

        if (numberOfItemsToCombine == 1)
        {
            foreach (var value in values)
            {
                var agg = aggregate(seed, value);
                if (isEligible(agg))
                {
                    yield return [value];
                }
            }
        }

        int i = 0;
        int remainingValuesCount = valuesCount;
        int numberOfFollowingValues = numberOfItemsToCombine - 1;
        foreach (var value in values)
        {
            i++;
            remainingValuesCount--;
            if (remainingValuesCount < numberOfFollowingValues)
            {
                break;
            }

            var agg = aggregate(seed, value);
            if (!isEligible(agg))
            {
                continue;
            }

            var remainingValues = values.Skip(i);
            foreach (
                var followingValuesCombination in this.EnumerateAllCombinations(
                    remainingValues,
                    remainingValuesCount,
                    agg,
                    numberOfFollowingValues
                )
            )
            {
                yield return followingValuesCombination.Prepend(value);
            }
        }
    }
}
