// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Generator.Empty;

using Straights.Solver.Generator.Empty;

/// <summary>
/// Tests for <see cref="RandomEmptyGridGenerator"/>.
/// </summary>
public class RandomEmptyGridGeneratorTests
{
    [Fact]
    public void Size9_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) => new RandomEmptyGridGenerator(p) { RandomNumberGenerator = r });

        actual.Should().Be(
"""
9
_,_,_,_,_,b1,_,_,b
b2,_,_,_,_,_,_,_,_
_,_,b,_,b,_,b,_,_
_,_,b,_,_,b,_,_,_
_,b3,_,b4,_,_,_,_,_
b,b5,_,_,_,_,_,_,_
b,b,b,_,_,_,_,_,_
_,b,_,_,_,b,_,_,_
b,_,_,_,_,_,_,_,_

""");
    }
}
