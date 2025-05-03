// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Data;

using Straights.Solver.Data;

/// <summary>
/// Tests for <see cref="IntRun"/>.
/// </summary>
public class IntRunTests
{
    [Fact]
    public void Enumeration_WhenMaxGreaterThanMin_ReturnsExpected()
    {
        _ = new IntRun(3, 6).Should().Equal([3, 4, 5, 6]);
    }

    [Fact]
    public void Enumeration_WhenMaxEqualsMin_ReturnsExpected()
    {
        _ = new IntRun(3, 3).Should().Equal([3]);
    }

    [Fact]
    public void Enumeration_WhenMaxLessThanMin_ReturnsExpected()
    {
        _ = new IntRun(3, 2).Should().Equal([]);
    }

    [Fact]
    public void Count_WhenMaxGreaterThanMin_ReturnsExpected()
    {
        _ = new IntRun(3, 6).Should().HaveCount(4);
    }

    [Fact]
    public void Count_WhenMaxEqualsMin_ReturnsExpected()
    {
        _ = new IntRun(3, 3).Should().ContainSingle();
    }

    [Fact]
    public void Count_WhenMaxLessThanMin_ReturnsExpected()
    {
        _ = new IntRun(3, 2).Should().BeEmpty();
    }

    [Fact]
    public void Equals_WhenBothEmpty_ReturnsTrue()
    {
        var sut1 = new IntRun(12, 5);
        var sut2 = new IntRun(-5, -6);

        _ = sut1.Equals(sut2).Should().BeTrue();
        _ = (sut1 == sut2).Should().BeTrue();
        _ = sut2.Equals(sut1).Should().BeTrue();
        _ = sut1.GetHashCode().Should().Be(sut2.GetHashCode());
    }

    [Theory]
    [InlineData(0, 1, 0, 2)]
    [InlineData(0, 1, 1, 1)]
    [InlineData(0, 1, 1, 2)]
    [InlineData(0, 0, 0, -1)]
    public void Equals_WhenDifferent_ReturnsFalse(int min1, int max1, int min2, int max2)
    {
        var sut1 = new IntRun(min1, max1);
        var sut2 = new IntRun(min2, max2);

        _ = sut1.Equals(sut2).Should().BeFalse();
        _ = (sut1 == sut2).Should().BeFalse();
        _ = sut2.Equals(sut1).Should().BeFalse();
        _ = sut1.GetHashCode().Should().NotBe(sut2.GetHashCode());
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void Equals_WhenSelf_ReturnsTrue(int min, int max)
    {
        var sut1 = new IntRun(min, max);
        var sut2 = sut1;

        _ = sut1.Equals(sut2).Should().BeTrue();
        _ = sut2.Equals(sut1).Should().BeTrue();
        _ = sut1.GetHashCode().Should().Be(sut2.GetHashCode());
    }

    [Theory]
    [InlineData(0, 1, 0, 2)]
    [InlineData(0, 1, 1, 1)]
    [InlineData(0, 1, 1, 2)]
    [InlineData(0, 0, 0, -1)]
    [InlineData(0, 17, 9, 20)]
    public void Intersect_IsEquivalentToSetIntersection(int min1, int max1, int min2, int max2)
    {
        var sut1 = new IntRun(min1, max1);
        var sut2 = new IntRun(min2, max2);

        var e1 = (IEnumerable<int>)sut1;
        _ = sut1.Intersect(sut2)
            .Should()
            .Equal(e1.Intersect(sut2));
    }
}