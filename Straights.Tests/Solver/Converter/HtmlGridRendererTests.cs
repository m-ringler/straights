// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Converter;

using Straights.Solver;
using Straights.Solver.Converter;
using Straights.Solver.Data;
using Straights.Solver.Simplification;

/// <summary>
/// Tests for <see cref="HtmlGridRenderer"/>.
/// </summary>
public class HtmlGridRendererTests
{
    internal const string Grid9x9 =
"""
9
w1,_,_,b,_,_,_,b,b
_,_,_,_,_,_,_,w9,_
b,_,_,_,w1,b,b9,_,_
_,_,b,b2,_,_,_,w7,_
_,b1,_,w9,_,_,_,b,_
w7,_,_,_,_,b,b,_,_
_,_,b,b,_,_,_,w3,b8
_,_,_,_,_,_,_,_,_
b,b,_,_,_,b5,_,_,_

""";

    [Fact]
    public Task Convert9x9()
    {
        // ARRANGE
        var grid = ToGrid(Grid9x9);
        var solverGrid = SolverGrid.FromFieldGrid(grid);
        var simplifier = new ColumnRemoveSolvedNumbers();
        foreach (var column in solverGrid.Columns)
        {
            simplifier.Simplify(column);
        }

        // ACT
        var actual = solverGrid.Convert().ToHtml();

        // ASSERT
        return Verify(actual);
    }

    private static Grid<SolverField> ToGrid(string grid)
    {
        return GridConverter.ParseBuilderText(grid).SolverFieldGrid;
    }
}
