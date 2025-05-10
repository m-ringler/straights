// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.IO.Abstractions.TestingHelpers;

using Straights.Tests.Console;

using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

/// <summary>
/// Tests for <see cref="EditCommand"/>.
/// </summary>
public class EditCommandTests
{
    private const string Grid4x4 =
        """
        4
        _,b,w3,b
        _,_,_,w1
        _,w2,_,_
        w3,_,_,_
        """;

    [Fact]
    public Task File()
    {
        var inputPath = XFS.Path(@"x:\grid.txt");
        var outputPath = XFS.Path(@"C:\foo\grid.txt");
        IReadOnlyList<string> userInput = [
            "4 1 w4",
            string.Empty,
            "y",
            outputPath,
        ];

        var fs = new MockFileSystem();
        fs.AddFile(inputPath, new MockFileData(Grid4x4));
        var sut = CreateSut(
            userInput.GetEnumerator(),
            fs,
            out var getConsoleOutput,
            inputPath);

        // ACT
        var exitCode = sut.Run();

        // ASSERT
        exitCode.Should().Be(0);
        return
            Verify(getConsoleOutput())
            .UseFileName($"{nameof(EditCommandTests)}.{nameof(this.File)}.IsUnix={XFS.IsUnixPlatform()}")
            .AppendContentAsFile(fs.File.ReadAllText(outputPath));
    }

    [Fact]
    public Task NoFile()
    {
        var outputPath = XFS.Path(@"C:\foo\grid.txt");
        IReadOnlyList<string> userInput = [
            "4",
            "1 1 w2",
            "1 3",
            "2 1 w3",
            "3 2 w1",
            "3 4 w3",
            "3 3",
            string.Empty,
            "y",
            XFS.Path(@"C:\foo\grid.txt"),
        ];

        var fs = new MockFileSystem();
        var sut = CreateSut(
            userInput.GetEnumerator(),
            fs,
            out var getConsoleOutput);

        // ACT
        var exitCode = sut.Run();

        // ASSERT
        exitCode.Should().Be(0);
        return
            Verify(getConsoleOutput())
            .UseFileName($"{nameof(EditCommandTests)}.{nameof(this.NoFile)}.IsUnix={XFS.IsUnixPlatform()}")
            .AppendContentAsFile(fs.File.ReadAllText(outputPath));
    }

    private static EditCommand CreateSut(
        IEnumerator<string> userInput,
        MockFileSystem fs,
        out Func<string> getConsoleOutput,
        string? fileName = null)
    {
        var console = new StringBuilderConsole();
        getConsoleOutput = console.ToString;

        var e = userInput;
        string? ReadNextLine()
        {
            var result = e.MoveNext() ? e.Current : string.Empty;
            console.Write(result ?? "<null>");
            console.WriteLine();
            return result;
        }

        var sut = new EditCommand(fs)
        {
            File = fileName == null ? null : fs.FileInfo.New(fileName),
            ReadLine = ReadNextLine,
            Terminal = console,
        };

        return sut;
    }
}