// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator.Empty;

using Straights.Solver.Builder;

internal sealed class PointSymmetricEmptyGridGenerator(GridParameters gridParameters)
    : IEmptyGridGenerator
{
    public required IRandom RandomNumberGenerator { get; init; }

    public GridParameters GridParameters { get; } = Validate(gridParameters);

    public BuilderField?[][] GenerateGrid()
    {
        return EmptyGridGenerator.GenerateGrid(
            this.GridParameters,
            this.GenerateFieldIndices(5));
    }

    private static GridParameters Validate(GridParameters gridParameters)
    {
        if (gridParameters.Size % 2 == 0 && gridParameters.TotalNumberOfBlackFields % 2 != 0)
        {
            throw new ArgumentException(
                paramName: nameof(gridParameters),
                message: $"The total number of black fields must be even for size {gridParameters.Size}.");
        }

        return gridParameters;
    }

    private IEnumerable<FieldIndex> GenerateFieldIndices(int maxRecurse)
    {
        int n = this.GridParameters.TotalNumberOfBlackFields;
        int size = this.GridParameters.Size;

        var index1D = Enumerable.Range(0, size);
        var halfSize = size / 2;
        var allIndices = from y in index1D
                         from x in Enumerable.Range(0, halfSize)
                         select new FieldIndex(x, y);

        if (size % 2 == 1)
        {
            allIndices = allIndices.Concat(
                from y in Enumerable.Range(0, halfSize)
                select new FieldIndex(halfSize, y));
        }

        List<FieldIndex> result = [];
        if (n % 2 == 1 && size % 2 == 1)
        {
            result.Add(new FieldIndex(halfSize, halfSize));
        }

        var all = allIndices.ToList();
        this.RandomNumberGenerator.Shuffle(all);

        foreach (var index in all)
        {
            result.Add(index);
            result.Add(new FieldIndex(size - 1 - index.X, size - 1 - index.Y));

            if (result.Count == n)
            {
                break;
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
