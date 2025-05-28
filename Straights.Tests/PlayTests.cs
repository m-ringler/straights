// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using Straights.Solver;
using Straights.Solver.Simplification;

/// <summary>
/// Tests for the <see cref="Play"/> class.
/// </summary>
public class PlayTests
{
    [Theory]
    [InlineData(5, 3)]
    [InlineData(6, 2)]
    [InlineData(7, 1)]
    [InlineData(8, 0)]
    public void GeneratesCorrectSizeAndDifficulty(
        int size,
        int difficulty)
    {
        // ACT
        var code = Play.GenerateGameCode(size, difficulty);

        // ASSERT
        var (solved, unsolved) = GridConverter.ConvertUrlParameter(code);
        _ = solved.Should().NotBeNull();
        _ = unsolved.Should().NotBeNull();

        var builderUnsolved = unsolved.Convert().Builder;
        builderUnsolved.Size.Should().Be(size);

        var solver = GridSimplifierFactory
            .BuildIterativeSimplifier(difficulty)
            .ToSolver();
        var solved2 = solver.Solve(unsolved.Convert().SolverGrid);
        solved2.IsSolved.Should().BeTrue(
            because: $"A grid of difficulty {difficulty} should be solvable with a strength-{difficulty} solver.");

        solved2.Convert().ToBuilderText()
            .Should().Be(
                solved.Convert().ToBuilderText());
    }
}