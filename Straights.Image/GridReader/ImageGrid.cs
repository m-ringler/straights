// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

public record class ImageGrid
{
    public (float W, float H) MedianCellSize { get; init; }

    public int NumCellX => this.VerticalLines.Length - 1;

    public int NumCellY => this.HorizontalLines.Length - 1;

    public ImmutableArray<(float Rho, float Theta)> HorizontalLines { get; init; }

    public ImmutableArray<(float Rho, float Theta)> VerticalLines { get; init; }

    public SkewedGridCell GetCell(int ix, int iy)
    {
        var left = this.VerticalLines[ix];
        var right = this.VerticalLines[ix + 1];
        var top = this.HorizontalLines[iy];
        var bottom = this.HorizontalLines[iy + 1];
        return new SkewedGridCell(
            TopLeft: Intersect(top, left),
            TopRight: Intersect(top, right),
            BottomRight: Intersect(bottom, right),
            BottomLeft: Intersect(bottom, left));
    }

    public T[][] BuildArray<T>(
        Func<SkewedGridCell, T> selectCellResult)
    {
        T[][] result = new T[this.NumCellY][];

        for (int iy = 0; iy < result.Length; iy++)
        {
            var row = new T[this.NumCellX];
            result[iy] = row;

            for (int ix = 0; ix < row.Length; ix++)
            {
                var cell = this.GetCell(ix, iy);
                row[ix] = selectCellResult(cell);
            }
        }

        return result;
    }

    private static PointF Intersect((float Rho, float Theta) a, (float Rho, float Theta) b)
    {
        var la = new LineSegmentPolar(a.Rho, a.Theta);
        var lb = new LineSegmentPolar(b.Rho, b.Theta);
        var pt = la.LineIntersection(lb);
        if (!pt.HasValue)
        {
            throw new InvalidOperationException("Cannot intersect parallel lines.");
        }

        var p = pt.Value;
        return new(p.X, p.Y);
    }
}
