// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.Collections.Generic;

using RandN;
using RandN.Compat;

using Straights.Solver;

/// <summary>
/// A random number generator wrapper for RandN's <see cref="IRng"/>.
/// </summary>
/// <typeparam name="TRng">The type of the core random number generator.</typeparam>
public record RandNRandom<TRng> : IRandom
    where TRng : IRng
{
    private readonly RandomShim<TRng> shim;

    /// <summary>
    /// Initializes a new instance of the <see cref="RandNRandom{TRng}"/> class.
    /// </summary>
    /// <param name="core">The core random number generator.</param>
    public RandNRandom(TRng core)
    {
        this.Core = core;
        this.shim = RandomShim.Create(core);
    }

    /// <summary>
    /// Gets the core random number generator.
    /// </summary>
    public TRng Core { get; }

    /// <summary>
    /// Gets the seed used to initialize the random number generator.
    /// </summary>
    public required string Seed { get; init; }

    /// <inheritdoc/>
    public int NextInt32(int minInclusive, int maxExclusive)
    {
        return this.shim.Next(minInclusive, maxExclusive);
    }

    /// <inheritdoc/>
    public double NextDouble()
    {
        return this.shim.NextDouble();
    }

    /// <inheritdoc/>
    public void Shuffle<T>(IList<T> list)
    {
        this.Core.ShuffleInPlace(list);
    }
}
