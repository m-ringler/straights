// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

/// <summary>
/// Detects changes of items of a particular type.
/// </summary>
/// <typeparam name="T">The type of the items.</typeparam>
public interface IChangeDetector<in T>
{
    bool HasChanged(T item);
}
