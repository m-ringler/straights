// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;

internal sealed class ConvertCommandBuilder(
        IFileSystem fs,
        Func<ConvertCommand, int> runCommand)
    : ICommandBuilder
{
    private const string FileArgumentDescription =
"""
An image file with a Straights grid, or a Straights grid saved as .txt or .json.
""";

    private const string OutputOptionDescription =
        """
        Write the generated grid to the specified file.
        Currently the following formats are supported:
        * HTML (.htm, .html)
        * TEXT (.txt)
        * JSON (.json)
        More formats may be supported in the future.
        """;

    private readonly Option<FileInfo> outputOption = new(
        name: "--output",
        description: OutputOptionDescription)
    {
        IsRequired = true,
    };

    private readonly Argument<FileInfo> fileArgument = new(
        name: "imageOrTextFile",
        description: FileArgumentDescription);

    public ConvertCommandBuilder(IFileSystem fs)
        : this(fs, program => program.Run())
    {
    }

    public Command Build()
    {
        var command = new Command("convert", "Converts a straights puzzle")
        {
            this.outputOption,
            this.fileArgument,
        };

        command.SetHandler(this.RunProgram);

        return command;
    }

    private void RunProgram(InvocationContext context)
    {
        var pr = context.ParseResult;

        var program = new ConvertCommand
        {
            OutputFile = pr.GetValueForOption(this.outputOption)!.Wrap(fs),
            InputFile = pr.GetValueForArgument(this.fileArgument).Wrap(fs),
        };

        int returnCode = runCommand(program);
        context.ExitCode = returnCode;
    }
}
