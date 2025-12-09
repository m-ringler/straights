// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Builder;

using Straights.Solver;
using Straights.Solver.Builder;
using Straights.Solver.Converter;
using Straights.Solver.Data;

/// <summary>
/// Tests for <see cref="GridBuilder"/>.
/// </summary>
public class GridBuilderTests
{
    [Fact]
    public void WhenAddNoFields_ToStringReturnsExpected()
    {
        var sut = new GridBuilder(3);
        _ = sut.ToString()
            .ReplaceLineEndings()
            .Should()
            .Be(
                """
                3
                _,_,_
                _,_,_
                _,_,_

                """.ReplaceLineEndings()
            );
    }

    [Fact]
    public void WhenAddSomeFields_ToStringReturnsExpected()
    {
        var sut = new GridBuilder(3);
        sut.SetBlack(2, 1, 3);
        sut.SetBlack(2, 2, 2);
        sut.SetWhite(1, 3, 3);
        sut.SetBlack(3, 1, 2);
        sut.SetBlack(3, 3);
        _ = sut.ToString()
            .ReplaceLineEndings()
            .Should()
            .Be(
                """
                3
                _,_,w3
                b3,b2,_
                b2,_,b

                """.ReplaceLineEndings()
            );
    }

    [Fact]
    public void WhenClear_ToStringReturnsExpected()
    {
        var sut = new GridBuilder(3);
        sut.SetBlack(2, 1, 3);
        sut.SetBlack(2, 2, 2);
        sut.SetWhite(1, 3, 3);
        sut.SetBlack(3, 1, 2);
        sut.SetBlack(3, 3);
        sut.Clear(1, 3);
        sut.Clear(3, 3);
        sut.Clear(2, 1);
        _ = sut.ToString()
            .ReplaceLineEndings()
            .Should()
            .Be(
                """
                3
                _,_,_
                _,b2,_
                b2,_,_

                """.ReplaceLineEndings()
            );
    }

    [Fact]
    public void AsBase64UrlString_ReturnsExpected()
    {
        const string grid = """
9
_,b,_,_,_,_,b,_,w7
b,_,_,_,w2,_,w1,_,_
b,_,_,_,_,b1,b8,_,b6
_,_,b,b8,_,_,_,_,_
_,b,_,_,w5,_,_,b,_
_,_,_,_,_,b9,b,_,_
b,_,b,b,_,w7,_,w4,b
w5,_,_,_,w9,w8,_,w3,b
w4,_,b,_,_,_,_,b9,_

""";
        var p = new GridBuilderTextPersister();
        var sut = p.Parse(grid);
        var unsolved = sut.Convert().SolverGrid;
        var solver = new EliminatingSolver();
        var solved = solver.Solve(unsolved);

        var binary = GridConverter.ToBinaryString(
            solved.Grid,
            unsolved.Grid,
            LuisWalterBinaryConverter.EncodingVersion
        );
        var base64 = Base64UrlEncoder.EncodeBase64Url(binary);

        _ = binary
            .Should()
            .Be(
                "00000010000101101111000001000000000010000011101111000111010110101111000110000011000010010001000100010000000101000111101111000100000010000001000011110000110111000110110101000110000101101111110111000000000001000010000100000011001000101111000101000110010100000010000011101111000001000111000011000100000101000110111000101111000001000010101111000000101111101111000101010110000100010011101111010100000001000110000011011000010111000101010010101111010011000010101111000100000111000101000110111000000000"
            );

        _ = base64
            .Should()
            .Be(
                "AhbwQAg7x1rxgwkREBR7xAgQ8NxtRhb9wAQhAyLxRlAg7wRwxBRuLwQrwL7xVhE71ARg2FxUr0wrxBxRuAA"
            );
    }

    [Fact]
    public void Gen4()
    {
        const string grid = """
9
_,b9,b,_,_,_,w1,w2,_
_,_,b,_,b,_,b9,_,_
w3,_,w1,b,_,_,_,b,_
_,b,w2,_,_,b,_,b,w7
w5,_,_,_,_,_,w8,_,_
_,_,b9,b1,b,b,_,_,_
b,_,w6,_,b9,_,_,_,b
_,_,b,_,_,w5,_,_,_
w8,_,_,_,w1,_,_,_,_

""";
        var p = new GridBuilderTextPersister();
        var sut = p.Parse(grid);
        var unsolved = sut.Convert().SolverGrid;
        var solver = new EliminatingSolver();
        var solved = solver.Solve(unsolved);

        var base64 = AsBase64UrlString(solved.Grid, unsolved.Grid);
        _ = base64
            .Should()
            .Be(
                "AhuLwwhUERAQLxLx-Ag0gULxBhbxxb0Qg7xL1lAwgRQFxiAxOML7xhxbxlR-AgQ7yB7xgVAxQlxRiEAwhAQ"
            );
    }

    private static string AsBase64UrlString(
        Grid<SolverField> solved,
        Grid<SolverField> unsolved
    )
    {
        return GridConverter.ToUrlParameter(
            solved,
            unsolved,
            LuisWalterBinaryConverter.EncodingVersion
        );
    }
}
