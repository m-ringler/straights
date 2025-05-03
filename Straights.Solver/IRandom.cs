// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver;

/// <summary>
/// Represents a random number generator interface.
/// </summary>
public interface IRandom
{
    /// <summary>
    /// Returns a random integer within the specified range.
    /// </summary>
    /// <param name="minInclusive">The inclusive lower bound of the range.</param>
    /// <param name="maxExclusive">The exclusive upper bound of the range.</param>
    /// <returns>A random integer within the specified range.</returns>
    int NextInt32(int minInclusive, int maxExclusive);

    /// <summary>
    /// Returns a random double between 0.0 (inclusive) and 1.0 (exclusive).
    /// </summary>
    /// <returns>A random double.</returns>
    double NextDouble();

    /// <summary>
    /// Shuffles the elements of the specified list in place.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to shuffle.</param>
    void Shuffle<T>(IList<T> list);
}
