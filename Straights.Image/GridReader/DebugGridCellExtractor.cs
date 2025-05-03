// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

public class DebugGridCellExtractor(IGridCellExtractor core, IDebugInfoWriter debug)
    : IGridCellExtractor
{
    private int i;

    public Mat ExtractGridCell(Mat img, SkewedGridCell cell)
    {
        var result = core.ExtractGridCell(img, cell);
        debug.Save(result, $"cell{this.i++}.png");
        return result;
    }
}
