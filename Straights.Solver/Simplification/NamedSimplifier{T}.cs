// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

internal record NamedSimplifier<T>(ISimplify<T> Core, string Name)
    : ISimplify<T>
{
    public void Simplify(T data)
    {
        this.Core.Simplify(data);
    }

    public override string ToString()
    {
        return this.Name;
    }
}
