// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO.Abstractions;

using Mode = SolveCommand.SolverMode;

internal sealed class SolveCommandBuilder(
        IFileSystem fs,
        Func<SolveCommand, int> execute)
    : ICommandBuilder
{
    private const string FileArgumentDescription =
        """
        An image file with a Straights grid, or a Straights grid saved as .txt or .json.
        When this argument is omitted, the grid editor is started.

        """;

    private const string DebugDataOptionDescription =
        """
        When this option is provided, debug data is written to the specified directory.
        Existing data in the directory will be overwritten without warning.

        """;

    private const string OutputOptionDescription =
        """
        Write the final grid (after solving) to the specified file.
        Currently the following formats are supported:
        * HTML (.htm, .html)
        * JSON (.json)
        More formats may be supported in the future.

        """;

    private const string ModeOptionDescription =
        $"""
        The solver mode to use.
        {nameof(Mode.SimplifyOnly)}:   reason about single rows and columns.
        {nameof(Mode.UniqueSolution)}: find a unique solution.
        {nameof(Mode.AnySolution)}:    find any solution (even if it is not unique).
        In {nameof(Mode.AnySolution)} mode, the solver uses random numbers,
        and results may vary from run to run.

        """;

    private readonly Option<bool> multiThreadedOption = new(
        name: "--multi-threaded",
        getDefaultValue: () => false,
        description: "Flag that turns on multi-threading.")
    {
        Arity = ArgumentArity.Zero,
    };

    private readonly Option<bool> interactiveOption = new(
        name: "--interactive",
        getDefaultValue: () => false,
        description: "Flag that turns on interactive mode.")
    {
        Arity = ArgumentArity.Zero,
    };

    private readonly Option<Mode> modeOption = new(
        name: "--mode",
        getDefaultValue: () => Mode.UniqueSolution,
        description: ModeOptionDescription)
    {
        Arity = ArgumentArity.ExactlyOne,
    };

    private readonly Option<FileInfo?> outputOption = new(
        name: "--output",
        getDefaultValue: () => null,
        description: OutputOptionDescription);

    private readonly Option<DirectoryInfo?> debugDataOption = new(
        name: "--write-debug-data-to",
        getDefaultValue: () => null,
        description: DebugDataOptionDescription);

    private readonly Argument<FileInfo?> fileArgument = new(
        name: "imageOrTextFile",
        description: FileArgumentDescription,
        getDefaultValue: () => null);

    public SolveCommandBuilder(
        IFileSystem fs)
        : this(fs, program => program.Run())
    {
    }

    public Command Build()
    {
        var command = new Command("solve", "Solves a straights puzzle")
        {
            this.modeOption,
            this.outputOption,
            this.interactiveOption,
#if DEBUG
            this.multiThreadedOption,
            this.debugDataOption,
#endif
        };

#if !DEBUG
        _ = this.multiThreadedOption;
        _ = this.debugDataOption;
#endif

        command.AddArgument(this.fileArgument);
        command.AddValidator(this.ValidateInteractive);

        command.SetHandler(this.RunProgram);

        return command;
    }

    private void RunProgram(InvocationContext context)
    {
        var pr = context.ParseResult;
        var file = pr.GetValueForArgument(this.fileArgument);

        var program = new SolveCommand(fs)
        {
            MultiThreaded = pr.GetValueForOption(this.multiThreadedOption),
            Interactive = pr.GetValueForOption(this.interactiveOption),
            DebugDataFolder = fs.Wrap(pr.GetValueForOption(this.debugDataOption)),
            Mode = pr.GetValueForOption(this.modeOption),
            OutputFile = fs.Wrap(pr.GetValueForOption(this.outputOption)),
            File = fs.Wrap(file),
        };

        int returnCode = execute(program);
        context.ExitCode = returnCode;
    }

    private void ValidateInteractive(CommandResult pr)
    {
        if (!pr.GetValueForOption(this.interactiveOption) &&
            pr.GetValueForArgument(this.fileArgument) == null)
        {
            pr.ErrorMessage =
            $"You must either provide an {this.fileArgument.Name} or use --{this.interactiveOption.Name}.";
        }
    }
}
