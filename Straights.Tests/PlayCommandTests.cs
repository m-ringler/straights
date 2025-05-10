// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

using Moq;

using Straights.Play;
using Straights.Tests.Console;

using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

/// <summary>
/// Tests for <see cref="PlayCommand"/>.
/// </summary>
public class PlayCommandTests
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

    [Fact]
    public Task OnlineNoInput()
    {
        // ARRANGE
        var sut = CreateSut(out var getConsoleOutput);

        // ACT
        var exitCode = sut.Run();

        // ASSERT
        exitCode.Should().Be(0);
        return Verify(getConsoleOutput());
    }

    [Fact]
    public Task OnlineWithInput()
    {
        // ARRANGE
        var sut = CreateSut(out var getConsoleOutput, XFS.Path(@"C:\foo\grid.txt"));

        // ACT
        var exitCode = sut.Run();

        // ASSERT
        exitCode.Should().Be(0);
        return Verify(getConsoleOutput());
    }

    [Fact]
    public Task OfflineNoInput()
    {
        // ARRANGE
        var sut = CreateSut(out var getConsoleOutput, portOnLocalHost: 2114);

        // ACT
        var exitCode = sut.Run();

        // ASSERT
        exitCode.Should().Be(0);
        return Verify(getConsoleOutput());
    }

    [Fact]
    public Task OfflineWithInput()
    {
        // ARRANGE
        var sut = CreateSut(out var getConsoleOutput, XFS.Path(@"C:\foo\grid.txt"), 3556);

        // ACT
        var exitCode = sut.Run();

        // ASSERT
        exitCode.Should().Be(0);
        return Verify(getConsoleOutput());
    }

    private static PlayCommand CreateSut(
        out Func<string> getConsoleOutput,
        string? inputFile = null,
        int? portOnLocalHost = null,
        Task? webAppTask = null)
    {
        var fs = new MockFileSystem();
        if (inputFile != null)
        {
            fs.AddFile(
                inputFile,
                new MockFileData(GridAsText));
        }

        var console = new StringBuilderConsole();
        getConsoleOutput = console.ToString;

        var webApp = new Mock<IWebApp>();
        webApp
            .Setup(x => x.Run(It.IsAny<string>(), It.IsAny<IDirectoryInfo>()))
            .Returns<string, IDirectoryInfo>(
                (url, folder) =>
                {
                    console.Write($"WebApp serving {folder} at {url}");
                    console.WriteLine();
                    return webAppTask ?? Task.CompletedTask;
                });

        var browserLauncher = new Mock<IBrowserLauncher>();
        browserLauncher
            .Setup(x => x.OpenBrowser(It.IsAny<string>()))
            .Callback<string>(url =>
            {
                console.Write($"Opening browser at {url}");
                console.WriteLine();
            });

        var sut = new PlayCommand(browserLauncher.Object, fs)
        {
            PortOnLocalHost = portOnLocalHost,
            InputFile = inputFile == null ? null : fs.FileInfo.New(inputFile),
            WebApp = webApp.Object,
            Terminal = console,
        };

        return sut;
    }
}
