// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Generator.Empty;

using Straights;
using Straights.Solver;
using Straights.Solver.Builder;
using Straights.Solver.Generator;

internal static class EmptyGridGeneratorTester
{
    public static string GenerateBuilderText(
        Func<GridParameters, IRandom, IEmptyGridGenerator> createSut,
        GridParameters? parameters = null,
        string seed = "Pcg32-1f048cbcdccace6a-60303cf0947a7d0d"
    )
    {
        // ARRANGE
        var rng = new RandNRandomFactory().CreatePcg32(seed);
        GridParameters gridParameters =
            parameters ?? GridParameters.DefaultParameters;

        var sut = createSut(gridParameters, rng);

        // ACT
        var grid = sut.GenerateGrid();

        // ASSERT
        CheckFieldCounts(grid, gridParameters);
        grid.Legalize();
        var actual = grid.Convert().ToBuilderText();
        return actual;
    }

    internal static void CheckFieldCounts(
        BuilderField?[][] grid,
        GridParameters gridParameters
    )
    {
        int blackNumbers = 0;
        int blackBlanks = 0;
        foreach (var field in grid.SelectMany(x => x).Where(x => x != null))
        {
            field!
                .IsWhite.Should()
                .BeFalse(because: "there should be no explicit white fields.");
            if (field.Value == null)
            {
                blackBlanks++;
            }
            else
            {
                field
                    .Value.Should()
                    .Be(0, because: "All black numbers should have value 0");
                blackNumbers++;
            }
        }

        blackBlanks.Should().Be(gridParameters.NumberOfBlackBlanks);
        blackNumbers.Should().Be(gridParameters.NumberOfBlackNumbers);
    }

    private static void Legalize(this BuilderField?[][] grid)
    {
        int k = 1;
        for (int i = 0; i < grid.Length; i++)
        {
            for (int j = 0; j < grid.Length; j++)
            {
                if (grid[i][j]?.Value == 0)
                {
                    grid[i][j] = grid[i][j]! with { Value = k++ };
                }
            }
        }
    }
}
