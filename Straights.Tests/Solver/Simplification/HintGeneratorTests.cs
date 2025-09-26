// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Simplification;

using Straights.Solver;
using Straights.Solver.Data;
using Straights.Solver.Simplification;

/// <summary>
/// Tests for <see cref="HintGenerator"/>.
/// </summary>
public class HintGeneratorTests
{
    private const string Grid10 =
    "[[[2],[9,10],[7,8,9,10],[1],[3],[8,9],[7,8],[4],[6],[5]],[[-1],[9,10],[8,9,10],[0],[4],[6],[5],[0],[3],[2]],[[8,9,10],[-1],[8,9,10],[8,9,10],[0],[7],[6],[5],[4],[3]],[[3],[2],[0],[7,8,9],[10],[8,9],[7,8],[6],[5],[4]],[[4],[-3],[5],[0],[9],[0],[0],[-10],[7],[6]],[[5],[4],[0],[0],[0],[2],[0],[9],[0],[7]],[[7],[5],[3],[2],[6],[1],[4],[0],[9],[8]],[[6,8],[6,8],[4],[5],[7],[-10],[3],[2],[0],[1]],[[6,8,9],[7,8],[0],[3],[5],[4],[0],[1],[2],[-10]],[[6,10],[6,7],[6,7,10],[4],[8],[5],[2],[3],[1],[9]]]";

    [Fact]
    public Task GenerateHint_SimplifiableGrid_ReturnsHint()
    {
        // Arrange
        var grid = GridConverter.ParseJson(Grid10).SolverGrid;
        var hintGenerator = new HintGenerator(new SimplifierStrength(3));

        // Act
        var hint = hintGenerator.GenerateHint(grid);

        // Assert
        return Verify(hint)
            .AddExtraSettings(s => s.DefaultValueHandling = Argon.DefaultValueHandling.Include);
    }

    [Fact]
    public void GenerateHint_NonSimplifiableGrid_Throws()
    {
        // Arrange
        var grid = GridConverter.ParseJson(Grid10).SolverGrid;
        var hintGenerator = new HintGenerator(new SimplifierStrength(0));

        // Act & Assert
        Action act = () => hintGenerator.GenerateHint(grid);
        act.Should().Throw<NotSolvableException>();
    }
}