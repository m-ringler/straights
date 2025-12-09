// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Generator.Empty;

using Straights.Solver.Generator.Empty;

/// <summary>
/// Tests for <see cref="UniformEmptyGridGenerator"/>.
/// </summary>
public class UniformEmptyGridGeneratorTests
{
    [Fact]
    public void Size9_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) =>
                new UniformEmptyGridGenerator(p) { RandomNumberGenerator = r }
        );

        actual
            .Should()
            .Be(
                """
9
_,b1,_,_,_,_,_,_,b
_,b2,b,_,_,_,_,_,_
_,_,_,_,_,b,_,b,_
_,_,_,b,_,_,_,_,_
_,b,_,_,_,b3,b,_,_
_,b,_,_,_,_,_,_,_
_,b,_,_,_,_,b4,_,_
_,_,_,b5,_,b,_,_,b
_,_,_,_,b,b,_,_,_

"""
            );
    }
}
