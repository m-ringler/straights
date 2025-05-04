// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver;

using Straights.Solver;
using Straights.Solver.Generator;
using Straights.Solver.Simplification;

using GridLayout = Straights.Solver.Generator.GridLayout;

public class GeneratorBuilderTests
{
    [Fact]
    public Task ReadmeSampleCode()
    {
        // All properties have default values.
        var generator = new GeneratorBuilder
        {
            DifficultyLevel = (SimplifierStrength)3,
            Layout = GridLayout.Uniform,
            GridParameters = new GridParameters(
                size: 5,
                numberOfBlackBlanks: 2,
                numberOfBlackNumbers: 1),
            Random = new SystemRandom { Core = new Random(123456) },
        }
        .Build();

        var grid = generator.GenerateGrid();
        var gridText = grid?.Convert().ToBuilderText()
            ?? throw new InvalidOperationException("Grid generation failed");

        return Verify(gridText);
    }
}