// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Builder;

/// <summary>
/// The location of a field in a square grid.
/// </summary>
/// <param name="X">The X coordinate in the range 1 to gridsize.</param>
/// <param name="Y">The Y coordinate in the range 1 to gridsize.</param>
public sealed record FieldLocation(int X, int Y)
{
    public void Validate(int size)
    {
        if (this.X < 1)
        {
            throw new ValidationException("X must not be less than one " + this);
        }

        if (this.Y < 1)
        {
            throw new ValidationException("Y must not be less than one " + this);
        }

        if (size < this.X)
        {
            throw new ValidationException($"X must not be greater than {size} " + this);
        }

        if (size < this.Y)
        {
            throw new ValidationException($"Y must not be greater than {size} " + this);
        }
    }
}
