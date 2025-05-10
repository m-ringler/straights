// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.IO.Abstractions.TestingHelpers;
using System.Runtime.CompilerServices;

using Straights.Solver;
using Straights.Solver.Builder;
using Straights.Tests.Console;

using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

/// <summary>
/// Tests for <see cref="InteractiveGridSaver"/>.
/// </summary>
public class InteractiveGridSaverTests
{
    private const string GridAsText =
"""
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

    private static GridBuilder Grid => GridConverter.ParseBuilderText(GridAsText).Builder;

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task DoNotSave(bool withSuggestedPath)
    {
        // ARRANGE
        IReadOnlyList<string> userInput = [
                "A",
                " foo ",
                "xy",
                "N",
                " n\n",
            ];

        var sut = CreateSut(userInput, out var fs, out var getConsoleOutput);
        var fsBefore = fs.AllNodes.OrderBy(x => x).ToList();

        // ACT
        sut.SaveGrid(
            Grid,
            withSuggestedPath ? XFS.Path(@"C:\output\foo.txt") : null);

        // ASSERT
        var fsAfter = fs.AllNodes.OrderBy(x => x).ToList();
        fsAfter.Should().Equal(fsBefore);
        return Verify(getConsoleOutput()).DisableRequireUniquePrefix();
    }

    [Fact]
    public Task AcceptSuggestedPath1()
    {
        string userPathAnswer = "\n";
        return AcceptSuggestedPath(userPathAnswer);
    }

    [Fact]
    public Task AcceptSuggestedPath2()
    {
        return AcceptSuggestedPath(null);
    }

    [Fact]
    public Task NoPath()
    {
        // ARRANGE
        IReadOnlyList<string> userInput = [
            "y",
            "\n",
        ];

        var sut = CreateSut(userInput, out var fs, out var getConsoleOutput);
        var fsBefore = fs.AllNodes.OrderBy(x => x).ToList();

        // ACT
        sut.SaveGrid(
            Grid,
            null);

        // ASSERT
        var fsAfter = fs.AllNodes.OrderBy(x => x).ToList();
        fsAfter.Should().Equal(fsBefore);
        return Verify(getConsoleOutput());
    }

    [Theory]
    [InlineData(true, "txt")]
    [InlineData(false, "html")]
    [InlineData(true, "json")]
    [InlineData(false, "x", "txt")]
    public Task UserPath(bool withSuggestedPath, string extension, string? expectedExtension = null)
    {
        // ARRANGE
        var suggestedPath = withSuggestedPath ? XFS.Path(@"c:\path\to\foo.txt") : null;
        var userPathAnswer = XFS.Path(@"c:\user\wants\bar." + extension);
        var expectedPath = XFS.Path(@"c:\user\wants\bar." + extension +
            (expectedExtension == null ? null : $".{expectedExtension}"));
        IReadOnlyList<string?> userInput = [
            "y",
            userPathAnswer,
        ];

        var sut = CreateSut(userInput, out var fs, out var getConsoleOutput);
        var fsBefore = fs.AllNodes.ToList();

        // ACT
        sut.SaveGrid(
            Grid,
            suggestedPath);

        // ASSERT
        var addedNodes = fs.AllNodes.OrderBy(x => x).Except(fsBefore).Should().Equal([
            expectedPath]);
        bool isUnix = XFS.IsUnixPlatform();
        return Verify(getConsoleOutput())
        .UseFileName(
                $"{nameof(InteractiveGridSaverTests)}.{nameof(this.UserPath)}.{extension}.IsUnix={isUnix}")
        .AppendContentAsFile(fs.File.ReadAllText(expectedPath), expectedExtension ?? extension);
    }

    [Fact]
    public void UserPathWithTilde()
    {
        // ARRANGE
        var userPathAnswer = XFS.Path(@"~/wants/bar.txt");
        var home = XFS.Path(@"c:\users\foo");
        var expectedPath = XFS.Path(@"c:\users\foo\wants\bar.txt");
        IReadOnlyList<string?> userInput = [
            "y",
            userPathAnswer,
        ];

        var sut = CreateSut(userInput, out var fs, out _, home);
        var fsBefore = fs.AllNodes.ToList();

        // ACT
        sut.SaveGrid(
            Grid,
            null);

        // ASSERT
        var addedNodes = fs.AllNodes.OrderBy(x => x).Except(fsBefore).Should().Equal([
            expectedPath]);
        fs.File.ReadAllText(expectedPath).Should().Be(GridAsText);
    }

    private static InteractiveGridSaver CreateSut(
        IReadOnlyList<string?> userInput,
        out MockFileSystem fs,
        out Func<string> getConsoleOutput,
        string? homeFolder = null)
    {
        fs = new(new MockFileSystemOptions { CreateDefaultTempDir = false, });
        var console = new StringBuilderConsole();
        getConsoleOutput = console.ToString;
        using var e = userInput.GetEnumerator();
        string? ReadNextLine()
        {
            var result = e.MoveNext() ? e.Current : string.Empty;
            console.Write(result ?? "<null>");
            console.WriteLine();
            return result;
        }

        var sut = new InteractiveGridSaver(fs, new(console, ReadNextLine), homeFolder);
        return sut;
    }

    private static Task AcceptSuggestedPath(
        string? userPathAnswer,
        [CallerMemberName] string? testMethod = null)
    {
        // ARRANGE
        var suggestedPath = XFS.Path(@"c:\path\to\foo.txt");
        IReadOnlyList<string?> userInput = [
            "y",
            userPathAnswer,
        ];

        var sut = CreateSut(userInput, out var fs, out var getConsoleOutput);
        var fsBefore = fs.AllNodes.ToList();

        // ACT
        sut.SaveGrid(
            Grid,
            suggestedPath);

        // ASSERT
        var addedNodes = fs.AllNodes.OrderBy(x => x).Except(fsBefore).Should().Equal([
            suggestedPath]);

        fs.File.ReadAllText(suggestedPath).Should().Be(GridAsText);
        bool isUnix = XFS.IsUnixPlatform();
        return Verify(getConsoleOutput())
            .UseFileName(
                $"{nameof(InteractiveGridSaverTests)}.{testMethod}.IsUnix={isUnix}");
    }
}
