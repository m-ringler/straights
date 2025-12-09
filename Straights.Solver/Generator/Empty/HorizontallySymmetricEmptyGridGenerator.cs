// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator.Empty;

using Straights.Solver.Builder;

internal sealed class HorizontallySymmetricEmptyGridGenerator(
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
            && gridParameters.TotalNumberOfBlackFields % 2 != 0
        )
        {
            throw new ArgumentException(
                paramName: nameof(gridParameters),
                message: $"The total number of black fields must be even for size {gridParameters.Size}."
            );
        }

        return gridParameters;
    }

    private IEnumerable<FieldIndex> GenerateFieldIndices(int maxRecurse)
    {
        int n = this.GridParameters.TotalNumberOfBlackFields;
        int size = this.GridParameters.Size;

        var index1D = Enumerable.Range(0, size);
        var allIndices =
            from y in index1D
            from x in Enumerable.Range(0, (size + 1) / 2)
            select new FieldIndex(x, y);

        var all = allIndices.ToList();
        this.RandomNumberGenerator.Shuffle(all);

        List<FieldIndex> result = [];
        int centerCol = size % 2 == 0 ? -1 : size / 2;
        foreach (var index in all)
        {
            bool isCenter = index.X == centerCol;

            if (result.Count == n - 2)
            {
                if (isCenter)
                {
                    continue;
                }
            }
            else if (result.Count == n - 1)
            {
                if (!isCenter)
                {
                    continue;
                }
            }
            else if (result.Count == n)
            {
                break;
            }

            result.Add(index);

            if (!isCenter)
            {
                result.Add(new FieldIndex(size - 1 - index.X, index.Y));
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
