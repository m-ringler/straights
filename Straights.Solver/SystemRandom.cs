// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver;

using System.Runtime.InteropServices;

/// <summary>
/// A random number generator wrapper for the .NET <see cref="Random"/> class.
/// </summary>
public sealed record SystemRandom
     : IRandom
{
    /// <summary>
    /// Gets the core <see cref="Random"/> instance.
    /// </summary>
    public Random Core { get; init; } = new();

    /// <inheritdoc/>
    public int NextInt32(int minInclusive, int maxExclusive)
    {
        return this.Core.Next(minInclusive, maxExclusive);
    }

    /// <inheritdoc/>
    public double NextDouble()
    {
        return this.Core.NextDouble();
    }

    /// <inheritdoc/>
    public void Shuffle<T>(IList<T> list)
    {
        if (list is T[] array)
        {
            this.Core.Shuffle(array);
        }
        else if (list is List<T> concreteList)
        {
            this.Core.Shuffle(
                CollectionsMarshal.AsSpan(concreteList));
        }
        else
        {
            array = [.. list];
            this.Core.Shuffle(array);
            list.Clear();
            foreach (var item in array)
            {
                list.Add(item);
            }
        }
    }
}
