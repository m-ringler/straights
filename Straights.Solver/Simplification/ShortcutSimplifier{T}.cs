// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

/// <summary>
/// A simplifier that simplifies an item
/// only if the <paramref name="changeDetector"/>
/// indicates that it may have changed (since the last
/// time that it has been simplified by the
/// <paramref name="core"/> simplifier).
/// </summary>
/// <typeparam name="T">The type of the items to simplify.</typeparam>
/// <param name="core">The core simplifier to wrap.</param>
/// <param name="changeDetector">
/// A change detector that indicates whether
/// items to be simplified may have changed
/// since the last time they were simplified.
/// </param>
public class ShortcutSimplifier<T>(
    ISimplify<T> core,
    IChangeDetector<T> changeDetector)
     : ISimplify<T>
{
    public ISimplify<T> Core { get; } = core;

    public IChangeDetector<T> ChangeDetector { get; } = changeDetector;

    public void Simplify(T item)
    {
        if (this.ChangeDetector.HasChanged(item))
        {
            this.Core.Simplify(item);
        }
    }
}