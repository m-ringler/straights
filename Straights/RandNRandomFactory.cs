// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.Text.RegularExpressions;
using RandN;
using RandN.Rngs;

public partial class RandNRandomFactory
{
    public RandNRandom<Pcg32> CreatePcg32()
    {
        // Use ThreadLocalRng to seed the RNG - this uses a cryptographically secure
        // algorithm, so tight loops won't result in similar seeds
        var seeder = ThreadLocalRng.Instance;

        // Create the seed.
        var factory = Pcg32.GetFactory();
        var seed = factory.CreateSeed(seeder);

        // Create the RNG from the seed.
        var rng = factory.Create(seed);
        return new RandNRandom<Pcg32>(rng) { Seed = GetSeedString(seed) };
    }

    public RandNRandom<Pcg32> CreatePcg32(Pcg32.Seed seed)
    {
        return CreatePcg32(seed, null);
    }

    public RandNRandom<Pcg32> CreatePcg32(string? seedString)
    {
        if (seedString == null)
        {
            return this.CreatePcg32();
        }

        var seed = ParseSeedString(seedString);
        return CreatePcg32(seed, seedString);
    }

    private static RandNRandom<Pcg32> CreatePcg32(
        Pcg32.Seed seed,
        string? seedString
    )
    {
        var factory = Pcg32.GetFactory();
        var rng = factory.Create(seed);
        return new RandNRandom<Pcg32>(rng)
        {
            Seed = seedString ?? GetSeedString(seed),
        };
    }

    private static string GetSeedString(Pcg32.Seed seed)
    {
        return $"Pcg32-{seed.State:x16}-{seed.Stream:x16}";
    }

    private static Pcg32.Seed ParseSeedString(string seed)
    {
        var match = SeedRegex().Match(seed);
        if (!match.Success)
        {
            throw new ArgumentException(
                "The seed string is not in a recognized format.",
                nameof(seed)
            );
        }

        var stateHex = match.Groups[1].Value;
        var streamHex = match.Groups[2].Value;
        var state = Convert.ToUInt64(stateHex, 16);
        var stream = Convert.ToUInt64(streamHex, 16);
        return new Pcg32.Seed(state: state, stream: stream);
    }

    [GeneratedRegex(
        "^Pcg32-([0-9a-f]{16})-([0-9a-f]{16})$",
        RegexOptions.IgnoreCase
    )]
    private static partial Regex SeedRegex();
}
