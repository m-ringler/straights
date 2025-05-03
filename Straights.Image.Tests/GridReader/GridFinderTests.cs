// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.Tests.GridReader;

using Straights.Image.GridReader;

/// <summary>
/// Tests for <see cref="GridFinder"/>.
/// </summary>
public class GridFinderTests
{
    private IDebugInfoWriter DebugInfoWriter { get; set; } = Directory.Exists("/tmp/straights")
        ? new DebugInfoWriter("/tmp/straights")
        : new NullDebugInfoWriter();

    [Theory]
    [InlineData("GridReader/grid-numbers1.png")]
    [InlineData("GridReader/grid-numbers2.png")]
    [InlineData("GridReader/grid-numbers3.png")]
    [InlineData("GridReader/grid-numbers4.png")]
    [InlineData("GridReader/grid-numbers5.png", true)]

    public void FindGrid(string fileName, bool skewed = false)
    {
        // ARRANGE
        var png = TestData.GetPath(fileName);

        // ACT
        var grid = new GridFinder(this.DebugInfoWriter)
            .FindGrid(png);

        // ASSERT
        if (!skewed)
        {
            _ = grid.MedianCellSize.W.Should().BeApproximately(45, 1.0f);
            _ = grid.MedianCellSize.H.Should().BeApproximately(45, 1.0f);
        }

        _ = grid.VerticalLines.Length.Should().Be(10);
        _ = grid.HorizontalLines.Length.Should().Be(10);
        _ = grid.VerticalLines.Should().Equal(grid.VerticalLines.OrderBy(x => x.Rho));
        _ = grid.HorizontalLines.Should().Equal(grid.HorizontalLines.OrderBy(x => x.Rho));

        if (!skewed)
        {
            var v0 = grid.VerticalLines[0];
            var h0 = grid.HorizontalLines[0];
            var v = grid.VerticalLines;
            var h = grid.HorizontalLines;
            float cw = grid.MedianCellSize.W;
            float ch = grid.MedianCellSize.H;
            for (int i = 0; i <= 9; i++)
            {
                _ = v[i].Rho.Should().BeApproximately(v[0].Rho + (i * cw), 6.0f);
                _ = h[i].Rho.Should().BeApproximately(h[0].Rho + (i * ch), 6.0f);
                _ = v[i].Theta.Should().BeApproximately(0.0000f, 0.03f);
                _ = h[i].Theta.Should().BeApproximately(1.5708f, 0.03f);
            }
        }
    }
}