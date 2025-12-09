// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

public readonly record struct SkewedGridCell(
    PointF TopLeft,
    PointF TopRight,
    PointF BottomRight,
    PointF BottomLeft
)
{
    public (int X, int Y, int Width, int Height) GetMaximumInscribedRect()
    {
        int top = (int)Math.Ceiling(Math.Max(this.TopLeft.Y, this.TopRight.Y));
        int left = (int)
            Math.Ceiling(Math.Max(this.TopLeft.X, this.BottomLeft.X));
        int bottom = (int)
            Math.Floor(Math.Min(this.BottomLeft.Y, this.BottomRight.Y));
        int right = (int)
            Math.Floor(Math.Min(this.TopRight.X, this.BottomRight.X));

        var width = right - left + 1;
        var height = bottom - top + 1;
        return (left, top, width, height);
    }

    public (int X, int Y, int Width, int Height) GetBoundingRect()
    {
        int top = (int)Math.Floor(Math.Min(this.TopLeft.Y, this.TopRight.Y));
        int left = (int)Math.Floor(Math.Min(this.TopLeft.X, this.BottomLeft.X));
        int bottom = (int)
            Math.Ceiling(Math.Max(this.BottomLeft.Y, this.BottomRight.Y));
        int right = (int)
            Math.Ceiling(Math.Max(this.TopRight.X, this.BottomRight.X));

        var width = right - left + 1;
        var height = bottom - top + 1;
        return (left, top, width, height);
    }
}
