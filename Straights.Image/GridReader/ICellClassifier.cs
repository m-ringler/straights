// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

/// <summary>
/// Classifies a grid cell.
/// </summary>
public interface ICellClassifier
{
    /// <summary>
    /// Classifies a grid cell.
    /// </summary>
    /// <param name="cellMat">The grid cell contents as a normalized 8-bit unsigned image.</param>
    /// <returns>The type of the cell.</returns>
    CellType GetCellType(Mat cellMat);
}