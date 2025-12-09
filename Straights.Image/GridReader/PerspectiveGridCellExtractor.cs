// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

public class PerspectiveGridCellExtractor : IGridCellExtractor
{
    public Mat ExtractGridCell(Mat img, SkewedGridCell cell)
    {
        var rect = cell.GetBoundingRect();
        var s = Math.Min(rect.Width, rect.Height);

        var pointsSrc = new[]
        {
            cell.TopLeft,
            cell.TopRight,
            cell.BottomRight,
            cell.BottomLeft,
        }.Select(p => new Point2f(p.X, p.Y));

        Point2f[] pointsDst = [new(0, 0), new(s, 0), new(s, s), new(0, s)];

        using var transform = Cv2.GetPerspectiveTransform(pointsSrc, pointsDst);

        var output = new Mat();
        Cv2.WarpPerspective(img, output, transform, new(s, s));

        var roi = new Rect(2, 2, s - 4, s - 4);
        return new Mat(output, roi);
    }
}
