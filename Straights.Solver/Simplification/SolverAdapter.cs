// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using Straights.Solver.Data;

public static class SolverAdapter
{
    public static ISolver ToSolver(this ISimplify<SolverGrid> gridSimplifier)
    {
        return new SimplifierSolver(gridSimplifier);
    }

    private sealed record SimplifierSolver(ISimplify<SolverGrid> Simplifier)
        : ISolver
    {
        public SolverGrid Solve(SolverGrid data)
        {
            var dataOut = data.CreateCopy();
            this.Simplifier.Simplify(dataOut);
            return dataOut;
        }
    }
}
