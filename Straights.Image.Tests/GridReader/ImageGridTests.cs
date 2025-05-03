// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.Tests.GridReader;

using System.Collections.Immutable;

using Straights.Image.GridReader;

public class ImageGridTests
{
    [Fact]
    public void GetCell_WhenCalled_ReturnsExpected()
    {
        // Arrange
        float thetaH = (float)(0.5 * Math.PI);
        float thetaV = 0;
        var linesH = Enumerable.Range(0, 10).Select(i => ((float)i, thetaH)).ToImmutableArray();
        var linesV = Enumerable.Range(0, 10).Select(i => ((float)i, thetaV)).ToImmutableArray();
        var grid = new ImageGrid
        {
            MedianCellSize = (1.0f, 1.0f),
            VerticalLines = linesV,
            HorizontalLines = linesH,
        };

        // Act & Assert
        _ = grid.GetCell(4, 3).Should().Be(
            new SkewedGridCell(
                new(4, 3),
                new(5, 3),
                new(5, 4),
                new(4, 4)));

        _ = grid.GetCell(0, 0).Should().Be(
            new SkewedGridCell(
                new(0, 0),
                new(1, 0),
                new(1, 1),
                new(0, 1)));

        _ = grid.GetCell(grid.NumCellX - 1, grid.NumCellY - 1).Should().Be(
            new SkewedGridCell(
                new(8, 8),
                new(9, 8),
                new(9, 9),
                new(8, 9)));
    }
}
