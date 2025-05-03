// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.Tests.GridReader;

using Straights.Image.GridReader;

public class SkewedGridCellTests
{
    [Fact]
    public void GetMaximumInscribedRect_WhenCalled_ReturnsExpected()
    {
        // ARRANGE
        var sut = new SkewedGridCell(
            TopLeft: new(1.3f, 1.3f),
            TopRight: new(8.3f, 2.5f),
            BottomLeft: new(0.8f, 21.0f),
            BottomRight: new(9.1f, 20.0f));

        _ = sut.GetMaximumInscribedRect().Should().Be(
            (2, 3, 7, 18));
    }
}
