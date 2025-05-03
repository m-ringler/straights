// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

public class InnerRectGridCellExtractor
    : IGridCellExtractor
{
    public Mat ExtractGridCell(Mat img, SkewedGridCell cell)
    {
        var rect = cell.GetMaximumInscribedRect();
        var roi = new Rect(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4);
        return new Mat(img, roi);
    }
}
