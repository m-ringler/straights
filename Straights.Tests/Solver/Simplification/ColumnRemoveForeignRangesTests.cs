// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Simplification;

using Straights.Solver.Data;
using Straights.Solver.Simplification;

/// <summary>
/// Tests for <see cref="ColumnRemoveForeignRanges"/>.
/// </summary>
public class ColumnRemoveForeignRangesTests
{
    [Fact]
    public void Simplify_WhenCalled_TransformsColumn1AsExpected()
    {
        // ARRANGE
        var column = SolverColumn.Create(
            9,
            [
                [
                    [3, 4, 5],
                    [3, 4],
                ],
                [
                    [2],
                ],
                [
                    [9],
                    [8],
                ],
                [
                    [1, 3, 4, 5, 6, 7],
                    [1, 3, 4, 6, 7],
                ],
            ]
        );

        var simplifier = new ColumnRemoveForeignRanges();

        // ACT
        simplifier.Simplify(column);

        // ASSERT
        _ = column
            .DumpCode()
            .Should()
            .Be(
                """
SolverColumn.Create(9,
[[[3, 4, 5],
[3, 4]],
[[2]],
[[9],
[8]],
[[1, 3, 5, 6, 7],
[1, 3, 6, 7]]])
"""
            );
    }
}
