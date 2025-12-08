// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Simplification;

using Straights.Solver.Simplification;

#pragma warning disable CS1718 // Comparison made to same variable

public class SimplifierStrengthTests
{
    [Fact]
    public void DefaultStrength_ShouldBeExpectedValue()
    {
        SimplifierStrength.DefaultStrength.Value.Should().Be(3);
    }

    [Fact]
    public void MaxStrength_ShouldBeExpectedValue()
    {
        SimplifierStrength.MaxStrength.Value.Should().Be(3);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentOutOfRangeException_WhenValueIsNegative()
    {
        Action act = () => _ = new SimplifierStrength(-1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_ShouldSetExpectedValue()
    {
        var strength = new SimplifierStrength(5);
        strength.Value.Should().Be(5);
    }

    [Fact]
    public void ImplicitConversion_ShouldCreateSimplifierStrength()
    {
        SimplifierStrength strength = 4;
        strength.Value.Should().Be(4);
    }

    [Fact]
    public void CompareTo_ShouldReturnExpectedResult()
    {
        var strength1 = new SimplifierStrength(2);
        var strength2 = new SimplifierStrength(3);

        strength1.CompareTo(strength2).Should().BeNegative();
        strength2.CompareTo(strength1).Should().BePositive();
        strength1.CompareTo(strength1).Should().Be(0);
    }

    [Fact]
    public void LessThanOperator_ShouldReturnExpectedResult()
    {
        var strength1 = new SimplifierStrength(2);
        var strength2 = new SimplifierStrength(3);

        (strength1 < strength2).Should().BeTrue();
        (strength2 < strength1).Should().BeFalse();
    }

    [Fact]
    public void LessThanOrEqualOperator_ShouldReturnExpectedResult()
    {
        var strength1 = new SimplifierStrength(2);
        var strength2 = new SimplifierStrength(3);

        (strength1 <= strength2).Should().BeTrue();
        (strength2 <= strength1).Should().BeFalse();
        (strength1 <= strength1).Should().BeTrue();
    }

    [Fact]
    public void GreaterThanOperator_ShouldReturnExpectedResult()
    {
        var strength1 = new SimplifierStrength(2);
        var strength2 = new SimplifierStrength(3);

        (strength2 > strength1).Should().BeTrue();
        (strength1 > strength2).Should().BeFalse();
    }

    [Fact]
    public void GreaterThanOrEqualOperator_ShouldReturnExpectedResult()
    {
        var strength1 = new SimplifierStrength(2);
        var strength2 = new SimplifierStrength(3);

        (strength2 >= strength1).Should().BeTrue();
        (strength1 >= strength2).Should().BeFalse();
        (strength1 >= strength1).Should().BeTrue();
    }
}
