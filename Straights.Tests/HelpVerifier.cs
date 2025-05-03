// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.CommandLine;

internal static class HelpVerifier
{
    public static Task VerifyHelp<TCommand>(
        Func<Func<TCommand, int>, ICommandBuilder> builderFactory)
    {
        // ARRANGE
        ConsoleStub console = new();
        var runner = new RunCommandFunction<TCommand>(0);
        var sut = builderFactory(runner.Invoke);

        var args = new[] { "--help" };

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, console);
        var errorOutput = console.Error.Buffer.ToString();
        var stdOutput = console.Out.Buffer.ToString();

        // ASSERT
        exitCode.Should().Be(0);
        runner.InvokedCommand.Should().BeNull();
        errorOutput.Should().BeEmpty();
        return Verify(stdOutput.TrimEnd());
    }
}
