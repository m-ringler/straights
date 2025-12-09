// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Generator.Empty;

using Moq;
using Straights.Solver;
using Straights.Solver.Generator;
using Straights.Solver.Generator.Empty;

/// <summary>
/// Tests for <see cref="HorizontallyAndVerticallySymmetricEmptyGridGenerator"/>.
/// </summary>
public class HorizontallyAndVerticallySymmetricEmptyGridGeneratorTests
{
    [Fact]
    public void Size9_4n_plus_0_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) =>
                new HorizontallyAndVerticallySymmetricEmptyGridGenerator(p)
                {
                    RandomNumberGenerator = r,
                },
            new GridParameters(9, 13, 3)
        );

        actual
            .Should()
            .Be(
                """
9
_,b,_,_,_,_,_,b,_
_,_,b,_,_,_,b,_,_
_,_,_,_,_,_,_,_,_
_,b1,_,_,_,_,_,b,_
_,b,_,b2,_,b,_,b,_
_,b,_,_,_,_,_,b,_
_,_,_,_,_,_,_,_,_
_,_,b,_,_,_,b,_,_
_,b3,_,_,_,_,_,b,_

"""
            );
    }

    [Fact]
    public void Size9_4n_plus_1_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) =>
                new HorizontallyAndVerticallySymmetricEmptyGridGenerator(p)
                {
                    RandomNumberGenerator = r,
                },
            new GridParameters(9, 13, 4)
        );

        actual
            .Should()
            .Be(
                """
9
_,b,_,_,_,_,_,b,_
_,_,b1,_,_,_,b,_,_
_,_,_,_,_,_,_,_,_
_,b,_,_,_,_,_,b,_
_,b,_,b,b2,b,_,b,_
_,b,_,_,_,_,_,b3,_
_,_,_,_,_,_,_,_,_
_,_,b,_,_,_,b4,_,_
_,b,_,_,_,_,_,b,_

"""
            );
    }

    [Fact]
    public void Size9_4n_plus_2_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) =>
                new HorizontallyAndVerticallySymmetricEmptyGridGenerator(p)
                {
                    RandomNumberGenerator = r,
                },
            new GridParameters(9, 13, 5)
        );

        actual
            .Should()
            .Be(
                """
9
_,b,_,_,_,_,_,b,_
_,_,b,_,_,_,b,_,_
_,_,_,b,_,b,_,_,_
_,b1,_,_,_,_,_,b2,_
_,_,_,b,_,b,_,_,_
_,b3,_,_,_,_,_,b,_
_,_,_,b4,_,b,_,_,_
_,_,b,_,_,_,b,_,_
_,b,_,_,_,_,_,b5,_

"""
            );
    }

    [Fact]
    public void Size9_4n_plus_3_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) =>
                new HorizontallyAndVerticallySymmetricEmptyGridGenerator(p)
                {
                    RandomNumberGenerator = r,
                },
            new GridParameters(9, 13, 6)
        );

        actual
            .Should()
            .Be(
                """
9
_,b,_,_,_,_,_,b,_
_,_,b1,_,_,_,b,_,_
_,_,_,b,_,b,_,_,_
_,b2,_,_,_,_,_,b,_
_,_,_,b,b,b3,_,_,_
_,b,_,_,_,_,_,b,_
_,_,_,b,_,b4,_,_,_
_,_,b,_,_,_,b,_,_
_,b5,_,_,_,_,_,b6,_

"""
            );
    }

    [Fact]
    public void Size8_WithNumberOfBlackFieldsMultipleOf4_CreatesExpectedGrid()
    {
        var actual = EmptyGridGeneratorTester.GenerateBuilderText(
            (p, r) =>
                new HorizontallyAndVerticallySymmetricEmptyGridGenerator(p)
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
_,b,_,_,_,_,b1,_
_,_,_,_,_,_,_,_
b,_,b,_,_,b2,_,b
_,b,_,_,_,_,b,_
_,b,_,_,_,_,b,_
b3,_,b,_,_,b,_,b
_,_,_,_,_,_,_,_
_,b,_,_,_,_,b4,_

"""
            );
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Size8_WithWrongNumberOfBlackFields_Throws(
        int numberOfBlackNumbers
    )
    {
        Action act = () =>
            _ = new HorizontallyAndVerticallySymmetricEmptyGridGenerator(
                new GridParameters(8, 12, numberOfBlackNumbers)
            )
            {
                RandomNumberGenerator = Mock.Of<IRandom>(),
            };

        var ex = act.Should().Throw<ArgumentException>().Which;
        ex.ParamName.Should().Be("gridParameters");
        ex.Message.Should()
            .Match(
                "The total number of black fields must be a multiple of 4 for size 8.*"
            );
    }
}
