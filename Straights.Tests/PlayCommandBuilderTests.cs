// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.CommandLine;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

using Straights.Console;
using Straights.Play;

using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

/// <summary>
/// Tests for <see cref="PlayCommandBuilder"/>.
/// </summary>
public class PlayCommandBuilderTests
{
    [Theory]
    [InlineData(new string[0], null)]
    [InlineData(new[] { "--offline-port", "1234" }, 1234)]
    [InlineData(new[] { "--offline", "--offline-port", "1234" }, 1234)]
    [InlineData(new[] { "--offline" }, 7138)]
    public void BuildAndInvoke_WhenFileProvided_ExecutesExpected(
        string[] offlineTokens,
        int? expectedPort)
    {
        // ARRANGE
        var fileSystem = FileSystem();
        var console = Console();
        var runner = new RunCommandFunction<PlayCommand>(5);
        var sut = new PlayCommandBuilder(fileSystem, runner.Invoke);

        string[] args = [
            XFS.Path(@"C:\Foo\input.json"),
            .. offlineTokens,
        ];

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, console);
        var errorOutput = console.Error.Buffer.ToString();

        // ASSERT
        errorOutput.Should().BeEmpty();
        exitCode.Should().Be(5);
        var c = runner.InvokedCommand;
        c.Should().NotBeNull();
        c.PortOnLocalHost.Should().Be(expectedPort);
        c.WebApp.Should().BeOfType<WebApp>();
        c.Terminal.Should().BeOfType<Terminal>();
        (c.InputFile?.FullName).Should().Be(args[0]);
        ShouldHaveFileSystem(c.InputFile, fileSystem);
    }

    [Fact]
    public void BuildAndInvoke_WhenFileArgumentMissing_NoError()
    {
        // ARRANGE
        var fileSystem = FileSystem();
        var console = Console();
        var runner = new RunCommandFunction<PlayCommand>(0);
        var sut = new PlayCommandBuilder(fileSystem, runner.Invoke);

        string[] args = [];

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, console);
        var errorOutput = console.Error.Buffer.ToString();

        // ASSERT
        errorOutput.Should().BeEmpty();
        exitCode.Should().Be(0);
        var c = runner.InvokedCommand;
        c.Should().NotBeNull();
        (c.InputFile?.FullName).Should().BeNull();
        c.PortOnLocalHost.Should().BeNull();
    }

    [Fact]
    public Task Help()
    {
        return HelpVerifier.VerifyHelp<PlayCommand>(
            execute =>
                new PlayCommandBuilder(new MockFileSystem(), execute));
    }

    private static void ShouldHaveFileSystem(IFileInfo f, IFileSystem fileSystem)
    {
        f.Should().BeOfType<FileInfoWrapper>().Which.FileSystem.Should().BeSameAs(fileSystem);
    }

    private static IFileSystem FileSystem()
    {
        return new MockFileSystem();
    }

    private static ConsoleStub Console()
    {
        return new();
    }
}
