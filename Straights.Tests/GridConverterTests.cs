// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using Straights.Solver;
using Straights.Solver.Converter;
using Straights.Solver.Simplification;

public class GridConverterTests
{
    private const string BuilderText5 =
"""
5
_,_,w3,_,b
w1,b,_,_,_
b,_,b1,_,w4
w4,_,_,w2,_
b,_,_,_,_

""";

    private const string BuilderText9 =
"""
9
b,b9,_,_,_,b,_,b,_
_,b,w8,_,b,_,_,_,_
_,w6,b9,b,_,b,_,w2,_
_,b,_,_,_,_,_,w1,b
_,_,b,_,w5,_,_,b9,w8
w4,w5,_,_,w1,w6,_,b,_
b9,w3,_,_,_,_,_,_,_
_,_,_,w2,b8,b,_,w5,_
_,_,b,_,w7,_,_,w4,_

""";

    private const string Json5 =
"""
[
  [
    [1, 2, 3, 4, 5],
    [1, 2, 3, 4, 5],
    [3],
    [1, 2, 3, 4, 5],
    [0]
  ],
  [
    [1],
    [0],
    [1, 2, 3, 4, 5],
    [1, 2, 3, 4, 5],
    [1, 2, 3, 4, 5]
  ],
  [
    [0],
    [1, 2, 3, 4, 5],
    [-1],
    [1, 2, 3, 4, 5],
    [4]
  ],
  [
    [4],
    [1, 2, 3, 4, 5],
    [1, 2, 3, 4, 5],
    [2],
    [1, 2, 3, 4, 5]
  ],
  [
    [0],
    [1, 2, 3, 4, 5],
    [1, 2, 3, 4, 5],
    [1, 2, 3, 4, 5],
    [1, 2, 3, 4, 5]
  ]
]

""";

    private const string Json5Compact =
"""
[
  [[ ], [ ], [3],  [ ], [0]],
  [[1], [0], [ ],  [ ], [ ]],
  [[0], [ ], [-1], [ ], [4]],
  [[4], [ ], [ ],  [2], [ ]],
  [[0], [ ], [ ],  [ ], [ ]]
]

""";

    [Fact]
    public void BuilderToJson()
    {
        var json = GridConverter
            .ParseBuilderText(BuilderText5)
            .ToJson();

        _ = json.Should().Be(Json5);
    }

    [Fact]
    public void SolveCompactJson()
    {
        var actual = SolveJsonGrid(Json5Compact);

        _ = actual.Should().Be(
"""
[
  [[2], [4], [3], [1], [0]],
  [[1], [0], [2], [4], [3]],
  [[0], [2], [-1], [5], [4]],
  [[4], [3], [5], [2], [1]],
  [[0], [1], [4], [3], [2]]
]

""");
    }

    [Fact]
    public void BuilderToLuisWalter()
    {
        var unsolved = GridConverter
            .ParseBuilderText(BuilderText9)
            .SolverGrid;

        var solved = GridSimplifierFactory
            .BuildIterativeSimplifier(SimplifierStrength.DefaultStrength)
            .ToSolver()
            .Solve(unsolved);

        var actual = GridConverter.ToUrlParameter(
            unsolved: unsolved.Grid,
            solved: solved.Grid,
            encodingVersion: LuisWalterBinaryConverter.EncodingVersion);

        _ = actual.Should().Be(
            "Ar-BhxbwrwRb1yLwRAgxFeLw7wEQhrxQwhAULwgbxVBg-" +
            "F01AQkFRryOEgBgQxxRAAwkd7xVBgQLxFhyExQ");
    }

    [Fact]
    public void JsonToBuilder()
    {
        var actual = GridConverter.ParseJson(Json5)
            .ToBuilderText();

        _ = actual.Should().Be(BuilderText5);
    }

    private static string SolveJsonGrid(string input)
    {
        var solver = new EliminatingSolver();
        var unsolved = GridConverter.ParseJson(input).SolverGrid;
        var solved = solver.Solve(unsolved);
        return solved.Convert().ToJson();
    }
}
