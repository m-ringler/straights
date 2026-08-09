// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Generator;

using Straights.Solver;
using Straights.Solver.Generator;
using Straights.Solver.Generator.Empty;
using Straights.Solver.Simplification;

/// <summary>
/// Tests for <see cref="GridGenerator"/>.
/// </summary>
public class GridGeneratorTests
{
    [Fact]
    public void GenerateGrid_WithDefaultParameters_CreatesNonNullGrid()
    {
        // ARRANGE
        string seed = "Pcg32-bf048cbcdccace6a-a0303cf0947a7d0d";
        var rnd = new RandNRandomFactory().CreatePcg32(seed);
        var solver = new RecursiveTrialAndErrorSolver()
        {
            RandomNumberGenerator = rnd,
        };

        var emptyGridGen = new RandomEmptyGridGenerator(
            GridParameters.DefaultParameters
        )
        {
            RandomNumberGenerator = rnd,
        };

        var sut = new GridGenerator(solver, emptyGridGen);

        // ACT
        var result = sut.GenerateGrid();

        // ASSERT
        _ = result.Should().NotBeNull();
        _ = result
            .ToString()
            .Should()
            .Be(
                """
9
b2,w6,w5,w7,w9,w8,w3,w4,b
b,w7,w6,w8,b,b3,w4,w5,b
w9,w8,w7,b,w5,w6,b1,w3,w4
b,b9,w8,w3,w4,w7,w6,w2,w5
b,w4,b,w6,w7,b,w5,b1,b
w4,w2,w1,w5,w3,b,w8,w7,w6
w5,w3,w2,w1,w8,w4,w7,w6,w9
w6,w1,w3,w4,w2,w5,b,w9,w8
w3,w5,w4,w2,w6,b,w9,w8,w7

"""
            );
    }

    [Fact]
    public void GenerateGrid_WithHintRemover_CreatesNonNullGrid()
    {
        // ARRANGE
        string seed = "Pcg32-bf048cbcdccace6a-a0303cf0947a7d0d";
        var rnd = new RandNRandomFactory().CreatePcg32(seed);
        var solver = new RecursiveTrialAndErrorSolver()
        {
            RandomNumberGenerator = rnd,
        };

        var emptyGridGen = new RandomEmptyGridGenerator(
            GridParameters.DefaultParameters
        )
        {
            RandomNumberGenerator = rnd,
        };

        var spuriousHintRemover = new DifficultyAdjuster(
            solver.GridSimplifier.ToSolver()
        )
        {
            RandomNumberGenerator = rnd,
        };

        var sut = spuriousHintRemover.Decorate(
            new GridGenerator(solver, emptyGridGen)
        );

        // ACT
        var result = sut.GenerateGrid();

        // ASSERT
        _ = result.Should().NotBeNull();
        _ = result
            .ToString()
            .Should()
            .Be(
                """
9
b2,_,_,_,_,_,_,_,b
b,_,_,_,b,b3,_,_,b
_,_,_,b,_,_,b1,_,_
b,b9,_,_,_,_,_,_,_
b,_,b,_,_,b,_,b1,b
_,w2,_,_,_,b,_,_,w6
_,_,_,_,_,w4,_,_,_
_,w1,_,_,_,_,b,_,_
w3,_,_,_,_,b,_,_,_

"""
            );
    }

    [Theory]
    [InlineData("Pcg32-1f048cbcdccace6a-60303cf0947a7d0d")]
    [InlineData("Pcg32-ffffffffffffffff-0000000000000000")]
    [InlineData("Pcg32-0000000000000000-0000000000000000")]
    [InlineData("Pcg32-ffffffffffffffff-ffffffffffffffff")]
    [InlineData("Pcg32-0000000000000000-ffffffffffffffff")]
    public void GenerateGrid_WithUniformEmptyGridGen_CreatesNonNullGrid(
        string seed
    )
    {
        // ARRANGE
        var rng = new RandNRandomFactory().CreatePcg32(seed);
        var solver = new RecursiveTrialAndErrorSolver()
        {
            RandomNumberGenerator = rng,
            MaximumNumberOfRecursions = 50,
        };

        var emptyGridGen = new UniformIndependentEmptyGridGenerator(
            GridParameters.DefaultParameters
        )
        {
            RandomNumberGenerator = rng,
        };

        var sut = new GridGenerator(solver, emptyGridGen);

        // ACT
        var result = sut.GenerateGrid();

        // ASSERT
        _ = result.Should().NotBeNull();
    }
}
