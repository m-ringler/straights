// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator.Empty;

using Straights.Solver.Builder;

internal static class EmptyGridGenerator
{
    public static void SetBlackNumber(BuilderField?[][] result, in FieldIndex index)
    {
        SetBlackField(result, index, 0);
    }

    public static void SetBlackField(BuilderField?[][] result, in FieldIndex index, int? value)
    {
        var (ix, iy) = index;
        result[iy][ix] = new BuilderField(
                                    new FieldLocation(ix + 1, iy + 1),
                                    Value: value)
        {
            IsWhite = false,
        };
    }

    public static void SetBlackBlank(BuilderField?[][] result, FieldIndex index)
    {
        SetBlackField(result, index, null);
    }

    public static BuilderField?[][] GenerateGrid(
        GridParameters parameters,
        IEnumerable<FieldIndex> placedFields)
    {
        var result = GridBuilder.AllocateFields(parameters.Size);

        using var e = placedFields.GetEnumerator();

        for (int i = 0; i < parameters.NumberOfBlackBlanks; i++)
        {
            _ = e.MoveNext();
            SetBlackBlank(result, e.Current);
        }

        while (e.MoveNext())
        {
            SetBlackNumber(result, e.Current);
        }

        return result;
    }

    public static IEnumerable<FieldIndex> PlaceFieldsUniform(
        IRandom random,
        int size,
        int resultCount)
    {
        return PlaceFieldsUniform(random, size, resultCount, _ => false);
    }

    public static IEnumerable<FieldIndex> PlaceFieldsUniform(
        IRandom random,
        int size,
        int resultCount,
        Func<FieldIndex, bool> isOccupied)
    {
        int numberOfCellsInY = (int)Math.Ceiling(Math.Sqrt(resultCount));
        double cellSize = 1.0 * size / numberOfCellsInY;

        List<UniformEmptyGridGeneratorCell> cells = GetCells(
            numberOfCellsInY,
            cellSize);
        random.Shuffle(cells);

        HashSet<FieldIndex> returned = [];
        List<FieldIndex> result = [];
        foreach (var targetCell in cells.Take(resultCount))
        {
            FieldIndex index;
            do
            {
                index = targetCell.GetRandomLocation(random);
            }
            while (isOccupied(index) || !returned.Add(index));

            result.Add(index);
        }

        return result;
    }

    public static FieldIndex GetFreeRandomLocation(IRandom rng, int size, Func<FieldIndex, bool> isOccupied)
    {
        while (true)
        {
            int ix = rng.NextInt32(0, size);
            int iy = rng.NextInt32(0, size);
            var result = new FieldIndex(ix, iy);
            if (!isOccupied(result))
            {
                return result;
            }
        }
    }

    private static List<UniformEmptyGridGeneratorCell> GetCells(int numberOfCellsInY, double cellSize)
    {
        int numberOfCells = numberOfCellsInY * numberOfCellsInY;
        UniformEmptyGridGeneratorCell cell = new(0, 0, numberOfCellsInY, cellSize);
        List<UniformEmptyGridGeneratorCell> cells = new(numberOfCells);
        while (cells.Count < numberOfCells)
        {
            cells.Add(cell);
            cell = cell.Next();
        }

        return cells;
    }
}
