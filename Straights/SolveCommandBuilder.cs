// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.IO.Abstractions;
using Mode = SolveCommand.SolverMode;

internal sealed class SolveCommandBuilder(
    IFileSystem fs,
    Func<SolveCommand, int> execute
) : ICommandBuilder
{
    private const string FileArgumentDescription = """
        An image file with a Straights grid, or a Straights grid saved as .txt or .json.
        When this argument is omitted, the grid editor is started.

        """;

    private const string DebugDataOptionDescription = """
        When this option is provided, debug data is written to the specified directory.
        Existing data in the directory will be overwritten without warning.

        """;

    private const string OutputOptionDescription = """
        Write the final grid (after solving) to the specified file.
        Currently the following formats are supported:
        * HTML (.htm, .html)
        * JSON (.json)
        More formats may be supported in the future.

        """;

    private const string ModeOptionDescription = $"""
        The solver mode to use.
        {nameof(Mode.SimplifyOnly)}:   reason about single rows and columns.
        {nameof(Mode.UniqueSolution)}: find a unique solution.
        {nameof(
            Mode.AnySolution
        )}:    find any solution (even if it is not unique).
        In {nameof(Mode.AnySolution)} mode, the solver uses random numbers,
        and results may vary from run to run.

        """;

    private readonly Option<bool> multiThreadedOption = new(
        name: "--multi-threaded"
    )
    {
        Description = "Flag that turns on multi-threading.",
        Arity = ArgumentArity.Zero,
        DefaultValueFactory = _ => false,
    };

    private readonly Option<bool> interactiveOption = new(name: "--interactive")
    {
        Description = "Flag that turns on interactive mode.",
        Arity = ArgumentArity.Zero,
        DefaultValueFactory = _ => false,
    };

    private readonly Option<Mode> modeOption = new(name: "--mode")
    {
        Description = ModeOptionDescription,
        Arity = ArgumentArity.ExactlyOne,
        DefaultValueFactory = _ => Mode.UniqueSolution,
    };

    private readonly Option<FileInfo?> outputOption = new(name: "--output")
    {
        Description = OutputOptionDescription,
        DefaultValueFactory = _ => null,
        HelpName = "file",
    };

    private readonly Option<DirectoryInfo?> debugDataOption = new(
        name: "--write-debug-data-to"
    )
    {
        Description = DebugDataOptionDescription,
        DefaultValueFactory = _ => null,
        HelpName = "directory",
    };

    private readonly Argument<FileInfo?> fileArgument = new(
        name: "imageOrTextFile"
    )
    {
        Description = FileArgumentDescription,
        DefaultValueFactory = _ => null,
    };

    public SolveCommandBuilder(IFileSystem fs)
        : this(fs, program => program.Run()) { }

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
            new HelpOption(),
            this.fileArgument,
        };

#if !DEBUG
        _ = this.multiThreadedOption;
        _ = this.debugDataOption;
#endif

        command.Validators.Add(this.ValidateInteractive);

        command.SetAction(this.RunProgram);

        return command;
    }

    private int RunProgram(ParseResult pr)
    {
        var file = pr.GetValue(this.fileArgument);

        var program = new SolveCommand(fs)
        {
            MultiThreaded = pr.GetValue(this.multiThreadedOption),
            Interactive = pr.GetValue(this.interactiveOption),
            DebugDataFolder = fs.Wrap(pr.GetValue(this.debugDataOption)),
            Mode = pr.GetValue(this.modeOption),
            OutputFile = fs.Wrap(pr.GetValue(this.outputOption)),
            File = fs.Wrap(file),
        };

        int returnCode = execute(program);
        return returnCode;
    }

    private void ValidateInteractive(CommandResult pr)
    {
        if (
            !pr.GetValue(this.interactiveOption)
            && pr.GetValue(this.fileArgument) == null
        )
        {
            pr.AddError(
                $"You must either provide an {this.fileArgument.Name} or use {this.interactiveOption.Name}."
            );
        }
    }
}
