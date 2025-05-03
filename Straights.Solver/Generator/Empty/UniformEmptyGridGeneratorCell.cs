// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator.Empty;

using Straights.Solver;

internal sealed record UniformEmptyGridGeneratorCell(int X, int Y, int GridSize, double CellSize)
{
    public UniformEmptyGridGeneratorCell Next()
    {
        int y = this.Y;
        int x = this.X + 1;
        if (x == this.GridSize)
        {
            x = 0;
            y++;
            if (y == this.GridSize)
            {
                y = 0;
            }
        }

        return new UniformEmptyGridGeneratorCell(x, y, this.GridSize, this.CellSize);
    }

    public FieldIndex GetRandomLocation(IRandom rng)
    {
        var xmin = this.X * this.CellSize;
        int x = (int)Math.Floor((rng.NextDouble() * this.CellSize) + xmin);

        var ymin = this.Y * this.CellSize;
        int y = (int)Math.Floor((rng.NextDouble() * this.CellSize) + ymin);
        return new(x, y);
    }
}
