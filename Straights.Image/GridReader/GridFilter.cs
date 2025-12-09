// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

using static Straights.Image.GridReader.GridFinder;

internal sealed class GridFilter(IDebugInfoWriter debug)
{
    public void FilterGrid(
        List<LineSegmentPolar> lines1,
        Size imgSize,
        out float cellLength
    )
    {
        var origin = new Point(0, 0);
        var meanTheta = lines1.Average(l => l.Theta);

        // Construct an orthogonal cut line through the image center.
        Point center = GetCenter(imgSize);
        LineSegmentPolar cutLine = ConstructLineThroughPoint(
            lineTheta: (0.5 * Math.PI) + meanTheta,
            point: center
        );

        // Intersect the lines with the cut line, calculate and store
        // the signed distance of the intersection point from the image center,
        // and order the lines by that value.
        double cc = center.DistanceTo(origin);
        List<LineWithCoordinate> lines =
        [
            .. from l in lines1
            let cutPoint = l.LineIntersection(cutLine)
            where cutPoint.HasValue
            let p = cutPoint.Value
            let r = p.DistanceTo(origin)
            let d = (float)(Math.Sign(r - cc) * center.DistanceTo(p))
            orderby d
            select new LineWithCoordinate(l, d),
        ];
        debug.Save(lines, "cuts.txt");

        // Calculate the distances of the lines along the cut line.
        var deltas = Distances(lines).ToList();

        // Compute the cell size as the median distance
        // and select a start line that we think belongs to
        // the grid, because it has at least one neighbor with the correct
        // distance.
        var cellSize = deltas[deltas.Count / 2].Delta;
        var startLine = deltas[deltas.Count / 2].Pair.First;
        cellLength = cellSize;

        const float toleranceRelative = 0.20f;
        var startIndex = lines.IndexOf(startLine);
        List<LineSegmentPolar>[] results =
        [
            [],
            [],
        ];
        int[] steps = [-1, 1];

        // Starting from that line iteratively look for the next grid line...
        // ... in both directions
        for (int istep = 0; istep < 2; istep++)
        {
            int step = steps[istep];
            var result = results[istep];
            LineWithCoordinate lastKeptLine = startLine;
            int lastKeptLineIndex = startIndex;
            float localCellSize = cellSize;
            float nextCoord = startLine.Coord + (step * localCellSize);
            int numCells = 1;
            List<(int Index, float Delta)> deltaCoords = [];

            for (
                int i = startIndex + step;
                i >= 0 && i < lines.Count;
                i += step
            )
            {
                var currentLine = lines[i];
                float deltaCoord = step * (currentLine.Coord - nextCoord);
                deltaCoords.Add((i, Math.Abs(deltaCoord)));

                if (deltaCoord > 0.5 * localCellSize)
                {
                    var best = deltaCoords.MinBy(x => x.Delta);
                    if (best.Delta < toleranceRelative * localCellSize)
                    {
                        float lastCoord = lastKeptLine.Coord;
                        currentLine = lines[best.Index];

                        // synthesize missing lines
                        if (numCells > 1)
                        {
                            var lastLine = lastKeptLine.Line;
                            float deltaRho =
                                (currentLine.Line.Rho - lastLine.Rho)
                                / numCells;
                            float deltaTheta =
                                (currentLine.Line.Theta - lastLine.Theta)
                                / numCells;
                            for (int j = 1; j < numCells; j++)
                            {
                                result.Add(
                                    new(
                                        rho: lastLine.Rho + (j * deltaRho),
                                        theta: lastLine.Theta + (j * deltaTheta)
                                    )
                                );
                            }
                        }

                        result.Add(currentLine.Line);
                        float currentCoord = currentLine.Coord;
                        localCellSize =
                            step * (currentCoord - lastCoord) / numCells;

                        nextCoord = currentCoord + (step * localCellSize);
                        lastKeptLine = currentLine;
                        lastKeptLineIndex = best.Index;
                        numCells = 1;
                    }
                    else
                    {
                        numCells++;
                        nextCoord += step * localCellSize;
                    }

                    i = lastKeptLineIndex;
                    deltaCoords.Clear();
                }
            }

            if (deltaCoords.Count > 0)
            {
                var best = deltaCoords.MinBy(x => x.Delta);
                if (best.Delta < toleranceRelative * localCellSize)
                {
                    var currentLine = lines[best.Index];
                    result.Add(currentLine.Line);
                }
            }
        }

        lines1.Clear();
        results[0].Reverse();
        lines1.AddRange([.. results[0], startLine.Line, .. results[1]]);
    }

    private static LineSegmentPolar ConstructLineThroughPoint(
        double lineTheta,
        Point point
    )
    {
        double c = point.DistanceTo(new Point(0, 0));
        double gamma = Math.Atan2(point.Y, point.X);
        double lineRho = c * Math.Cos(gamma - lineTheta);
        return new((float)lineRho, (float)lineTheta);
    }

    private static Point GetCenter(Size imgSize)
    {
        double c1 = imgSize.Width / 2.0;
        double c2 = imgSize.Height / 2.0;
        Point center = new(c1, c2);
        return center;
    }

    private static IEnumerable<(
        float Delta,
        (LineWithCoordinate First, LineWithCoordinate Second) Pair
    )> Distances(IEnumerable<LineWithCoordinate> lines)
    {
        var pairs = PairsOfNeighbors(lines);
        return from pair in pairs
            select (Delta: pair.Second.Coord - pair.First.Coord, pair) into item
            orderby item.Delta
            select item;
    }

    private static IEnumerable<(T First, T Second)> PairsOfNeighbors<T>(
        IEnumerable<T> items
    )
    {
        return items.Zip(items.Skip(1));
    }
}
