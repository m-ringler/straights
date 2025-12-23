// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

internal static class HelpVerifier
{
    public static Task VerifyHelp<TCommand>(
        Func<Func<TCommand, int>, ICommandBuilder> builderFactory
    )
    {
        // ARRANGE
        var runner = new RunCommandFunction<TCommand>(0);
        var sut = builderFactory(runner.Invoke);

        var args = new[] { "--help" };

        // ACT
        var command = sut.Build();
        var pr = command.Parse(args);
        using var outputWriter = new StringWriter();
        using var errorWriter = new StringWriter();
        pr.InvocationConfiguration.Output = outputWriter;
        pr.InvocationConfiguration.Error = errorWriter;
        int exitCode = pr.Invoke();
        var errorOutput = errorWriter.ToString();
        var stdOutput = outputWriter.ToString();

        // ASSERT
        exitCode.Should().Be(0);
        runner.InvokedCommand.Should().BeNull();
        errorOutput.Should().BeEmpty();
        return Verify(stdOutput.TrimEnd());
    }
}
