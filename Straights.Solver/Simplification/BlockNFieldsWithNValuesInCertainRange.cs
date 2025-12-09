// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using Straights.Solver.Data;

/// <summary>
/// Simplifies blocks by looking for combinations
/// of N values that must occur in the block,
/// and that occur in exactly N fields. On success,
/// all other values are removed from those fields.
/// </summary>
public sealed class BlockNFieldsWithNValuesInCertainRange
    : ISimplify<SolverBlock>
{
    public void Simplify(SolverBlock item)
    {
        var certainRange = item.FindCertainRange();
        if (certainRange.Count == 0)
        {
            return;
        }

        var certainRangeNumbers = certainRange.ToHashSet();
        foreach (var solvedField in item.Solved())
        {
            _ = certainRangeNumbers.Remove(solvedField.Min);
        }

        if (certainRangeNumbers.Count == 0)
        {
            return;
        }

        foreach (
            var combination in certainRangeNumbers.GetUnorderedCombinations()
        )
        {
            var fieldsWithTheseNumbers = item
                .Fields.Where(f => combination.Any(f.Contains))
                .Take(combination.Count + 1)
                .ToArray();

            if (fieldsWithTheseNumbers.Length == combination.Count)
            {
                var allValues = Enumerable.Range(
                    1,
                    fieldsWithTheseNumbers[0].Size
                );

                var remove = allValues.Except(combination).ToArray();

                fieldsWithTheseNumbers.Remove(remove);
            }
        }
    }
}
