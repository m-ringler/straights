// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Generator.Empty;

using Moq;
using Straights.Solver;
using Straights.Solver.Generator;
using Straights.Solver.Generator.Empty;

/// <summary>
/// Tests for <see cref="HorizontallySymmetricEmptyGridGenerator"/>.
/// </summary>
public class HorizontallySymmetricEmptyGridGeneratorTests
{
    [Fact]
    public void Size9_2n_plus_0_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) =>
                new HorizontallySymmetricEmptyGridGenerator(p)
                {
                    RandomNumberGenerator = r,
                },
            new GridParameters(9, 13, 3),
            "Pcg32-1f048cbcdccace6a-60303cf0947a7fff"
        );

        actual
            .Should()
            .Be(
                """
9
_,_,_,_,_,_,_,_,_
_,b,b,_,b1,_,b,b,_
_,_,b2,_,b,_,b,_,_
_,_,_,b,_,b,_,_,_
_,_,b,_,_,_,b,_,_
_,_,_,b,_,b,_,_,_
_,_,_,_,_,_,_,_,_
_,_,b3,_,_,_,b,_,_
_,_,_,_,_,_,_,_,_

"""
            );
    }

    [Fact]
    public void Size9_2n_plus_1_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) =>
                new HorizontallySymmetricEmptyGridGenerator(p)
                {
                    RandomNumberGenerator = r,
                },
            new GridParameters(9, 13, 4),
            "Pcg32-1f048cbcdccace6a-60303cf0947a7fff"
        );

        actual
            .Should()
            .Be(
                """
9
_,_,_,_,_,_,_,_,_
_,b,b,_,b,_,b1,b,_
_,_,b,_,b,_,b,_,_
_,_,_,b2,_,b,_,_,_
_,_,b,_,_,_,b3,_,_
_,_,_,b,_,b4,_,_,_
_,_,_,_,b,_,_,_,_
_,_,b,_,_,_,b,_,_
_,_,_,_,_,_,_,_,_

"""
            );
    }

    [Fact]
    public void Size8_WithEvenNumberOfBlackFields_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) =>
                new HorizontallySymmetricEmptyGridGenerator(p)
                {
                    RandomNumberGenerator = r,
                },
            new GridParameters(8, 10, 4),
            "Pcg32-1f048cbcdccace6a-60303cf0947a7fff"
        );

        actual
            .Should()
            .Be(
                """
8
_,_,_,_,_,_,_,_
b,_,_,b,b,_,_,b1
_,_,b,_,_,b,_,_
_,_,_,b2,b3,_,_,_
_,_,b,b,b4,b,_,_
_,_,_,_,_,_,_,_
_,_,_,_,_,_,_,_
_,_,b,_,_,b,_,_

"""
            );
    }

    [Fact]
    public void Size8_WithOddNumberOfBlackFields_Throws()
    {
        Action act = () =>
            _ = new HorizontallySymmetricEmptyGridGenerator(
                new GridParameters(8, 10, 5)
            )
            {
                RandomNumberGenerator = Mock.Of<IRandom>(),
            };

        var ex = act.Should().Throw<ArgumentException>().Which;
        ex.ParamName.Should().Be("gridParameters");
        ex.Message.Should()
            .Match(
                "The total number of black fields must be even for size 8.*"
            );
    }
}
