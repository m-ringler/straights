// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

using Straights.Solver.Builder;
using Straights.Solver.Data;

internal static class SolverGridToBuilderConverter
{
    public static GridBuilder ToBuilder(this Grid<SolverField> grid)
    {
        var result = new GridBuilder(grid.Size);
        foreach (var index in grid.AllFieldIndices())
        {
            var field = grid.GetField(index);
            var (ix, iy) = index;
            if (field is SolverField.WhiteField wf && wf.Data.IsSolved)
            {
                result.SetWhite(iy + 1, ix + 1, wf.Data.Min);
            }
            else if (field is SolverField.BlackNumber bn)
            {
                result.SetBlack(iy + 1, ix + 1, bn.GetWhiteFieldData().Min);
            }
            else if (field is SolverField.BlackBlank)
            {
                result.SetBlack(iy + 1, ix + 1);
            }
        }

        return result;
    }
}
