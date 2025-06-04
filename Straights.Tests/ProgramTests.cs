// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.CommandLine;
using System.IO.Abstractions.TestingHelpers;

/// <summary>
/// Tests for <see cref="Program"/>.
/// </summary>
public class ProgramTests
{
    [Fact]
    public Task Help()
    {
        // ARRANGE
        ConsoleStub console = new();
        RootCommand command = Program.Build(new MockFileSystem());

        var args = new[] { "--help" };

        // ACT
        int exitCode = command.Invoke(args, console);
        var errorOutput = console.Error.Buffer.ToString();
        console.Out.Buffer.Replace("testhost", "straights");
        console.Out.Buffer.Replace("Straights.Tests", "straights");
        console.Out.Buffer.TrimEnd();
        var stdOutput = console.Out.Buffer.ToString();

        // ASSERT
        exitCode.Should().Be(0);
        errorOutput.Should().BeEmpty();
        return Verify(stdOutput);
    }
}