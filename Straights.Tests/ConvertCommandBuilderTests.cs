// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.CommandLine;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

/// <summary>
/// Tests for <see cref="ConvertCommandBuilder"/>.
/// </summary>
public class ConvertCommandBuilderTests
{
    private readonly IFileSystem fileSystem = new MockFileSystem();
    private readonly ConsoleStub console = new();

    [Fact]
    public void BuildAndInvoke_WhenBothArgumentsProvided_ExecutesExpected()
    {
        // ARRANGE
        var runner = new RunCommandFunction<ConvertCommand>(5);
        var sut = new ConvertCommandBuilder(this.fileSystem, runner.Invoke);

        var args = new[] { XFS.Path(@"C:\Foo\input.json"), "--output", XFS.Path(@"C:\Foo\output.txt") };

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, this.console);
        var errorOutput = this.console.Error.Buffer.ToString();

        // ASSERT
        exitCode.Should().Be(5);
        var c = runner.InvokedCommand;
        c.Should().NotBeNull();
        c.OutputFile.FullName.Should().Be(args[2]);
        this.ShouldHaveCorrectFileSystem(c.OutputFile);
        c.InputFile.FullName.Should().Be(args[0]);
        this.ShouldHaveCorrectFileSystem(c.InputFile);
        errorOutput.Should().BeEmpty();
    }

    [Fact]
    public void BuildAndInvoke_WhenOutputOptionMissing_ReturnsError()
    {
        // ARRANGE
        var runner = new RunCommandFunction<ConvertCommand>(0);
        var sut = new ConvertCommandBuilder(this.fileSystem, runner.Invoke);

        var args = new[] { @"C:\Foo\input.json" };

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, this.console);
        var errorOutput = this.console.Error.Buffer.ToString();

        // ASSERT
        exitCode.Should().NotBe(0);
        runner.InvokedCommand.Should().BeNull();
        errorOutput.Trim().Should().Be("Option '--output' is required.");
    }

    [Fact]
    public void BuildAndInvoke_WhenFileArgumentMissing_ReturnsError()
    {
        // ARRANGE
        var runner = new RunCommandFunction<ConvertCommand>(0);
        var sut = new ConvertCommandBuilder(this.fileSystem, runner.Invoke);

        var args = new[] { "--output", @"C:\Foo\output.txt" };

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, this.console);
        var errorOutput = this.console.Error.Buffer.ToString();

        // ASSERT
        exitCode.Should().NotBe(0);
        runner.InvokedCommand.Should().BeNull();
        errorOutput.Trim().Should().Be("Required argument missing for command: 'convert'.");
    }

    [Fact]
    public Task Help()
    {
        return HelpVerifier.VerifyHelp<ConvertCommand>(
            execute =>
                new ConvertCommandBuilder(new MockFileSystem(), execute));
    }

    private void ShouldHaveCorrectFileSystem(IFileInfo f)
    {
        f.Should().BeOfType<FileInfoWrapper>().Which.FileSystem.Should().BeSameAs(this.fileSystem);
    }
}