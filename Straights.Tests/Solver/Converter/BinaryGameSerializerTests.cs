// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Converter;

using Straights.Solver;
using Straights.Solver.Converter;
using Straights.Solver.Data;
using Straights.Solver.Generator;
using Straights.Solver.Generator.Empty;
using Straights.Solver.Simplification;

/// <summary>
/// Tests for <see cref="BinaryGameSerializer"/>.
/// </summary>
public class BinaryGameSerializerTests
{
    private static readonly int[][][] Solved4x4 =
[
  [[1], [2], [3], [4]],
  [[2], [3], [4], [0]],
  [[4], [0], [-1], [2]],
  [[3], [4], [2], [1]]
];

    private static readonly int[][][] Unsolved4x4 =
    [
      [[1], [], [], []],
      [[], [], [4], [0]],
      [[], [0], [-1], []],
      [[3], [], [], [1]]
    ];

    public static TheoryData<int> GridSizes => [.. Enumerable.Range(4, 6)];

    [Fact]
    public void ToBinary4x4()
    {
        // ARRANGE
        var solved = GridConverter.Convert(Solved4x4).SolverFieldGrid;
        var unsolved = GridConverter.Convert(Unsolved4x4).SolverFieldGrid;

        // ACT
        var actual = GridConverter.ToBinaryString(solved, unsolved, BinaryGameSerializer.EncodingVersion);

        // ASSERT
        var expected = (
          "10000000" // EncodingVersion
/*  8 */+ "00100" // Size 0b100
/* 13 */+ "0100 0001 0010 0011"
/* 29 */+ "0001 0010 0111 1011"
/* 45 */+ "0011 1011 1100 0001"
/* 61 */+ "0110 0011 0001 0100").Replace(" ", string.Empty);
        actual.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(GridSizes))]
    public void RoundTripBinary(int gridSize)
    {
        // ARRANGE
        var (solved, unsolved) = GenerateGame(gridSize);

        // ACT
        var binary = GridConverter.ToBinaryString(
            solved, unsolved, BinaryGameSerializer.EncodingVersion);
        var (actualSolved, actualUnsolved) = GridConverter.ConvertBinaryString(
            binary, BinaryGameSerializer.EncodingVersion);

        // ASSERT
        ShouldEqual(unsolved, actualUnsolved);
        ShouldEqual(solved, actualSolved);
    }

    [Theory]
    [MemberData(nameof(GridSizes))]
    public void RoundTripBase64(int gridSize)
    {
        // ARRANGE
        var (solved, unsolved) = GenerateGame(gridSize);

        // ACT
        var encoded = GridConverter.ToUrlParameter(
            solved,
            unsolved,
            BinaryGameSerializer.EncodingVersion);
        var (actualSolved, actualUnsolved) = GridConverter.ConvertUrlParameter(
            encoded,
            BinaryGameSerializer.EncodingVersion);

        // ASSERT
        ShouldEqual(unsolved, actualUnsolved);
        ShouldEqual(solved, actualSolved);
    }

    private static void ShouldEqual(Grid<SolverField> expected, Grid<SolverField> actual)
    {
        var actualJson = actual.Convert().ToJson();
        var expectedJson = expected.Convert().ToJson();
        actualJson.Should().Be(expectedJson);
    }

    private static Game GenerateGame(int size)
    {
        var rng = new RandNRandomFactory().CreatePcg32("Pcg32-aa6eb674cf4465ab-ad4c4f2aa9acaba8");
        var parameters = new GridParameters(size, size * size * 13 / 81, size * size * 5 / 81);
        var solver = new RecursiveTrialAndErrorSolver { RandomNumberGenerator = rng };
        var generator = new DifficultyAdjuster(solver.GridSimplifier.ToSolver())
        {
            RandomNumberGenerator = rng,
        }
            .Decorate(new GridGenerator(
                solver,
                new RandomEmptyGridGenerator(parameters) { RandomNumberGenerator = rng }));
        var builder = generator.GenerateGrid()!;
        var unsolved = builder.Convert().SolverGrid;
        var solved = solver.Solve(unsolved);
        return new(solved.Grid, unsolved.Grid);
    }
}
