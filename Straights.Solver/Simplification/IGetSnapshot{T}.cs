// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

/// <summary>
/// Interface of an object that can create an equality-comparable
/// snapshot to use for change monitoring.
/// </summary>
/// <remarks>
/// If two snapshots are equal this means that the
/// object has not changed between the snapshots.
/// The reverse does not have to hold.
/// </remarks>
/// <typeparam name="T">The type of the snapshot.</typeparam>
public interface IGetSnapshot<out T>
    where T : IEquatable<T>
{
    T GetSnapshot();
}
