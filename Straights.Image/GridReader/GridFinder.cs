// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

public class GridFinder(IDebugInfoWriter debug)
{
    private const float Degree = (float)(Math.PI / 180);

    public int MinimumLineDistancePixel { get; } = 30;

    public ImageGrid FindGrid(string pathToImage)
    {
        using var img = Cv2.ImRead(pathToImage, ImreadModes.Grayscale);
        var result = this.FindGrid(img);
        return result;
    }

    public ImageGrid FindGrid(Mat img)
    {
        using Mat edges = this.FindEdges(img);

        debug.Save(edges, "edges.png");

        LineSegmentPolar[] lines = FindLines(edges);
        this.Save(img, lines, "lines0");

        LineSegmentPolar[] filteredLines =
            SimilarLinesFilter.FilterSimilarLines(
                lines,
                rho_threshold: this.MinimumLineDistancePixel / 3.0f,
                theta_threshold: 30 * Degree
            );
        this.Save(img, filteredLines, "lines1");

        // remove angle outliers
        var (linesH, linesV) = AngleFilter.FilterAngles(
            filteredLines,
            15 * Degree
        );

        filteredLines = [.. linesH, .. linesV];
        this.Save(img, filteredLines, "lines2");

        // find cell size and remove lines far from the grid.
        var size = img.Size();
        var gf = new GridFilter(debug);
        gf.FilterGrid(linesH, size, out var cellSizeH);
        gf.FilterGrid(linesV, size, out var cellSizeV);

        filteredLines = [.. linesH, .. linesV];
        this.Save(img, filteredLines, "lines3");

        this.Save(
            img,
            [linesH[0], linesH[^1], linesV[0], linesV[^1]],
            "gridBounds"
        );

        return new ImageGrid
        {
            MedianCellSize = (cellSizeV, cellSizeH),
            HorizontalLines = [.. from l in linesH select (l.Rho, l.Theta)],
            VerticalLines = [.. from l in linesV select (l.Rho, l.Theta)],
        };
    }

    private static LineSegmentPolar[] FindLines(Mat edges)
    {
        // Straights grids are usually between
        // 5x5 and 9x9 fields. Therefore, we expect
        // between 10 and 18 lines. We allow for
        // some spurious lines.
        const int approximateNumberOfLines = 20;

        LineSegmentPolar[] lines = FindLines(edges, approximateNumberOfLines);

        Normalize(lines);
        return lines;
    }

    private static LineSegmentPolar[] FindLines(Mat edges, int targetNumLines)
    {
        int minimumNumberOfPointsOnLine = 300;
        const double resolutionRho = 1.0;
        const double resolutionTheta = 2.0f * Degree;

        LineSegmentPolar[] previous = [];
        LineSegmentPolar[] lines = [];

        // We decrease the minimumNumberOfPointsOnLine parameter
        // until we find more than our target number of lines.
        while (lines.Length < targetNumLines)
        {
            previous = lines;
            lines = Cv2.HoughLines(
                edges,
                resolutionRho,
                resolutionTheta,
                threshold: minimumNumberOfPointsOnLine
            );
            minimumNumberOfPointsOnLine -= 20;
        }

        // Then we check whether these lines or the
        // previously generated lines are closer in number to
        // our target, and choose our return value accordingly.
        // Empirically, we have a big step in the number of lines when we
        // start seeing lines that are not there.
        // Therefore, this works even with a rough estimate of the
        // correct number of lines.
        if (
            previous.Length != 0
            && targetNumLines - previous.Length < lines.Length - targetNumLines
        )
        {
            lines = previous;
        }

        return lines;
    }

    private static void Normalize(LineSegmentPolar[] lines)
    {
        const float Pi = (float)Math.PI;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Rho < 0)
            {
                lines[i].Rho = -lines[i].Rho;
                lines[i].Theta -= Pi;
            }
        }
    }

    private void Save(Mat img, LineSegmentPolar[] filteredLines, string name)
    {
        debug.Save(img, filteredLines, name + ".png");
        debug.Save(filteredLines, name + ".txt");
    }

    private Mat FindEdges(Mat img)
    {
        Mat edges = img.Canny(90, 150);
        try
        {
            debug.Save(edges, "canny.png");

            const int closeKernelSize = 2;
            edges.Close(closeKernelSize);
            return edges;
        }
        catch
        {
            edges.Dispose();
            throw;
        }
    }

    public record class LineWithCoordinate(LineSegmentPolar Line, float Coord);
}
