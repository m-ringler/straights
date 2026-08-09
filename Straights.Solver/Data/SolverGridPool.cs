// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

using System.Diagnostics;

public sealed class SolverGridPool
{
    private readonly Stack<SolverGrid> grids = new();
    private readonly Lock syncObj = new();

    public SolverGrid GetCopyOf(SolverGrid template)
    {
        ArgumentNullException.ThrowIfNull(template);

        lock (this.syncObj)
        {
            if (this.grids.TryPop(out var result))
            {
                result.ResetFrom(template);
                return result;
            }
        }

        return template.CreateCopy();
    }

    public void Release(SolverGrid grid)
    {
        ArgumentNullException.ThrowIfNull(grid);

        lock (this.syncObj)
        {
            if (this.grids.Contains(grid))
            {
                throw new InvalidOperationException(
                    "Cannot release a grid that is already in the pool."
                );
            }

            this.grids.Push(grid);
        }
    }
}
