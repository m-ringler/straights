// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

using Straights.Solver.Data;

internal static class GridToIntArraysConverter
{
    public static int[][][] ToIntArrays(this Grid<SolverField> grid)
    {
        var result = new int[grid.Size][][];
        for (int iy = 0; iy < grid.Size; iy++)
        {
            var row = new int[grid.Size][];
            result[iy] = row;
            for (int ix = 0; ix < grid.Size; ix++)
            {
                var field = grid.GetField(ix, iy);
                row[ix] = field switch
                {
                    SolverField.BlackBlank => [0],
                    SolverField.BlackNumber b =>
                    [
                        b.Number == 0 && b.GetWhiteFieldData().IsSolved
                            ? -b.GetWhiteFieldData().Min
                            : -b.Number,
                    ],
                    SolverField.WhiteField w => [.. w.Data],
                    _ => throw new InvalidOperationException(
                        "Unknown field type " + field.GetType()
                    ),
                };
            }
        }

        return result;
    }

    public static Grid<SolverField> GridFromIntArrays(this int[][][] fields)
    {
        var bb = new SolverField.BlackBlank();
        int size = fields.Length;

        IEnumerable<SolverField> Convert()
        {
            for (int iy = 0; iy < size; iy++)
            {
                var row = fields[iy];
                for (int ix = 0; ix < row.Length; ix++)
                {
                    SolverField field = row[ix] switch
                    {
                        int[] x when x.Length == 0 =>
                            new SolverField.WhiteField(
                                new WhiteFieldData(size)
                            ),
                        int[] x when x.Length == 1 && x[0] == 0 => bb,
                        int[] x when x.Length == 1 && x[0] < 0 =>
                            new SolverField.BlackNumber(-x[0], size),
                        int[] x => new SolverField.WhiteField(
                            CreateWhiteFieldData(x, size)
                        ),
                    };

                    yield return field;
                }
            }
        }

        return new Grid<SolverField>([.. Convert()]);
    }

    public static Grid<SolverField> GridFromJson(string json)
    {
        return GridFromIntArrays(
            IntArraysToJsonConverter.IntArraysFromJson(json)
        );
    }

    public static string ToJson(this Grid<SolverField> grid)
    {
        return IntArraysToJsonConverter.ToJson(ToIntArrays(grid));
    }

    private static WhiteFieldData CreateWhiteFieldData(int[] x, int size)
    {
        var wf = new WhiteFieldData(size);
        var others = wf.Except(x).ToArray();
        wf.Remove(others);
        return wf;
    }
}
