// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using Straights.Solver;
using Straights.Solver.Converter;
using Straights.Solver.Generator;
using Straights.Solver.Simplification;
using Straights.Tests.Solver.Simplification;
using static Straights.Solver.Generator.GridLayout;

/// <summary>
/// Tests for the <see cref="Play"/> class.
/// </summary>
public class PlayTests
{
    public static TheoryData<GridLayout> SymmetricGridLayouts()
    {
        return
        [
            .. from layout in Enum.GetValues<GridLayout>()
            where
                layout
                    .ToString()
                    .Contains("Symmetric", StringComparison.Ordinal)
            select layout,
        ];
    }

    [Theory]
    [InlineData(5, 3)]
    [InlineData(6, 2)]
    [InlineData(7, 1)]
    [InlineData(8, 0)]
    public void GeneratesCorrectSizeAndDifficulty(int size, int difficulty)
    {
        // ACT
        var code = Play.GenerateGameCode(size, difficulty, (int)PointSymmetric);

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
        solved2
            .IsSolved.Should()
            .BeTrue(
                because: $"A grid of difficulty {difficulty} should be solvable with a strength-{difficulty} solver."
            );

        solved2
            .Convert()
            .ToBuilderText()
            .Should()
            .Be(solved.Convert().ToBuilderText());
    }

    [Fact]
    public Task GenerateHint_WhenSolvable_ReturnsHint()
    {
        var grid = HintGeneratorTests.Grid10;
        return Verify(Play.GenerateHint(grid, 3));
    }

    [Fact]
    public void GenerateHint_WhenNotSolvable_ReturnsEmptyObject()
    {
        var grid = HintGeneratorTests.Grid10;
        Play.GenerateHint(grid, 0).Should().Be("{}");
    }

    [Theory]
    [MemberData(nameof(SymmetricGridLayouts))]
    public void GenerateGameCode_ProducesSymmetricGrid(GridLayout layout)
    {
        // ARRANGE / ACT
        var code = Play.GenerateGameCode(
            size: 7,
            difficulty: 3,
            gridLayout: (int)layout
        );

        // ASSERT
        var (_, unsolved) = GridConverter.ConvertUrlParameter(code);
        var grid = unsolved.Convert();
        VerifySymmetry(grid, layout);
    }

    private static void VerifySymmetry(ConvertibleGrid grid, GridLayout layout)
    {
        var builderFields = grid.Builder.GetFields();
        var bw =
            from row in builderFields
            select (from field in row select field?.IsWhite != false).ToArray();
        var bwArray = bw.ToArray();
        int size = bwArray.Length;
        (int X, int Y)[] GetSymmetricCoordinates((int X, int Y) coord)
        {
            return layout switch
            {
                DiagonallySymmetric => [(coord.Y, coord.X)],
                HorizontallySymmetric => [(coord.X, size - 1 - coord.Y)],
                VerticallySymmetric => [(size - 1 - coord.X, coord.Y)],
                HorizontallyAndVerticallySymmetric =>
                [
                    (size - 1 - coord.X, coord.Y),
                    (coord.X, size - 1 - coord.Y),
                ],
                PointSymmetric => [(size - 1 - coord.X, size - 1 - coord.Y)],
                _ => throw new InvalidOperationException(
                    "Symmetry check not implemented for layout " + layout
                ),
            };
        }

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                var coord = (X: x, Y: y);
                var symmetricCoords = GetSymmetricCoordinates(coord);
                foreach (var symCoord in symmetricCoords)
                {
                    bwArray[coord.X]
                        [coord.Y]
                        .Should()
                        .Be(
                            bwArray[symCoord.X][symCoord.Y],
                            because: $"Field at ({coord.X},{coord.Y}) and ({symCoord.X},{symCoord.Y}) should be symmetric in layout {layout}."
                        );
                }
            }
        }
    }
}
