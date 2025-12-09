// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

/// <summary>
/// Wraps multiple simplifiers as one by calling them one after the other.
/// </summary>
/// <typeparam name="T">The type of the items to simplify.</typeparam>
/// <param name="components">The simplifiers to wrap.</param>
internal sealed class CompositeSimplifier<T>(
    IEnumerable<ISimplify<T>> components
) : ISimplify<T>
{
    public ImmutableArray<ISimplify<T>> Components { get; } = [.. components];

    public void Simplify(T item)
    {
        foreach (var component in this.Components)
        {
            component.Simplify(item);
        }
    }
}
