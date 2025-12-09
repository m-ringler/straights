// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using Straights.Image.GridReader;
using Straights.Solver.Builder;

public static class CellsToGridBuilderAdapter
{
    public static GridBuilder ToBuilder(Cell[][] cells)
    {
        int n = cells.Length;
        var builder = new GridBuilder(n);
        for (int iy = 0; iy < n; iy++)
        {
            var row = cells[iy];
            int nx = Math.Min(row.Length, n);

            for (int ix = 0; ix < nx; ix++)
            {
                var cell = row[ix];
                switch (cell)
                {
                    case Cell.BlackNumber bn:
                        builder.SetBlack(iy + 1, ix + 1, bn.Number);
                        break;
                    case Cell.WhiteNumber wn:
                        builder.SetWhite(iy + 1, ix + 1, wn.Number);
                        break;
                    case Cell.BlackBlank:
                        builder.SetBlack(iy + 1, ix + 1);
                        break;
                    default:
                        break;
                }
            }
        }

        return builder;
    }
}
