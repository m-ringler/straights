// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.CommandLine;
using System.CommandLine.Help;
using System.IO.Abstractions;

internal sealed class ConvertCommandBuilder : ICommandBuilder
{
    private const string FileArgumentDescription = """
An image file with a Straights grid, or a Straights grid saved as .txt or .json.
""";

    private const string OutputOptionDescription = """
        Write the generated grid to the specified file.
        Currently the following formats are supported:
        * HTML (.htm, .html)
        * TEXT (.txt)
        * JSON (.json)
        More formats may be supported in the future.
        """;

    private readonly IFileSystem fs;
    private readonly Func<ConvertCommand, int> runCommand;

    private readonly Option<FileInfo> outputOption = new("--output")
    {
        Description = OutputOptionDescription,
        Required = true,
        HelpName = "file",
    };

    private readonly Argument<FileInfo> fileArgument = new("imageOrTextFile")
    {
        Description = FileArgumentDescription,
    };

    public ConvertCommandBuilder(
        IFileSystem fs,
        Func<ConvertCommand, int> runCommand
    )
    {
        this.fs = fs;
        this.runCommand = runCommand;
        _ = this.outputOption.AcceptLegalFilePathsOnly();
        _ = this.fileArgument.AcceptLegalFilePathsOnly();
    }

    public ConvertCommandBuilder(IFileSystem fs)
        : this(fs, program => program.Run()) { }

    public Command Build()
    {
        var command = new Command("convert", "Converts a straights puzzle")
        {
            this.outputOption,
            this.fileArgument,
            new HelpOption(),
        };

        command.SetAction(this.RunProgram);

        return command;
    }

    private int RunProgram(ParseResult pr)
    {
        var file = this.fs.Wrap(pr.GetRequiredValue(this.fileArgument));

        var program = new ConvertCommand
        {
            OutputFile = pr.GetValue(this.outputOption)!.Wrap(this.fs),
            InputFile = file!,
        };

        int returnCode = this.runCommand(program);
        return returnCode;
    }
}
