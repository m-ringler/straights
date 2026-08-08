// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

public sealed class SolverGridPool
{
    private readonly Stack<SolverGrid> grids = new();

    public SolverGrid GetCopyOf(SolverGrid template)
    {
        ArgumentNullException.ThrowIfNull(template);

        if (this.grids.Count > 0)
        {
            var result = this.grids.Pop();
            result.ResetFrom(template);
            return result;
        }

        return template.CreateCopy();
    }

    public void Release(SolverGrid grid)
    {
        ArgumentNullException.ThrowIfNull(grid);
        this.grids.Push(grid);
    }
}
