// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using Straights.Solver.Builder;

using static Straights.InteractiveGridEditor;

/// <summary>
/// Tests for <see cref="InteractiveGridEditor"/>.
/// </summary>
public class GridEditorTests
{
    [Fact]
    public void TryParse_WhenTwoDigits_ReturnsExpected()
    {
        const string input = "3 4";
        _ = TryParseBuilderField(input, out var result).Should().BeTrue();

        _ = result.Should().Be(new BuilderField(new FieldLocation(4, 3)));
    }

    [Fact]
    public void TryParse_WhenThreeDigits_ReturnsExpected()
    {
        const string input = "3 4 5";
        _ = TryParseBuilderField(input, out var result).Should().BeTrue();

        _ = result.Should().Be(new BuilderField(new FieldLocation(4, 3), 5));
    }

    [Fact]
    public void TryParse_WhenWhiteField_ReturnsExpected()
    {
        const string input = "3 4 w5";
        _ = TryParseBuilderField(input, out var result).Should().BeTrue();

        _ = result.Should().Be(new BuilderField(new FieldLocation(4, 3), 5) { IsWhite = true });
    }
}
