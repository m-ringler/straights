// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Simplification;

using Straights.Solver.Data;
using Straights.Solver.Simplification;

/// <summary>
/// Tests for <see cref="ColumnNFieldsWithNNumbers"/>.
/// </summary>
public class ColumnNFieldsWithNNumbersTests
{
    [Fact]
    public void Simplify_WhenCalled_TransformsColumn1AsExpected()
    {
        // ARRANGE
        var column = SolverColumn.Create(
            9,
            [[[1],
            [2]],
            [[3, 4],
            [3, 4],
            [7],
            [5, 6],
            [3, 4, 5, 6, 8]]]);

        var simplifier = new ColumnNFieldsWithNNumbers();

        // ACT
        simplifier.Simplify(column);

        // ASSERT
        _ = column.DumpCode().Should().Be(
"""
SolverColumn.Create(9,
[[[1],
[2]],
[[3, 4],
[3, 4],
[7],
[5, 6],
[5, 6, 8]]])
""");
    }

    [Fact]
    public void Simplify_WhenCalled_TransformsColumn2AsExpected()
    {
        // ARRANGE
        var column = SolverColumn.Create(
            9,
            [[[1],
            [3, 4]],
            [[2, 3, 4],
            [2, 4],
            [7],
            [5, 6],
            [3, 4, 5, 6, 8]]]);

        var simplifier = new ColumnNFieldsWithNNumbers();

        // ACT
        simplifier.Simplify(column);

        // ASSERT
        _ = column.DumpCode().Should().Be(
"""
SolverColumn.Create(9,
[[[1],
[3, 4]],
[[2, 3, 4],
[2, 4],
[7],
[5, 6],
[5, 6, 8]]])
""");
    }

    [Fact]
    public void Simplify_WhenCalled_TransformsColumn3AsExpected()
    {
        // ARRANGE
        var column = SolverColumn.Create(
            9,
            [[[5, 7]],
            [[4, 5],
            [4, 5],
            [2],
            [3]],
            [[6]]]);

        var simplifier = new ColumnNFieldsWithNNumbers();

        // ACT
        simplifier.Simplify(column);

        // ASSERT
        _ = column.DumpCode().Should().Be(
"""
SolverColumn.Create(9,
[[[7]],
[[4, 5],
[4, 5],
[2],
[3]],
[[6]]])
""");
    }
}
