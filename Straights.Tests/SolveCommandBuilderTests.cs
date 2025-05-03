// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.CommandLine;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

/// <summary>
/// Tests for <see cref="SolveCommandBuilder"/>.
/// </summary>
public class SolveCommandBuilderTests
{
    [Fact]
    public void BuildAndInvoke_WhenFileProvided_ExecutesExpected()
    {
        // ARRANGE
        var fileSystem = FileSystem();
        var console = Console();
        var runner = new RunCommandFunction<SolveCommand>(5);
        var sut = new SolveCommandBuilder(fileSystem, runner.Invoke);

        string[] args = [
            XFS.Path(@"C:\Foo\input.json"),
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
        (c.File?.FullName).Should().Be(args[0]);
        ShouldHaveFileSystem(c.File, fileSystem);
    }

    [Fact]
    public void BuildAndInvoke_WhenFileArgumentMissing_Error()
    {
        // ARRANGE
        var fileSystem = FileSystem();
        var console = Console();
        var runner = new RunCommandFunction<SolveCommand>(0);
        var sut = new SolveCommandBuilder(fileSystem, runner.Invoke);

        string[] args = [];

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, console);
        var errorOutput = console.Error.Buffer.ToString();

        // ASSERT
        errorOutput.Trim().Should().Be("You must either provide an imageOrTextFile or use --interactive.");
        exitCode.Should().NotBe(0);
        runner.InvokedCommand.Should().BeNull();
    }

    [Fact]
    public Task

#if DEBUG
    Help_Debug()
#else
    Help_Release()
#endif
    {
        return HelpVerifier.VerifyHelp<SolveCommand>(
            execute =>
                new SolveCommandBuilder(new MockFileSystem(), execute));
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
