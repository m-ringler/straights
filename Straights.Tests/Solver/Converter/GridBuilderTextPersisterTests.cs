// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Converter;

using Straights.Solver.Builder;
using Straights.Solver.Converter;

/// <summary>
/// Tests for <see cref="GridBuilderTextPersister"/>.
/// </summary>
public class GridBuilderTextPersisterTests
{
    [Fact]
    public void WhenSaveAndLoad_ToStringReturnsTheSame()
    {
        // ARRANGE
        var grid = new GridBuilder(3);
        grid.SetBlack(2, 1, 3);
        grid.SetBlack(2, 2, 2);
        grid.SetWhite(1, 3, 3);
        grid.SetBlack(3, 1, 2);
        grid.SetBlack(3, 3);

        // ACT
        using var writer = new StringWriter();
        var sut = new GridBuilderTextPersister();
        sut.Save(grid, writer);

        var saved = writer.ToString();

        using var reader = new StringReader(saved);
        sut = new GridBuilderTextPersister();
        grid = sut.Load(reader);

        // ASSERT
        _ = grid.ToString().ReplaceLineEndings().Should().Be(
            """
            3
            _,_,w3
            b3,b2,_
            b2,_,b

            """.ReplaceLineEndings());
    }

    [Fact]
    public void WhenSave_ToStringReturnsExpected()
    {
        // ARRANGE
        var grid = new GridBuilder(3);
        grid.SetBlack(2, 1, 3);
        grid.SetBlack(2, 2, 2);
        grid.SetWhite(1, 3, 3);
        grid.SetBlack(3, 1, 2);
        grid.SetBlack(3, 3);

        var sut = new GridBuilderTextPersister();

        // ACT
        using var writer = new StringWriter();
        sut.Save(grid, writer);

        // ASSERT
        var saved = writer.ToString();
        _ = saved.ReplaceLineEndings().Should().Be(
"""
3
_,_,w3
b3,b2,_
b2,_,b

""".ReplaceLineEndings());
    }
}
