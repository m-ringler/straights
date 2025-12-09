// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator.Empty;

using System.Diagnostics;
using Straights.Solver.Builder;

internal sealed class HorizontallyAndVerticallySymmetricEmptyGridGenerator(
    GridParameters gridParameters
) : IEmptyGridGenerator
{
    public required IRandom RandomNumberGenerator { get; init; }

    public GridParameters GridParameters { get; } = Validate(gridParameters);

    public BuilderField?[][] GenerateGrid()
    {
        return EmptyGridGenerator.GenerateGrid(
            this.GridParameters,
            this.GenerateFieldIndices(5)
        );
    }

    private static GridParameters Validate(GridParameters gridParameters)
    {
        if (
            gridParameters.Size % 2 == 0
            && gridParameters.TotalNumberOfBlackFields % 4 != 0
        )
        {
            throw new ArgumentException(
                paramName: nameof(gridParameters),
                message: $"The total number of black fields must be a multiple of 4 for size {gridParameters.Size}."
            );
        }

        return gridParameters;
    }

    private IEnumerable<FieldIndex> GenerateFieldIndices(int maxRecurse)
    {
        int n = this.GridParameters.TotalNumberOfBlackFields;
        int size = this.GridParameters.Size;

        var index1D = Enumerable.Range(0, (size + 1) / 2);
        var allIndices =
            from y in index1D
            from x in index1D
            select new FieldIndex(x, y);

        List<FieldIndex> result = [];
        var all = allIndices.ToList();

        // Odd grid size ...
        int center = -1;
        if (size % 2 == 1)
        {
            // Odd total number of black fields
            // => add center cell
            if (n % 2 == 1)
            {
                result.Add(all[^1]);
            }

            center = size / 2;
            all.RemoveAt(all.Count - 1);
        }

        this.RandomNumberGenerator.Shuffle(all);
        foreach (var index in all)
        {
            bool isCenterX = index.X == center;
            bool isCenterY = index.Y == center;

            if (result.Count == n - 3 || result.Count == n - 1)
            {
                Debug.Fail("We should never get here.");
            }

            if (result.Count == n - 4)
            {
                if (isCenterX || isCenterY)
                {
                    continue;
                }
            }
            else if (result.Count == n - 2)
            {
                if (!(isCenterX ^ isCenterY))
                {
                    continue;
                }
            }
            else if (result.Count == n)
            {
                break;
            }

            var (x, y) = index;
            int xmirror = size - 1 - x;
            int ymirror = size - 1 - y;
            int[] xx = x == xmirror ? [x] : [x, xmirror];
            int[] yy = y == ymirror ? [y] : [y, ymirror];
            foreach (var xi in xx)
            {
                foreach (var yi in yy)
                {
                    result.Add(new FieldIndex(xi, yi));
                }
            }
        }

        if (result.Count != n)
        {
            return maxRecurse > 0
                ? this.GenerateFieldIndices(maxRecurse - 1)
                : throw new InvalidOperationException(
                    "Failed to build a grid for parameters "
                        + this.GridParameters
                );
        }

        this.RandomNumberGenerator.Shuffle(result);
        return result;
    }
}
