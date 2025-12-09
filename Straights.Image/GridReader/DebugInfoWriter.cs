// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

using static Straights.Image.GridReader.GridFinder;

public class DebugInfoWriter(string folder) : IDebugInfoWriter
{
    public void Save(Mat img, string fileName)
    {
        _ = Cv2.ImWrite(this.GetPath(fileName), img);
    }

    public void Save(IEnumerable<LineSegmentPolar> lines, string fileName)
    {
        var textLines =
            from l in lines select $"{l.Rho}; {l.Theta}";
        File.WriteAllLines(this.GetPath(fileName), textLines);
    }

    public void Save(IEnumerable<LineWithCoordinate> lines, string fileName)
    {
        var textLines =
            from l in lines select $"{l.Coord}; {l.Line.Rho}; {l.Line.Theta}";
        File.WriteAllLines(this.GetPath(fileName), textLines);
    }

    public void Save(Mat img, LineSegmentPolar[] lines, string fileName)
    {
        using var linesImage = img.CvtColor(ColorConversionCodes.GRAY2BGR);
        foreach (var line in lines)
        {
            Draw(linesImage, line);
        }

        _ = Cv2.ImWrite(this.GetPath(fileName), linesImage);
    }

    private static void Draw(Mat linesImage, LineSegmentPolar line)
    {
        var seg = line.ToSegmentPoint(5000);
        Cv2.Line(linesImage, seg.P1, seg.P2, new(0, 0, 255), 2);
    }

    private string GetPath(string name)
    {
        return Path.Combine(folder, name);
    }
}
