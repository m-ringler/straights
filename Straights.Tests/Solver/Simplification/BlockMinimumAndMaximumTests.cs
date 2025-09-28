// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Simplification;

using Straights.Solver.Data;
using Straights.Solver.Simplification;

/// <summary>
/// Tests for <see cref="BlockMinimumAndMaximum"/>.
/// </summary>
public class BlockMinimumAndMaximumTests
{
    [Fact]
    public void Simplify_WhenCalled_TransformsColumn1AsExpected()
    {
        // ARRANGE
        var column = SolverColumn.Create(
            9,
            [[[9], [2, 4, 5, 6, 8], [7]]]);

        var simplifier = new BlockMinimumAndMaximum();

        // ACT
        simplifier.Simplify(column.Blocks[0]);

        // ASSERT
        _ = column.DumpCode().Should().Be(
"""
SolverColumn.Create(9,
[[[9],
[8],
[7]]])
""");
    }

    [Fact]
    public void Simplify_WhenCalled_TransformsColumn2AsExpected()
    {
        // ARRANGE
        var column = SolverColumn.Create(
            9,
            [[[1, 2, 3, 4, 5, 6, 7, 8, 9], [1, 2, 3, 4, 5, 6, 7, 8, 9], [1, 2, 3, 4, 5, 6, 7, 8, 9], [1, 2]]]);

        var simplifier = new BlockMinimumAndMaximum();

        // ACT
        simplifier.Simplify(column.Blocks[0]);

        // ASSERT
        _ = column.DumpCode().Should().Be(
"""
SolverColumn.Create(9,
[[[1, 2, 3, 4, 5],
[1, 2, 3, 4, 5],
[1, 2, 3, 4, 5],
[1, 2]]])
""");
    }

    [Fact]
    public void Simplify_WhenCalled_TransformsColumn3AsExpected()
    {
        // ARRANGE
        var column = SolverColumn.Create(
            9,
            [[[1, 2, 3, 4, 5, 6, 7, 8, 9], [1, 2, 3, 4, 5, 6, 7, 8, 9], [1, 2, 3, 4, 5, 6, 7, 8, 9], [7, 8, 9]]]);

        var simplifier = new BlockMinimumAndMaximum();

        // ACT
        simplifier.Simplify(column.Blocks[0]);

        // ASSERT
        _ = column.DumpCode().Should().Be(
"""
SolverColumn.Create(9,
[[[4, 5, 6, 7, 8, 9],
[4, 5, 6, 7, 8, 9],
[4, 5, 6, 7, 8, 9],
[7, 8, 9]]])
""");
    }

    [Fact]
    public void Simplify_WhenCalled_TransformsColumn4AsExpected()
    {
        // ARRANGE
        var column = SolverColumn.Create(
            9,
            [[[7], [1, 2, 3, 4, 5, 6, 7, 8, 9], [1, 2, 3, 4, 5, 6, 7, 8, 9], [8], [1, 2, 3, 4, 5, 6, 7, 8, 9]]]);

        var simplifier = new BlockMinimumAndMaximum();

        // ACT
        simplifier.Simplify(column.Blocks[0]);

        // ASSERT
        _ = column.DumpCode().Should().Be(
"""
SolverColumn.Create(9,
[[[7],
[4, 5, 6, 7, 8, 9],
[4, 5, 6, 7, 8, 9],
[8],
[4, 5, 6, 7, 8, 9]]])
""");
    }
}
