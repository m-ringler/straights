// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

/// <summary>
/// Wraps a delegate as a simplifier.
/// </summary>
/// <typeparam name="T">The type of the items to simplify.</typeparam>
/// <param name="simplify">The delegate to call.</param>
internal sealed class DelegateSimplifier<T>(Action<T> simplify) : ISimplify<T>
{
    public void Simplify(T item)
    {
        simplify(item);
    }
}
