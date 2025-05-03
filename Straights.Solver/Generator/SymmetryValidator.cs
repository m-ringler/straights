// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator;

public static class SymmetryValidator
{
    public static IEnumerable<string> GetSymmetryViolationErrors(
        UnvalidatedGridConfiguration grid)
    {
        var divisor = GetDivisor(grid.Layout, grid.Size);

        if (grid.TotalNumberOfBlackFields % divisor != 0)
        {
            yield return $"The total number of black fields must be a multiple of {divisor} for size {grid.Size} and layout {grid.Layout}.";
        }
    }

    public static int GetDivisor(GridLayout layout, int size)
    {
        if (size % 2 == 1)
        {
            return 1;
        }

        var divisor = layout switch
        {
            GridLayout.HorizontallySymmetric or
            GridLayout.VerticallySymmetric => 2,
            GridLayout.PointSymmetric => 2,
            GridLayout.HorizontallyAndVerticallySymmetric => 4,
            _ => 1,
        };
        return divisor;
    }
}
