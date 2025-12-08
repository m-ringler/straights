// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.Tests.DigitReader;

using Straights.Image.DigitReader;

public class DigitClassifierTests
{
    [Theory]
    [InlineData(4)]
    [InlineData(9)]
    public void PredictPngBopd(int digit)
    {
        var path = TestData.GetPath($"DigitReader/b{digit}.png");
        using var classifier = new DigitClassifierOnnx("bekhzod-olimov-printed-digits");

        var result = classifier.PredictPng(path);
        _ = result.Should().Be(digit);
    }
}
