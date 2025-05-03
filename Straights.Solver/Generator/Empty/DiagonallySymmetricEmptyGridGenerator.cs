// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator.Empty;

using Straights.Solver.Builder;

internal sealed class DiagonallySymmetricEmptyGridGenerator(GridParameters gridParameters)
    : IEmptyGridGenerator
{
    public required IRandom RandomNumberGenerator { get; init; }

    public GridParameters GridParameters { get; } = gridParameters;

    public BuilderField?[][] GenerateGrid()
    {
        return EmptyGridGenerator.GenerateGrid(
            this.GridParameters,
            this.GenerateFieldIndices(5));
    }

#if false
    private static GridParameters Validate(GridParameters gridParameters)
    {
        if (gridParameters.Size % 2 == 0 && gridParameters.TotalNumberOfBlackFields % 2 != 0)
        {
            throw new ArgumentException(
                paramName: nameof(gridParameters),
                message: $"The total number of black fields must be even for size {gridParameters.Size}");
        }

        return gridParameters;
    }
#endif

    private IEnumerable<FieldIndex> GenerateFieldIndices(int maxRecurse)
    {
        int n = this.GridParameters.TotalNumberOfBlackFields;
        int size = this.GridParameters.Size;

        var index1D = Enumerable.Range(0, size);
        var allIndices = from y in index1D
                         from x in Enumerable.Range(0, y + 1)
                         select new FieldIndex(x, y);

        var all = allIndices.ToList();
        this.RandomNumberGenerator.Shuffle(all);

        List<FieldIndex> result = [];
        foreach (var index in all)
        {
            bool isDiagonal = index.X == index.Y;

            if (result.Count == n - 2)
            {
                if (isDiagonal)
                {
                    continue;
                }
            }
            else if (result.Count == n - 1)
            {
                if (!isDiagonal)
                {
                    continue;
                }
            }
            else if (result.Count == n)
            {
                break;
            }

            result.Add(index);

            if (!isDiagonal)
            {
                result.Add(index.Transpose());
            }
        }

        if (result.Count != n)
        {
            return maxRecurse > 0
                ? this.GenerateFieldIndices(maxRecurse - 1)
                : throw new InvalidOperationException(
                    "Failed to build a grid for parameters " + this.GridParameters);
        }

        this.RandomNumberGenerator.Shuffle(result);
        return result;
    }
}
