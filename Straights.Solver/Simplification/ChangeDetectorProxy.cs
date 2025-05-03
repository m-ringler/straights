// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

/// <summary>
/// Wraps changing <see cref="IChangeDetector{T}"/>s
/// as a single <see cref="IChangeDetector{T}"/>.
/// </summary>
/// <typeparam name="T">
/// The type of the items whose changes are detected.
/// </typeparam>
public sealed class ChangeDetectorProxy<T>(bool defaultValue)
    : IChangeDetector<T>
{
    public IChangeDetector<T>? Core { get; set; }

    public bool DefaultValue { get; } = defaultValue;

    /// <summary>
    /// Delegates to <see cref="Core"/> or returns the <see cref="DefaultValue"/>
    /// when it is null.
    /// </summary>
    /// <param name="item">The item that may have changed.</param>
    /// <returns>
    /// The value returned by <see cref="Core"/> if it is not null,
    /// or the <see cref="DefaultValue"/> otherwise.
    /// </returns>
    public bool HasChanged(T item)
    {
        return this.Core?.HasChanged(item) ?? this.DefaultValue;
    }
}