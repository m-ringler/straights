// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

using Straights.Solver.Data;

internal readonly record struct Game(
    Grid<SolverField> Solved,
    Grid<SolverField> Unsolved
)
{
    public static implicit operator (
        Grid<SolverField> Solved,
        Grid<SolverField> Unsolved
    )(Game game)
    {
        return (game.Solved, game.Unsolved);
    }
}
