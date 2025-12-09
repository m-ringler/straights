// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Generator.Empty;

using Straights.Solver.Generator.Empty;

/// <summary>
/// Tests for <see cref="UniformIndependentEmptyGridGenerator"/>.
/// </summary>
public class UniformIndependentEmptyGridGeneratorTests
{
    [Fact]
    public void Size9_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) =>
                new UniformIndependentEmptyGridGenerator(p)
                {
                    RandomNumberGenerator = r,
                }
        );

        actual
            .Should()
            .Be(
                """
9
_,b,_,_,_,_,_,b1,_
b2,_,b,_,_,b,_,_,_
_,_,b,_,_,b,_,_,_
_,_,_,_,b3,_,_,_,_
b,_,_,_,_,_,b4,_,_
_,_,_,b,_,_,b,_,_
_,b,_,_,_,_,_,b,_
b5,_,_,_,_,_,_,_,_
_,_,b,_,_,_,b,_,b

"""
            );
    }
}
