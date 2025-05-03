// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

/// <summary>
/// Extracts a grid cell to a rectangular, undistorted image.
/// </summary>
public interface IGridCellExtractor
{
    Mat ExtractGridCell(Mat img, SkewedGridCell cell);
}
