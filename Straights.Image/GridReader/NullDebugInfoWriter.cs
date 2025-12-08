// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

public class NullDebugInfoWriter : IDebugInfoWriter
{
    public void Save(IEnumerable<LineSegmentPolar> lines, string fileName)
    {
    }

    public void Save(IEnumerable<GridFinder.LineWithCoordinate> lines, string fileName)
    {
    }

    public void Save(Mat img, LineSegmentPolar[] lines, string fileName)
    {
    }

    public void Save(Mat img, string fileName)
    {
    }
}
