// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver;

using Straights.Solver;

/// <summary>
/// Tests for <see cref="RecursiveTrialAndErrorSolver"/>.
/// and <see cref="EliminatingSolver"/>.
/// </summary>
public class SolverTests
{
    [Fact]
    public void RecursiveTrialAndErrorSolver_SolvesGrid()
    {
        var solver = new RecursiveTrialAndErrorSolver
        {
            RandomNumberGenerator = new SystemRandom(),
        };

        TestSolver(solver);
    }

    [Fact]
    public void EliminatingSolver_SolvesGrid()
    {
        var solver = new EliminatingSolver();
        TestSolver(solver);
    }

    private static void TestSolver(ISolver solver)
    {
        const string unsolvedGrid =
"""
9
b,_,b9,b,_,w4,_,b,w2
_,_,_,_,w4,_,_,_,b9
w7,b,_,_,_,_,b,_,b
_,b,_,_,_,w2,_,w8,_
b,_,_,w6,b1,b,_,_,w3
_,_,_,_,w6,_,_,w5,b
_,_,b1,_,_,_,_,_,_
_,b,b,_,b,_,_,_,w5
w1,_,_,b9,_,w5,_,_,_

""";

        var grid = GridConverter
            .ParseBuilderText(unsolvedGrid)
            .SolverGrid;

        // ACT
        var solvedGrid = solver.Solve(grid);

        // ASSERT
        _ = solvedGrid.IsSolved.Should().BeTrue();

        _ = solvedGrid.Convert()
            .ToJson().Should().Be(
        """
        [
          [[0], [7], [-9], [0], [5], [4], [3], [0], [2]],
          [[8], [6], [5], [7], [4], [1], [2], [3], [-9]],
          [[7], [0], [4], [1], [2], [3], [0], [6], [0]],
          [[9], [0], [6], [5], [3], [2], [7], [8], [4]],
          [[0], [5], [7], [6], [-1], [0], [4], [2], [3]],
          [[3], [4], [8], [2], [6], [7], [9], [5], [0]],
          [[2], [3], [-1], [4], [7], [8], [5], [9], [6]],
          [[4], [0], [0], [3], [0], [6], [8], [7], [5]],
          [[1], [2], [3], [-9], [8], [5], [6], [4], [7]]
        ]

        """);
    }
}