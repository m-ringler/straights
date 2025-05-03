// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Simplification;

using Straights.Solver.Data;
using Straights.Solver.Simplification;

/// <summary>
/// Tests for <see cref="ColumnConsistentRanges"/>.
/// </summary>
public class ColumnConsistentRangesTests
{
    [Fact]
    public void Simplify_WhenCalled_TransformsColumn1AsExpected()
    {
        // ARRANGE
        var column = SolverColumn.Create(
            9,
            [[[2, 3, 4, 5],
            [4, 6],
            [2, 3, 4, 5, 7]],
            [[5, 6, 7, 8],
            [4, 6, 7]]]);

        var simplifier = new ColumnConsistentRanges();

        // ACT
        simplifier.Simplify(column);

        // ASSERT
        _ = column.DumpCode().Should().Be(
"""
SolverColumn.Create(9,
[[[2, 3, 4, 5],
[4, 6],
[2, 3, 4, 5]],
[[5, 6, 7, 8],
[6, 7]]])
""");
    }

    [Fact]
    public void Simplify_WhenCalled_TransformsColumn2AsExpected()
    {
        // ARRANGE
        var column = SolverColumn.Create(
            9,
            [[[1, 2, 3, 4, 5, 6, 8],
            [1, 2, 3, 4, 7]],
            [[9]],
            [[2, 3, 4, 5],
            [4, 6],
            [2, 3, 4, 5, 7]],
            [[5, 6, 7, 8],
            [6, 7]]]);

        var simplifier = new ColumnConsistentRanges();

        // ACT
        simplifier.Simplify(column);

        // ASSERT
        _ = column.DumpCode().Should().Be(
"""
SolverColumn.Create(9,
[[[1, 2, 3, 5, 6, 8],
[1, 2, 3, 7]],
[[9]],
[[2, 3, 4, 5],
[4, 6],
[2, 3, 4, 5]],
[[5, 6, 7, 8],
[6, 7]]])
""");
    }
}
