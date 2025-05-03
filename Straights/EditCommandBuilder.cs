// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;

internal sealed class EditCommandBuilder(
        IFileSystem fs,
        Func<EditCommand, int> execute)
    : ICommandBuilder
{
    private readonly Argument<FileInfo?> fileArgument = new(
        name: "imageOrTextFile",
        description: "An image file with a Straights grid, or a Straights grid saved as .txt or .json.",
        getDefaultValue: () => null);

    public EditCommandBuilder(
        IFileSystem fs)
        : this(fs, program => program.Run())
    {
    }

    public Command Build()
    {
        var command = new Command("edit", "Edits a straights puzzle");

        command.AddArgument(this.fileArgument);

        command.SetHandler(this.RunProgram);

        return command;
    }

    private void RunProgram(InvocationContext context)
    {
        var pr = context.ParseResult;

        var file = pr.GetValueForArgument(this.fileArgument);

        var program = new EditCommand(fs)
        {
            File = fs.Wrap(file),
        };

        int returnCode = execute(program);
        context.ExitCode = returnCode;
    }
}
