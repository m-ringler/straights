// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

using System.Collections.Generic;

public interface IDebugInfoWriter : IDebugImageWriter
{
    void Save(IEnumerable<LineSegmentPolar> lines, string fileName);

    void Save(IEnumerable<GridFinder.LineWithCoordinate> lines, string fileName);

    void Save(Mat img, LineSegmentPolar[] lines, string fileName);
}

public interface IDebugImageWriter
{
    void Save(Mat img, string fileName);
}
