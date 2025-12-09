// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Console;

using Straights.Console;
using Straights.Solver;
using Straights.Solver.Data;

/// <summary>
/// Tests for <see cref="ConsoleGridRenderer"/>.
/// </summary>
public class ConsoleGridRendererTests
{
    private const string Grid9x9 = """
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
    public void ConsoleGridRenderer_WhiteToConsole_ProducesCorrectString()
    {
        // ARRANGE
        var grid = ToGrid(Grid9x9);
        var console = new StringBuilderConsole();
        var sut = new ConsoleGridRenderer(console);

        // ACT
        sut.WriteToConsole(grid.Fields);

        // ASSERT
        var actual = console.ToString();
        _ = actual
            .Should()
            .Be(
                """
│ 1  │ ⠿⠇ │ ⠿⠇ │    │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │    │    │
│ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ 9  │ ⠿⠇ │
│    │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ 1  │    │ 9  │ ⠿⠇ │ ⠿⠇ │
│ ⠿⠇ │ ⠿⠇ │    │ 2  │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ 7  │ ⠿⠇ │
│ ⠿⠇ │ 1  │ ⠿⠇ │ 9  │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │    │ ⠿⠇ │
│ 7  │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │    │    │ ⠿⠇ │ ⠿⠇ │
│ ⠿⠇ │ ⠿⠇ │    │    │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ 3  │ 8  │
│ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │
│    │    │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │ 5  │ ⠿⠇ │ ⠿⠇ │ ⠿⠇ │

"""
            );
    }

    private static Grid<SolverField> ToGrid(string builderText)
    {
        return GridConverter.ParseBuilderText(builderText).SolverGrid.Grid;
    }
}
