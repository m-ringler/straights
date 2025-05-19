// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver;

/// <summary>
/// The zero-based coordinates of a field
/// in a square grid.
/// </summary>
/// <param name="X">
/// The x coordinate,
/// in the range 0 (leftmost column) to size - 1 (rightmost column).
/// </param>
/// <param name="Y">
/// The y coordinate (row),
/// in the range 0 (topmost row) to size - 1 (bottommost row).
/// </param>
public readonly record struct FieldIndex(int X, int Y)
{
    /// <summary>
    /// Returns the transposed field index.
    /// </summary>
    /// <returns>A field index with swapped X and Y.</returns>
    public FieldIndex Transpose()
    {
        return new(this.Y, this.X);
    }
}
