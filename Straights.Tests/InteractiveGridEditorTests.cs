// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using Straights.Console;
using Straights.Solver;
using Straights.Solver.Builder;
using Straights.Tests.Console;
using static Straights.InteractiveGridEditor;

/// <summary>
/// Tests for <see cref="InteractiveGridEditor"/>.
/// </summary>
public class InteractiveGridEditorTests
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

    [Theory]
    [InlineData("3 4 w5")]
    [InlineData("3 4 w5 ")]
    public void TryParse_WhenWhiteField_ReturnsExpected(string input)
    {
        _ = TryParseBuilderField(input, out var result).Should().BeTrue();

        _ = result
            .Should()
            .Be(
                new BuilderField(new FieldLocation(4, 3), 5) { IsWhite = true }
            );
    }

    [Theory]
    [InlineData("3 4 x5")]
    [InlineData("3 4 5 6")]
    [InlineData("-3 4 5")]
    [InlineData("1")]
    public void TryParse_WhenNoMatch_ReturnsFalseNull(string input)
    {
        _ = TryParseBuilderField(input, out var result).Should().BeFalse();

        _ = result.Should().BeNull();
    }

    [Fact]
    public async Task EditingSequenceReturnsExpected()
    {
        // ARRANGE
        var builder = new GridBuilder(9);
        IReadOnlyList<string> lines =
        [
            "1 3", // black blank
            "2 3 3", // black number
            "7 4 w5",
            "7 4 0", // clear
            "7 5 w7",
            "7 5 w8", // overwrite
            "2 3 5",
            "2 4 5", // conflict
            "a b c", // invalid
            "7 7 w6", // white number
            "9 1",
        ];

        IWriteOnlyConsole stringConsole = new StringBuilderConsole();
        using var e = lines.GetEnumerator();
        string ReadNextLine()
        {
            var result = e.MoveNext() ? e.Current : string.Empty;
            stringConsole.WriteLine(result);
            return result;
        }

        var editor = new InteractiveGridEditor(
            new(stringConsole, ReadNextLine)
        );
        var result = editor.Edit(builder);
        _ = result.Should().BeTrue();
        await Verify(builder.Convert().ToBuilderText())
            .UseFileName($"{nameof(InteractiveGridEditorTests)}.Builder");
        await Verify(stringConsole.ToString())
            .UseFileName($"{nameof(InteractiveGridEditorTests)}.Console");
    }
}
