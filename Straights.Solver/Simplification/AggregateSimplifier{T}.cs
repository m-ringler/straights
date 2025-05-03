// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

/// <summary>
/// Simplifies a collection of items by simplifying the items.
/// </summary>
/// <typeparam name="T">The type of the items in the collection.</typeparam>
/// <param name="core">The object used to simplify the items.</param>
internal sealed class AggregateSimplifier<T>(ISimplify<T> core)
    : ISimplify<IEnumerable<T>>
{
    public ISimplify<T> Core { get; } = core;

    public void Simplify(IEnumerable<T> items)
    {
        this.Core.SimplifyMany(items);
    }
}
