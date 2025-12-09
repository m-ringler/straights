// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Generator;

using Moq;
using Straights.Solver;
using Straights.Solver.Generator;
using Straights.Solver.Generator.Empty;

/// <summary>
/// Tests for <see cref="RandomEmptyGridGeneratorFactory"/>.
/// </summary>
public class EmptyGridGeneratorFactoryTests
{
    private readonly GridParameters gridParameters = new(15, 3, 7);
    private readonly RandomEmptyGridGeneratorFactory sut = new(
        Mock.Of<IRandom>()
    );

    [Fact]
    public void GetRandom_ReturnsExpected()
    {
        var actual = this.sut.GetRandom(this.gridParameters);
        var result = actual
            .Should()
            .BeOfType<RandomEmptyGridGenerator>()
            .Subject;
        result
            .RandomNumberGenerator.Should()
            .BeSameAs(this.sut.RandomNumberGenerator);
        result.GridParameters.Should().Be(this.gridParameters);
    }

    [Fact]
    public void GetUniform_ReturnsExpected()
    {
        var actual = this.sut.GetUniform(this.gridParameters);
        var result = actual
            .Should()
            .BeOfType<UniformEmptyGridGenerator>()
            .Subject;
        result
            .RandomNumberGenerator.Should()
            .BeSameAs(this.sut.RandomNumberGenerator);
        result.GridParameters.Should().Be(this.gridParameters);
    }

    [Fact]
    public void GetUniformIndependent_ReturnsExpected()
    {
        var actual = this.sut.GetUniformIndependent(this.gridParameters);
        var result = actual
            .Should()
            .BeOfType<UniformIndependentEmptyGridGenerator>()
            .Subject;
        result
            .RandomNumberGenerator.Should()
            .BeSameAs(this.sut.RandomNumberGenerator);
        result.GridParameters.Should().Be(this.gridParameters);
    }

    [Fact]
    public void GetDiagonallySymmetric_ReturnsExpected()
    {
        var actual = this.sut.GetDiagonallySymmetric(this.gridParameters);
        var result = actual
            .Should()
            .BeOfType<DiagonallySymmetricEmptyGridGenerator>()
            .Subject;
        result
            .RandomNumberGenerator.Should()
            .BeSameAs(this.sut.RandomNumberGenerator);
        result.GridParameters.Should().Be(this.gridParameters);
    }

    [Fact]
    public void GetHorizontallySymmetric_ReturnsExpected()
    {
        var actual = this.sut.GetHorizontallySymmetric(this.gridParameters);
        var result = actual
            .Should()
            .BeOfType<HorizontallySymmetricEmptyGridGenerator>()
            .Subject;
        result
            .RandomNumberGenerator.Should()
            .BeSameAs(this.sut.RandomNumberGenerator);
        result.GridParameters.Should().Be(this.gridParameters);
    }

    [Fact]
    public void GetVerticallySymmetric_ReturnsExpected()
    {
        var actual = this.sut.GetVerticallySymmetric(this.gridParameters);
        var result = actual
            .Should()
            .BeOfType<VerticallySymmetricEmptyGridGenerator>()
            .Subject;
        result
            .RandomNumberGenerator.Should()
            .BeSameAs(this.sut.RandomNumberGenerator);
        result.GridParameters.Should().Be(this.gridParameters);
    }

    [Fact]
    public void GetHorizontallyAndVerticallySymmetric_ReturnsExpected()
    {
        var actual = this.sut.GetHorizontallyAndVerticallySymmetric(
            this.gridParameters
        );
        var result = actual
            .Should()
            .BeOfType<HorizontallyAndVerticallySymmetricEmptyGridGenerator>()
            .Subject;
        result
            .RandomNumberGenerator.Should()
            .BeSameAs(this.sut.RandomNumberGenerator);
        result.GridParameters.Should().Be(this.gridParameters);
    }
}
