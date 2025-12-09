// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

/// <summary>
/// Provides combinatorics extensions for collections.
/// </summary>
public static class Combinatorics
{
    /// <summary>
    /// Gets all (unordered) combinations of the
    /// <paramref name="items"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="items">The items to combine, will be enumerated once.</param>
    /// <returns>All possible combinations of the items.</returns>
    /// <remarks>
    /// The order of the combinations in the result is unspecified
    /// and may change.
    /// <para/>
    /// If there are duplicate items in the input there will be duplicate
    /// items in the output. The items are not compared by this method.
    /// </remarks>
    public static List<List<T>> GetUnorderedCombinations<T>(this IEnumerable<T> items)
    {
        var queue = new Queue<T>(items);
        return queue.Count == 0 ? [] : GetCombinations(queue);
    }

    private static List<List<T>> GetCombinations<T>(Queue<T> items)
    {
        var result = new List<List<T>>();
        var currentItem = items.Dequeue();
        result.Add([currentItem]);
        int remainingItemsCount = items.Count;
        if (remainingItemsCount != 0)
        {
            var remainingItemsCombinations = GetCombinations(items);
            result.AddRange(remainingItemsCombinations);
            foreach (var c in remainingItemsCombinations)
            {
                result.Add([currentItem, .. c]);
            }
        }

        return result;
    }
}
