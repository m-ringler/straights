// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Simplification;

using Straights.Solver.Simplification;

/// <summary>
/// Tests for <see cref="Combinatorics"/>.
/// </summary>
public class CombinatoricsTests
{
    [Fact]
    public void GetUnorderedCombinations_WhenNonEmpty_YieldsExpected()
    {
        int[] data = [3, 2, 7, 90];

        int[][] expected = [
            [3],
            [2],
            [7],
            [90],
            [3, 2],
            [3, 7],
            [3, 90],
            [2, 7],
            [2, 90],
            [7, 90],
            [3, 2, 7],
            [3, 2, 90],
            [3, 7, 90],
            [2, 7, 90],
            [3, 2, 7, 90],
        ];

        _ = data.GetUnorderedCombinations().Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetUnorderedCombinations_WhenOneItem_YieldsExpected()
    {
        int[] data = [3];

        int[][] expected = [
            [3],
        ];

        _ = data.GetUnorderedCombinations().Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetUnorderedCombinations_WhenEmpty_ReturnsEmpty()
    {
        int[] data = [];

        int[][] expected = [];

        _ = data.GetUnorderedCombinations().Should().BeEquivalentTo(expected);
    }
}
