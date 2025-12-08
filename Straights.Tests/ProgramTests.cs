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
        RootCommand command = Program.Build(new MockFileSystem());
        var args = new[] { "--help" };

        // ACT
        var parseResult = command.Parse(args);
        using var outputWriter = new StringWriter();
        using var errorWriter = new StringWriter();
        parseResult.Configuration.Output = outputWriter;
        parseResult.Configuration.Error = errorWriter;
        int exitCode = parseResult.Invoke();
        var errorOutput = errorWriter.ToString();
        var stdOutput = outputWriter.ToString().Replace("testhost", "straights").Replace("Straights.Tests", "straights").TrimEnd();

        // ASSERT
        exitCode.Should().Be(0);
        errorOutput.Should().BeEmpty();
        return Verify(stdOutput);
    }
}
