// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO.Abstractions;

using RandN.Rngs;

using Straights.Generate;
using Straights.Solver.Generator;
using Straights.Solver.Simplification;

internal sealed class GenerateCommandBuilder(
        IFileSystem fs,
        Func<GenerateCommand, int> runCommand)
    : ICommandBuilder
{
    public const int DefaultAttempts = 10;
    public const int DefaultFailureThreshold = 50;
    public const GridLayout DefaultLayout = EmptyGrid.DefaultLayout;
    public static readonly int DefaultSize = EmptyGrid.DefaultSize;
    public static readonly SimplifierStrength DefaultDifficulty
        = SimplifierStrength.DefaultStrength;

    private const string OutputOptionDescription =
        """
        Write the generated grid to the specified file.
        Currently the following formats are supported:
        * HTML (.htm, .html)
        * TEXT (.txt)
        * JSON (.json)
        More formats may be supported in the future.

        """;

    private const string SeedOptionDescription =
        """
        The seed to use for the pseudo-random number generator.
        Use this if you want repeatable results. The command
        prints the used seed value for each generated grid.
        Currently, grid generation results for a given seed may
        change between program versions, because of changes in the
        grid generation algorithm. Store the generated grids,
        not the seed values.

        """;

    private const string AttemptsOptionDescription =
        """
        The maximum number of attempts to generate a grid
        with the specified parameters.

        """;

    private const string FailureThresholdOptionDescription =
        """
        A positive number indicating a threshold when to regard
        a generation attempt as failed. Increasing this number
        will increase the average time it takes to generate a grid.

        """;

    private const string DifficultyOptionDescription =
        """
        The difficulty of the generated grid, in the range 0 to 3.
        A higher value means you need to apply more and more complex
        rules to solve the generated grid.
        This option determines which white numbers are revealed
        from the start. The overall difficulty is also influenced
        by the number of black blanks and the number of black numbers.
        
        """;

    private readonly Option<FileInfo?> outputOption = new(
        name: "--output",
        getDefaultValue: () => null,
        description: OutputOptionDescription);

    private readonly Option<string?> seedOption = new(
        name: "--seed",
        getDefaultValue: () => null,
        description: SeedOptionDescription)
    {
        Arity = ArgumentArity.ExactlyOne,
    };

    private readonly Option<int> attemptsOption = new(
        name: "--attempts",
        getDefaultValue: () => DefaultAttempts,
        description: AttemptsOptionDescription)
    {
        Arity = ArgumentArity.ExactlyOne,
    };

    private readonly Option<int> failureThresholdOption = new(
        name: "--failure-threshold",
        getDefaultValue: () => DefaultFailureThreshold,
        description: FailureThresholdOptionDescription)
    {
        Arity = ArgumentArity.ExactlyOne,
    };

    private readonly Option<int> difficultyOption = new(
        name: "--difficulty",
        getDefaultValue: () => DefaultDifficulty.Value,
        description: DifficultyOptionDescription)
    {
        Arity = ArgumentArity.ExactlyOne,
    };

    private readonly EmptyGrid emptyGrid = new();

    public GenerateCommandBuilder(IFileSystem fs)
        : this(fs, c => c.Run())
    {
    }

    public Command Build()
    {
        Option[] options =
        [
            this.outputOption,
            .. this.emptyGrid.Options,
            this.seedOption,
            this.attemptsOption,
            this.failureThresholdOption,
            this.difficultyOption
        ];

        var command = new Command("generate", "Generates a straights puzzle");
        foreach (var option in options)
        {
            command.Add(option);
        }

        command.AddValidator(this.Validate);
        command.SetHandler(this.RunProgram);

        return command;
    }

    private void RunProgram(InvocationContext context)
    {
        var pr = context.ParseResult;
        this.emptyGrid.GetGridParameters(
            pr.CommandResult,
            out GridParameters? gridParameters,
            out GridLayout layout,
            out FileInfo? template);

        var random = this.CreateRandomNumberGenerator(pr);

        var program = new GenerateCommand
        {
            GridParameters = gridParameters ?? GridParameters.DefaultParameters,
            Layout = layout,
            Template = fs.Wrap(template),
            OutputFile = fs.Wrap(pr.GetValueForOption(this.outputOption)),
            Random = (random, random.Seed),
            Attempts = pr.GetValueForOption(this.attemptsOption),
            FailureThreshold = pr.GetValueForOption(this.failureThresholdOption),
            DifficultyLevel = (SimplifierStrength)pr.GetValueForOption(this.difficultyOption),
        };

        context.ExitCode = runCommand(program);
    }

    private RandNRandom<Pcg32> CreateRandomNumberGenerator(ParseResult pr)
    {
        var seed = pr.GetValueForOption(this.seedOption);

        var factory = new RandNRandomFactory();
        var random = factory.CreatePcg32(seed);
        return random;
    }

    private void Validate(SymbolResult symbolResult)
    {
        IReadOnlyCollection<string> errors = [
            .. this.emptyGrid.GetValidationMessages(symbolResult),
            .. this.GetOutOfRangeErrors(symbolResult)];

        if (errors.Count != 0)
        {
            symbolResult.ErrorMessage = string.Join(Environment.NewLine, errors);
        }
    }

    private IEnumerable<string> GetOutOfRangeErrors(SymbolResult r)
    {
        return
        [
            .. RequireMinimum(this.attemptsOption, 1),
            .. RequireMinimum(this.failureThresholdOption, 1),
            .. RequireMinimum(this.difficultyOption, 0),
        ];

        IEnumerable<string> RequireMinimum(Option<int> option, int min)
        {
            int value = r.GetValueForOption(option);
            return option.RequireMin(value, min);
        }
    }
}
