// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Simplification;

using Straights.Solver.Data;
using Straights.Solver.Simplification;

/// <summary>
/// Tests for <see cref="BlockDisjunctSubsets"/>.
/// </summary>
public class BlockDisjunctSubsetsTests
{
    [Fact]
    public void Simplify_WhenCalled_TransformsColumn1AsExpected()
    {
        // ARRANGE
        var column = SolverColumn.Create(
            9,
            [
                [
                    [4, 6, 7, 8, 9],
                    [6, 7],
                    [4, 6, 7, 8, 9],
                ],
            ]
        );

        var simplifier = new BlockDisjunctSubsets();

        // ACT
        simplifier.Simplify(column.Blocks[0]);

        // ASSERT
        _ = column
            .DumpCode()
            .Should()
            .Be(
                """
SolverColumn.Create(9,
[[[6, 7, 8, 9],
[6, 7],
[6, 7, 8, 9]]])
"""
            );
    }

    [Fact]
    public void Simplify_WhenCalled_TransformsColumn2AsExpected()
    {
        // ARRANGE
        var column = SolverColumn.Create(
            9,
            [
                [
                    [4, 6, 7, 9],
                    [5, 6, 7],
                    [4, 6, 7, 9],
                ],
            ]
        );

        var simplifier = new BlockDisjunctSubsets();

        // ACT
        simplifier.Simplify(column.Blocks[0]);

        // ASSERT
        _ = column
            .DumpCode()
            .Should()
            .Be(
                """
SolverColumn.Create(9,
[[[4, 6, 7],
[5, 6, 7],
[4, 6, 7]]])
"""
            );
    }
}
