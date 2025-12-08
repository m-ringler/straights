// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

public static class WhiteFieldDataEnumerable
{
    public static IEnumerable<WhiteFieldData> Solved(this IEnumerable<WhiteFieldData> self)
    {
        return self.Where(f => f.IsSolved);
    }

    public static IEnumerable<WhiteFieldData> Unsolved(this IEnumerable<WhiteFieldData> self)
    {
        return self.Where(f => !f.IsSolved);
    }

    /// <summary>
    /// Gets the sum of the <see cref="WhiteFieldData"/>.Count of the fields.
    /// </summary>
    /// <param name="self">The collection of fields.</param>
    /// <returns>The total count of values in the fields.</returns>
    public static int TotalCount(this IEnumerable<WhiteFieldData> self)
    {
        return self.Sum(x => x.Count);
    }

    public static bool IsSolved(this IEnumerable<WhiteFieldData> self)
    {
        return self.All(x => x.IsSolved);
    }

    /// <summary>
    /// Gets the <see cref="WhiteFieldData.Size"/> of the first field
    /// or 0 if the collection is empty.
    /// </summary>
    /// <param name="self">The collection of fields.</param>
    /// <returns>The grid size according to the first field, or 0 if the collection is empty.</returns>
    public static int GridSize(this IEnumerable<WhiteFieldData> self)
    {
        return self.Select(x => x.Size).FirstOrDefault();
    }

    /// <summary>
    /// Removes the specified numbers from all the fields.
    /// </summary>
    /// <param name="self">The collection of fields.</param>
    /// <param name="numbers">The numbers to remove.</param>
    public static void Remove(this IEnumerable<WhiteFieldData> self, IEnumerable<int> numbers)
    {
        foreach (var field in self)
        {
            field.Remove(numbers);
        }
    }

    /// <summary>
    /// Gets the <see cref="WhiteFieldData.Union" /> of all fields.
    /// </summary>
    /// <param name="self">The collection of fields.</param>
    /// <returns>The union of all fields or null if the collecton is empty.</returns>
    public static WhiteFieldData? Union(
        this IReadOnlyCollection<WhiteFieldData> self)
    {
        if (self.Count == 0)
        {
            return null;
        }

        var all = self.First().Clone();
        foreach (var x in self.Skip(1))
        {
            all = all.Union(x);
        }

        return all;
    }
}
