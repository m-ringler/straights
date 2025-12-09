// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

/// <summary>
/// Represents the strength of a simplifier, produced by the
/// <see cref="GridSimplifierFactory"/>.
/// </summary>
public readonly record struct SimplifierStrength
    : IComparable<SimplifierStrength>
{
    public static readonly SimplifierStrength DefaultStrength = new(3);
    public static readonly SimplifierStrength MaxStrength = new(3);

    public SimplifierStrength(int value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, 0);

        this.Value = value;
    }

    public int Value { get; }

    public static implicit operator SimplifierStrength(int value)
    {
        return new SimplifierStrength(value);
    }

    public static bool operator <(
        SimplifierStrength left,
        SimplifierStrength right
    )
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(
        SimplifierStrength left,
        SimplifierStrength right
    )
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(
        SimplifierStrength left,
        SimplifierStrength right
    )
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(
        SimplifierStrength left,
        SimplifierStrength right
    )
    {
        return left.CompareTo(right) >= 0;
    }

    public int CompareTo(SimplifierStrength other)
    {
        return this.Value.CompareTo(other.Value);
    }
}
