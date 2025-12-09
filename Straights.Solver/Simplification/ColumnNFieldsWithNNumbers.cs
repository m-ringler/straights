// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using System;
using Straights.Solver.Data;

/// <summary>
/// A simplifier for <see cref="SolverColumn"/>s that detects
/// groups of N fields with N admissible values
/// (for 2 ≤ N &lt; number of fields in the column),
/// and eliminates these values from the other fields in the
/// column.
/// </summary>
public sealed class ColumnNFieldsWithNNumbers : ISimplify<SolverColumn>
{
    public void Simplify(SolverColumn item)
    {
        var fieldsByCount = new FieldsByCount(item.Fields.Count() - 1);
        foreach (var field in item.Fields)
        {
            fieldsByCount.AddField(field);
        }

        for (int n = 2; n <= fieldsByCount.MaxCount; n++)
        {
            var fieldsWithAtMostNValues =
                fieldsByCount.GetFieldsWithAtMostNValues(n);
            if (fieldsWithAtMostNValues.Count < n)
            {
                continue;
            }

            // Starting with fields with more values we are more likely to fail fast.
            fieldsWithAtMostNValues.Sort((a, b) => b.Count.CompareTo(a.Count));

            // Find combinations of n fields that contain at most n distinct values.
            var combinationFinder = new CombinationFinder<
                WhiteFieldData,
                WhiteFieldData?
            >(
                aggregate: (agg, a) => agg?.Union(a) ?? a,
                isEligible: agg => agg != null && agg.Count <= n
            );

            var combinations = combinationFinder.EnumerateEligibleCombinations(
                fieldsWithAtMostNValues,
                null,
                n
            );

            foreach (var fieldGroup in combinations)
            {
                // Perform the lazy enumeration of fieldGroup
                // and store the result as a set.
                var theFields = fieldGroup.ToHashSet();
                var theValues = theFields.Union() ?? Enumerable.Empty<int>();
                item.Fields.Where(x => !theFields.Contains(x))
                    .Remove(theValues);
            }
        }
    }

    private sealed class FieldsByCount(int maxCount)
    {
        private readonly Dictionary<int, List<WhiteFieldData>> fieldsByCount =
            new();

        public int MaxCount { get; } = maxCount;

        public void AddField(WhiteFieldData f)
        {
            // solved fields are handled by another rule
            if (f.Count == 1)
            {
                return;
            }

            for (int n = f.Count; n <= this.MaxCount; n++)
            {
                this.AddField(n, f);
            }
        }

        public List<WhiteFieldData> GetFieldsWithAtMostNValues(int n)
        {
            if (this.fieldsByCount.TryGetValue(n, out var result))
            {
                return result;
            }

            return [];
        }

        private void AddField(int n, WhiteFieldData f)
        {
            this.GetOrCreateList(n).Add(f);
        }

        private List<WhiteFieldData> GetOrCreateList(int n)
        {
            if (this.fieldsByCount.TryGetValue(n, out var result))
            {
                return result;
            }

            result = new();
            this.fieldsByCount[n] = result;
            return result;
        }
    }
}
