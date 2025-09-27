// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using Straights.Solver.Data;

using static GridSimplifierFactory;

/// <summary>
/// Generates hints for solving a partially solved grid.
/// </summary>
/// <param name="maxStrength">The maximum simplifier strength to use.</param>
public sealed class HintGenerator(SimplifierStrength maxStrength)
{
    private readonly SimplifierStrength maxStrength = maxStrength;

    /// <summary>
    /// Generates the next hint for solving the grid.
    /// </summary>
    /// <param name="grid">The grid to generate hints for.</param>
    /// <returns>A hint containing the field that can be simplified and the simplifier to use.</returns>
    /// <exception cref="NotSolvableException">Thrown when no hint can be found within the maximum strength.</exception>
    public Hint GenerateHint(SolverGrid grid)
    {
        var originalGrid = grid.CreateCopy();
        var provider = new SimplifierProvider();

        var changeDetector = new StateComparer<int>([
            grid,
            ..grid.Grid.Fields.OfType<SolverField.WhiteField>(),
        ]);

        while (provider.CurrentStrength <= this.maxStrength)
        {
            var columnSimplifiers = provider.GetNextSimplifiers();
            foreach (var simplifier in columnSimplifiers)
            {
                foreach (var (data, isRow) in this.GetRowsAndColumns(grid))
                {
                    simplifier.Simplify(data);
                    bool hasChanged = changeDetector.HasChanged(grid);
                    if (hasChanged)
                    {
                        var index = GetChangedFieldIndex(grid.Grid, changeDetector);
                        int removedNumber = GetRemovedNumber(
                            originalGrid.Grid.GetField(index),
                            grid.Grid.GetField(index));

                        return new Hint(
                            removedNumber,
                            index,
                            provider.GetType(simplifier),
                            isRow);
                    }
                }
            }
        }

        throw new NotSolvableException($"No hints found within maximum strength {this.maxStrength}");
    }

    private static int GetRemovedNumber(SolverField original, SolverField simplified)
    {
        return original.GetWhiteFieldData()!
            .Except(simplified.GetWhiteFieldData()!)
            .FirstOrDefault();
    }

    private static FieldIndex GetChangedFieldIndex(Grid<SolverField> grid, StateComparer<int> changeDetector)
    {
        var locations =
            from idx in grid.AllFieldIndices()
            let f = grid.GetField(idx)
            where f is SolverField.WhiteField w && changeDetector.HasChanged(w)
            select idx;

        return locations.First();
    }

    private IEnumerable<(SolverColumn Data, bool IsRow)> GetRowsAndColumns(SolverGrid grid)
    {
        var rows = from r in grid.Rows
                   select (r, isRow: true);

        var columns = from c in grid.Columns
                      select (c, isRow: false);

        return rows.Concat(columns);
    }

    private class SimplifierProvider
    {
        private readonly HashSet<Type> alreadyReturnedSimplifiers = [];
        private readonly Dictionary<ISimplify<SolverColumn>, Type> types = [];

        public SimplifierStrength CurrentStrength { get; private set; } = new(0);

        public Type GetType(ISimplify<SolverColumn> simplifier) => this.types[simplifier];

        public IEnumerable<ISimplify<SolverColumn>> GetNextSimplifiers()
        {
            var all = this.GetAllSimplifiersForCurrentStrength();

            foreach (var (simplifier, type) in all)
            {
                if (this.alreadyReturnedSimplifiers.Add(type))
                {
                    this.types[simplifier] = type;
                    yield return simplifier;
                }
            }

            this.CurrentStrength = new(this.CurrentStrength.Value + 1);
        }

        private IEnumerable<(ISimplify<SolverColumn> Simplifier, Type Type)> GetAllSimplifiersForCurrentStrength()
        {
            var blockSimplifiers =
                from bs in GetBlockSimplifiers(this.CurrentStrength)
                let bscs = (ISimplify<SolverColumn>)bs.AsAggregateSimplifier()
                select (bscs, bs.GetType());

            var columnSimplifiers =
                from cs in GetColumnSimplifiers(this.CurrentStrength, [])
                select (cs, cs.GetType());

            var all = columnSimplifiers.Concat(blockSimplifiers);
            return all;
        }
    }
}