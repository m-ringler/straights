// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Data;

using Straights.Solver.Data;

/// <summary>
/// Tests for <see cref="WhiteFieldData"/>.
/// </summary>
public class WhiteFieldDataTests
{
    [Fact]
    public void SolverFieldData_WhenConstructed_ContainsAllNumbers()
    {
        const int size = 17;
        var sut = new WhiteFieldData(size);
        _ = sut.Max.Should().Be(size);
        _ = sut.Min.Should().Be(1);
        _ = sut.Count.Should().Be(size);
        _ = sut.Size.Should().Be(size);
        _ = sut.Should().Equal(Enumerable.Range(1, size));
        for (int i = 1; i <= size; i++)
        {
            _ = sut.Contains(i).Should().BeTrue();
        }
    }

    [Fact]
    public void SolverFieldData_WhenSomeNumbersRemoved_ContainsExpected()
    {
        const int size = 17;
        int[] remove = [5, 6, 7, 1, 2, 17, 12];
        var sut = new WhiteFieldData(size);

        int n = size;
        foreach (var item in remove)
        {
            _ = sut.Remove(item).Should().BeTrue();
            _ = sut.Count.Should().Be(--n);
        }

        _ = sut.Max.Should().Be(16);
        _ = sut.Min.Should().Be(3);
        _ = sut.Count.Should().Be(size - remove.Length);
        _ = sut.Size.Should().Be(size);
        _ = sut.Should().Equal(Enumerable.Range(1, size).Except(remove));
        for (int i = 1; i <= size; i++)
        {
            _ = sut.Contains(i).Should().Be(!remove.Contains(i));
        }
    }

    [Fact]
    public void SolverFieldData_WhenConstructed_HasCorrectBrailleString()
    {
        const int size = 9;
        var sut = new WhiteFieldData(size);
        _ = sut.ToCompactString().Should().Be("⠿⠇");
    }

    [Fact]
    public void SolverFieldData_WhenSomeNumbersRemoved_HasCorrectBrailleString()
    {
        const int size = 9;
        var sut = new WhiteFieldData(size);
        _ = sut.Remove(2);
        _ = sut.Remove(8);
        _ = sut.Remove(9);
        _ = sut.ToCompactString().Should().Be("⠽⠁");
    }

    [Fact]
    public void SolverFieldData_WhenAllButOneNumberRemoved_HasCorrectCompactString()
    {
        const int size = 9;
        var sut = new WhiteFieldData(size);
        for (int i = 1; i <= size; i++)
        {
            if (i != 3)
            {
                _ = sut.Remove(i);
            }
        }

        _ = sut.ToCompactString().Should().Be("3");
    }
}
