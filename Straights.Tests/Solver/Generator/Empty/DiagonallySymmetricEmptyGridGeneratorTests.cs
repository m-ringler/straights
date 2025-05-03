// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Generator.Empty;

using Straights.Solver.Generator.Empty;

/// <summary>
/// Tests for <see cref="DiagonallySymmetricEmptyGridGenerator"/>.
/// </summary>
public class DiagonallySymmetricEmptyGridGeneratorTests
{
    [Fact]
    public void Size9_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) => new DiagonallySymmetricEmptyGridGenerator(p) { RandomNumberGenerator = r });

        actual.Should().Be(
"""
9
_,b1,_,_,b2,b,_,_,_
b,_,_,_,_,b,_,b,_
_,_,b,_,b,_,_,_,_
_,_,_,_,_,_,_,_,_
b,_,b,_,b,_,_,_,b
b3,b4,_,_,_,_,_,_,_
_,_,_,_,_,_,_,_,b
_,b,_,_,_,_,_,_,_
_,_,_,_,b5,_,b,_,_

""");
    }
}
