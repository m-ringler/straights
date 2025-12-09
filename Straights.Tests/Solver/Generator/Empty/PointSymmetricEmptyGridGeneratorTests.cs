// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Generator.Empty;

using Moq;
using Straights.Solver;
using Straights.Solver.Generator;
using Straights.Solver.Generator.Empty;

/// <summary>
/// Tests for <see cref="DiagonallySymmetricEmptyGridGenerator"/>.
/// </summary>
public class PointSymmetricEmptyGridGeneratorTests
{
    [Fact]
    public void Size9_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) =>
                new PointSymmetricEmptyGridGenerator(p)
                {
                    RandomNumberGenerator = r,
                }
        );

        actual
            .Should()
            .Be(
                """
9
_,_,b1,_,_,_,_,_,b
_,_,b,_,_,_,b,_,_
b,_,_,b,_,_,_,_,b
_,b2,_,_,_,_,_,_,_
_,_,_,b3,_,b4,_,_,_
_,_,_,_,_,_,_,b,_
b5,_,_,_,_,b,_,_,b
_,_,b,_,_,_,b,_,_
b,_,_,_,_,_,b,_,_

"""
            );
    }

    [Fact]
    public void Size5_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) =>
                new PointSymmetricEmptyGridGenerator(p)
                {
                    RandomNumberGenerator = r,
                },
            new(5, 3, 2),
            seed: "Pcg32-d385c64a3dd36d56-49d6424b625a94a8"
        );

        actual
            .Should()
            .Be(
                """
5
_,_,_,_,_
_,_,_,_,b
b,_,b1,_,b
b2,_,_,_,_
_,_,_,_,_

"""
            );
    }

    [Fact]
    public void Size8_WhenEven_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) =>
                new PointSymmetricEmptyGridGenerator(p)
                {
                    RandomNumberGenerator = r,
                },
            new GridParameters(8, 12, 4)
        );

        actual
            .Should()
            .Be(
                """
8
_,b,_,_,b1,b,_,_
_,b2,_,_,_,_,_,_
b3,_,_,_,_,b,_,_
_,_,b,_,_,_,_,b
b,_,_,_,_,b,_,_
_,_,b,_,_,_,_,b
_,_,_,_,_,_,b,_
_,_,b,b4,_,_,b,_

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
