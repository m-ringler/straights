// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Simplification;

using Straights.Solver.Data;
using Straights.Solver.Simplification;

/// <summary>
/// Tests for <see cref="ColumnRemoveSolvedNumbers"/>.
/// </summary>
public class ColumnRemoveSolvedNumbersTests
{
    [Fact]
    public void Simplify_WhenCalled_TransformsColumnAsExpected()
    {
        // ARRANGE
        var column = SolverColumn.Create(
            3,
            [
                [
                    [1],
                    [1, 2, 3],
                ],
                [
                    [1, 2, 3],
                ],
            ]
        );

        var simplifier = new ColumnRemoveSolvedNumbers();

        // ACT
        simplifier.Simplify(column);

        // ASSERT
        _ = column
            .DumpCode()
            .Should()
            .Be(
                """
SolverColumn.Create(3,
[[[1],
[2, 3]],
[[2, 3]]])
"""
            );
    }
}
