// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

/// <summary>
/// An integer interval.
/// </summary>
/// <param name="Min">The smallest value in the interval.</param>
/// <param name="Max">The greatest value in the interval.</param>
/// <remarks>
/// If <paramref name="Max"/> is less than <paramref name="Min"/>
/// the interval is empty. All empty intervals equal each other.
/// </remarks>
public readonly record struct IntRun(int Min, int Max)
    : IReadOnlyCollection<int>, IEquatable<IntRun>
{
    /// <summary>
    /// Gets the number of values in the interval.
    /// </summary>
    public int Count => Math.Max(0, this.Max - this.Min + 1);

    public bool IsEmpty => this.Max < this.Min;

    public IntRun Intersect(IntRun other)
    {
        return new IntRun(
            Math.Max(this.Min, other.Min),
            Math.Min(this.Max, other.Max));
    }

    public bool Intersects(IntRun other)
    {
        return !this.Intersect(other).IsEmpty;
    }

    public bool Equals(IntRun other)
    {
        if (this.IsEmpty)
        {
            return other.IsEmpty;
        }

        return this.Min == other.Min && this.Max == other.Max;
    }

    public override int GetHashCode()
    {
        var (min, max) = this;
        if (max < min)
        {
            min = 0;
            max = -1;
        }

        return HashCode.Combine(typeof(IntRun), min, max);
    }

    public IEnumerator<int> GetEnumerator()
    {
        for (int i = this.Min; i <= this.Max; i++)
        {
            yield return i;
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}