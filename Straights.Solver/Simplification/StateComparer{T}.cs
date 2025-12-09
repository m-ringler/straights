// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

/// <summary>
/// An <see cref="IChangeDetector{T}"/> implementation
/// for snapshottable objects, i. e. objects implementing
/// <see cref="IGetSnapshot{T}"/>.
/// </summary>
/// <remarks>
/// A <see cref="StateComparer{T}"/> compares a
/// possibly fluctuating current state with a fixed
/// initial state. The initial state cannot be updated.
/// </remarks>
/// <typeparam name="T">
/// The type of the snapshot data.
/// </typeparam>
public class StateComparer<T>(IEnumerable<IGetSnapshot<T>> itemsToMonitor)
    : IChangeDetector<IGetSnapshot<T>>
    where T : IEquatable<T>
{
    private readonly ImmutableDictionary<IGetSnapshot<T>, T> initialSnapshots =
        itemsToMonitor.ToImmutableDictionary(
            item => item,
            item => item.GetSnapshot()
        );

    public IEnumerable<IGetSnapshot<T>> MonitoredItems =>
        this.initialSnapshots.Keys;

    public bool HasChanged(IGetSnapshot<T> item)
    {
        if (!this.initialSnapshots.TryGetValue(item, out T? initialState))
        {
            return true;
        }

        T currentState = item.GetSnapshot();
        return !currentState.Equals(initialState);
    }
}
