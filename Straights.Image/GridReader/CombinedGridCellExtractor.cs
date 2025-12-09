// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

public class CombinedGridCellExtractor : IGridCellExtractor
{
    private readonly IGridCellExtractor perspective =
        new PerspectiveGridCellExtractor();
    private readonly IGridCellExtractor simple =
        new InnerRectGridCellExtractor();

    public Mat ExtractGridCell(Mat img, SkewedGridCell cell)
    {
        var outer = cell.GetBoundingRect();
        var inner = cell.GetMaximumInscribedRect();
        var innerArea = 1.0 * inner.Width * inner.Height;
        var outerArea = 1.0 * outer.Width * outer.Height;
        bool useSimple =
            (outerArea - innerArea) / (outerArea + innerArea) < 0.03;
        var core = useSimple ? this.simple : this.perspective;
        return core.ExtractGridCell(img, cell);
    }
}
