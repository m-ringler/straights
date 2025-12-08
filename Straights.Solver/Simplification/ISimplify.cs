// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

/// <summary>
/// An interface of an object that can simplify
/// items of type <typeparamref name="T"/>
/// by modifiying them in place.
/// </summary>
/// <typeparam name="T">
/// The type of the items to simplify.
/// </typeparam>
public interface ISimplify<in T>
{
    void Simplify(T item);
}
