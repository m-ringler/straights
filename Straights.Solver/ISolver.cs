// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver;

using Straights.Solver.Data;

public interface ISolver
{
    /// <summary>
    /// Attempts to solve the specified grid.
    /// </summary>
    /// <param name="data">The data to solve, will not be modified.</param>
    /// <returns>
    /// The processed data, <see cref="SolverGrid.IsSolved"/> indicates whether
    /// solving was successful.
    /// </returns>
    SolverGrid Solve(SolverGrid data);
}
