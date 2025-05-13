// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.CommandLine;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

using RandN.Rngs;

using Straights.Solver.Generator;
using Straights.Solver.Simplification;

using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

/// <summary>
/// Tests for <see cref="GenerateCommandBuilder"/>.
/// </summary>
public class GenerateCommandBuilderTests
{
    private readonly IFileSystem fileSystem = new MockFileSystem();
    private readonly ConsoleStub console = new();

    [Fact]
    public void CheckDefaults()
    {
        GenerateCommandBuilder.DefaultSize.Should().Be(9);
        GenerateCommandBuilder.DefaultSize.Should().Be(GridParameters.DefaultParameters.Size);
        GenerateCommandBuilder.DefaultAttempts.Should().Be(10);
        GenerateCommandBuilder.DefaultFailureThreshold.Should().Be(50);
        GenerateCommandBuilder.DefaultLayout.Should().Be(GridLayout.Uniform);
        GenerateCommandBuilder.DefaultDifficulty.Should().Be(SimplifierStrength.DefaultStrength);
    }

    [Fact]
    public void BuildAndInvoke_WhenGridParametersProvided_ExecutesExpected()
    {
        // ARRANGE
        var runner = new RunCommandFunction<GenerateCommand>(5);
        var sut = new GenerateCommandBuilder(this.fileSystem, runner.Invoke);

        var args = new[]
        {
            "--output", XFS.Path(@"C:\Foo\output.txt"),
            "--size", "11",
            "--seed", "Pcg32-6f1987e8d8374b4b-9ce293ec9c374996",
            "--attempts", "15",
            "--failure-threshold", "91",
            "--black-blanks", "17",
            "--black-numbers", "2",
            "--layout", "HorizontallyAndVerticallySymmetric",
            "--difficulty", "2",
        };

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, this.console);
        var errorOutput = this.console.Error.Buffer.ToString();

        // ASSERT
        exitCode.Should().Be(5);
        var c = runner.InvokedCommand;
        c.Should().NotBeNull();

        (c.OutputFile?.FullName).Should().Be(args[1]);
        this.ShouldHaveCorrectFileSystem(c.OutputFile);

        c.GridParameters.Should().Be(new GridParameters(11, 17, 2));
        c.Random.Seed.Should().Be("Pcg32-6f1987e8d8374b4b-9ce293ec9c374996");
        c.Attempts.Should().Be(15);
        c.FailureThreshold.Should().Be(91);
        c.Layout.Should().Be(GridLayout.HorizontallyAndVerticallySymmetric);
        c.Template.Should().BeNull();
        c.DifficultyLevel.Should().Be((SimplifierStrength)2);
        errorOutput.Should().BeEmpty();
    }

    [Fact]
    public void BuildAndInvoke_TemplateProvided_ExecutesExpected()
    {
        // ARRANGE
        var runner = new RunCommandFunction<GenerateCommand>(3);
        var sut = new GenerateCommandBuilder(this.fileSystem, runner.Invoke);

        string[] args =
        [
            "--template", XFS.Path(@"C:\Foo\template.txt")
        ];

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, this.console);
        var errorOutput = this.console.Error.Buffer.ToString();

        // ASSERT
        errorOutput.Should().BeEmpty();
        exitCode.Should().Be(3);
        var c = runner.InvokedCommand;
        c.Should().NotBeNull();

        c.OutputFile.Should().BeNull();

        (c.Template?.FullName).Should().Be(args[1]);
        this.ShouldHaveCorrectFileSystem(c.Template);

        c.DifficultyLevel.Should().Be(SimplifierStrength.DefaultStrength);
    }

    [Theory]
    [InlineData(new string[] { "--black-blanks", "1" }, "You cannot use the --black-blanks and --template options together.*")]
    [InlineData(new string[] { "--black-numbers", "1" }, "You cannot use the --black-numbers and --template options together.*")]
    [InlineData(new string[] { "--size", "10" }, "You cannot use the --size and --template options together.*")]
    [InlineData(new string[] { "--layout", "Random" }, "You cannot use the --layout and --template options together.*")]
    [InlineData(new string[] { "--layout", "Random", "--size", "10" }, "You cannot use the --size and --template options together.*You cannot use the --layout and --template options together.*")]
    public void BuildAndInvoke_TemplateAndGridParametersProvided_Fails(
        string[] gridArgs,
        string expectedMessage)
    {
        // ARRANGE
        var runner = new RunCommandFunction<GenerateCommand>(0);
        var sut = new GenerateCommandBuilder(this.fileSystem, runner.Invoke);

        string[] args =
        [
            "--template", @"C:\Foo\template.txt", .. gridArgs,
        ];

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, this.console);
        var errorOutput = this.console.Error.Buffer.ToString();

        // ASSERT
        exitCode.Should().NotBe(0);
        var c = runner.InvokedCommand;
        c.Should().BeNull();
        errorOutput.Should().Match(expectedMessage);
    }

    [Fact]
    public void BuildAndInvoke_NoArgs_ExecutesExpected()
    {
        // ARRANGE
        var runner = new RunCommandFunction<GenerateCommand>(5);
        var sut = new GenerateCommandBuilder(this.fileSystem, runner.Invoke);

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke([], this.console);
        var errorOutput = this.console.Error.Buffer.ToString();

        // ASSERT
        exitCode.Should().Be(5);
        var c = runner.InvokedCommand;
        c.Should().NotBeNull();

        c.GridParameters.Should().Be(GridParameters.DefaultParameters);
        c.OutputFile.Should().BeNull();
        c.Random.Seed.Should().Match("Pcg32-*");
        c.Random.Rng.Should().BeOfType<RandNRandom<Pcg32>>();
        c.Attempts.Should().Be(GenerateCommandBuilder.DefaultAttempts);
        c.FailureThreshold.Should().Be(GenerateCommandBuilder.DefaultFailureThreshold);
        c.Layout.Should().Be(GenerateCommandBuilder.DefaultLayout);
        c.Template.Should().BeNull();
        c.PlayUri.Should().Be(new Uri("http://mospace.de/str8ts/?code="));
        errorOutput.Should().BeEmpty();
    }

    [Fact]
    public void BuildAndInvoke_NoSeedArgument_UsesADifferentSeedEveryTime()
    {
        // ARRANGE
        var runner = new RunCommandFunction<GenerateCommand>(5);
        var sut = new GenerateCommandBuilder(this.fileSystem, runner.Invoke);

        // ACT
        var command = sut.Build();
        _ = command.Invoke([], this.console);
        var seed1 = runner.InvokedCommand!.Random.Seed;
        _ = command.Invoke([], this.console);
        var seed2 = runner.InvokedCommand!.Random.Seed;

        // ASSERT
        seed2.Should().NotBe(seed1);
    }

    [Theory]
    [InlineData(new string[] { "--size", "25" }, "--size must be less than or equal to 24.*")]
    [InlineData(new string[] { "--size", "2" }, "--size must be greater than or equal to 4.*")]
    [InlineData(new string[] { "--size", "10", "--black-blanks", "26" }, "--black-blanks must be less than or equal to 25 for size 10.*")]
    [InlineData(new string[] { "--black-blanks", "-1" }, "--black-blanks must be greater than or equal to 0.*")]
    [InlineData(new string[] { "--black-numbers", "-1" }, "--black-numbers must be greater than or equal to 0.*")]
    [InlineData(new string[] { "--size", "10", "--black-numbers", "11" }, "--black-numbers must be less than or equal to 10 for size 10.*")]
    [InlineData(new string[] { "--attempts", "0", "--failure-threshold", "0" }, "--attempts must be greater than or equal to 1.*--failure-threshold must be greater than or equal to 1.*")]
    [InlineData(new string[] { "--difficulty", "-1" }, "--difficulty must be greater than or equal to 0.*")]
    public void BuildAndInvoke_ArgumentOutOfRange_Fails(string[] args, string expectedMessage)
    {
        // ARRANGE
        var runner = new RunCommandFunction<GenerateCommand>(0);
        var sut = new GenerateCommandBuilder(this.fileSystem, runner.Invoke);

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, this.console);
        var errorOutput = this.console.Error.Buffer.ToString();

        // ASSERT
        exitCode.Should().NotBe(0);
        var c = runner.InvokedCommand;
        c.Should().BeNull();
        errorOutput.Should().Match(expectedMessage);
    }

    [Fact]
    public Task Help()
    {
        return HelpVerifier.VerifyHelp<GenerateCommand>(
            execute =>
                new GenerateCommandBuilder(new MockFileSystem(), execute));
    }

    [Theory]
    [InlineData(GridLayout.HorizontallySymmetric, 4, 1)]
    [InlineData(GridLayout.PointSymmetric, 4, 1)]
    [InlineData(GridLayout.VerticallySymmetric, 4, 1)]
    [InlineData(GridLayout.VerticallySymmetric, 6, 1)]
    [InlineData(GridLayout.VerticallySymmetric, 8, 3)]
    [InlineData(GridLayout.VerticallySymmetric, 10, 5)]
    [InlineData(GridLayout.VerticallySymmetric, 12, 9)]
    public void BuildAndInvoke_WhenGridConstrainedBySymmetry2_ExecutesExpected(
        GridLayout layout,
        int size,
        int expectedBlackNumbers)
    {
        // ARRANGE
        var runner = new RunCommandFunction<GenerateCommand>(5);
        var sut = new GenerateCommandBuilder(this.fileSystem, runner.Invoke);

        var args = new[]
        {
            "--size", $"{size}",
            "--black-blanks", "3",
            "--layout", layout.ToString(),
        };

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, this.console);
        var errorOutput = this.console.Error.Buffer.ToString();

        // ASSERT
        exitCode.Should().Be(5);
        errorOutput.Should().BeEmpty();
        var c = runner.InvokedCommand;
        c.Should().NotBeNull();

        var g = c.GridParameters;
        g.Should().Be(new GridParameters(size, 3, expectedBlackNumbers));
        (g.TotalNumberOfBlackFields % 2).Should().Be(0);

        c.Layout.Should().Be(layout);
        c.Template.Should().BeNull();
    }

    [Theory]
    [InlineData(6, 3)]
    [InlineData(8, 3)]
    [InlineData(10, 7)]
    [InlineData(12, 7)]
    public void BuildAndInvoke_WhenGridConstrainedBySymmetry4_ExecutesExpected(
        int size,
        int expectedBlackNumbers)
    {
        // ARRANGE
        var runner = new RunCommandFunction<GenerateCommand>(5);
        var sut = new GenerateCommandBuilder(this.fileSystem, runner.Invoke);

        var args = new[]
        {
            "--size", $"{size}",
            "--black-blanks", "5",
            "--layout", $"{GridLayout.HorizontallyAndVerticallySymmetric}",
        };

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, this.console);
        var errorOutput = this.console.Error.Buffer.ToString();

        // ASSERT
        exitCode.Should().Be(5);
        errorOutput.Should().BeEmpty();
        var c = runner.InvokedCommand;
        c.Should().NotBeNull();

        var g = c.GridParameters;
        g.Should().Be(new GridParameters(size, 5, expectedBlackNumbers));
        (g.TotalNumberOfBlackFields % 4).Should().Be(0);

        c.Layout.Should().Be(GridLayout.HorizontallyAndVerticallySymmetric);
        c.Template.Should().BeNull();
    }

    [Theory]
    [InlineData(4, 0, 4)]
    [InlineData(6, 2, 6)]
    [InlineData(8, 3, 9)]
    [InlineData(10, 6, 18)]
    [InlineData(12, 8, 24)]
    public void BuildAndInvoke_WhenGridConstrainedBySymmetry4_ExecutesExpected1(
        int size,
        int expectedBlackNumbers,
        int expectedBlackBlanks)
    {
        // ARRANGE
        var runner = new RunCommandFunction<GenerateCommand>(5);
        var sut = new GenerateCommandBuilder(this.fileSystem, runner.Invoke);

        var args = new[]
        {
            "--size", $"{size}",
            "--layout", $"{GridLayout.HorizontallyAndVerticallySymmetric}",
        };

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, this.console);
        var errorOutput = this.console.Error.Buffer.ToString();

        // ASSERT
        exitCode.Should().Be(5);
        errorOutput.Should().BeEmpty();
        var c = runner.InvokedCommand;
        c.Should().NotBeNull();

        var g = c.GridParameters;
        g.Should().Be(new GridParameters(size, expectedBlackBlanks, expectedBlackNumbers));
        (g.TotalNumberOfBlackFields % 4).Should().Be(0);

        c.Layout.Should().Be(GridLayout.HorizontallyAndVerticallySymmetric);
        c.Template.Should().BeNull();
    }

    [Theory]
    [InlineData(GridLayout.HorizontallySymmetric, 15, "The total number of black fields must be a multiple of 2 for size 8 and layout HorizontallySymmetric.*")]
    [InlineData(GridLayout.VerticallySymmetric, 15, "The total number of black fields must be a multiple of 2 for size 8 and layout VerticallySymmetric.*")]
    [InlineData(GridLayout.PointSymmetric, 15, "The total number of black fields must be a multiple of 2 for size 8 and layout PointSymmetric.*")]
    [InlineData(GridLayout.HorizontallyAndVerticallySymmetric, 15, "The total number of black fields must be a multiple of 4 for size 8 and layout HorizontallyAndVerticallySymmetric.*")]
    public void BuildAndInvoke_WhenSymmetryViolated_Fails(
        GridLayout layout,
        int numberOfBlackFields,
        string expectedMessage)
    {
        // ARRANGE
        var runner = new RunCommandFunction<GenerateCommand>(0);
        var sut = new GenerateCommandBuilder(this.fileSystem, runner.Invoke);

        var args = new[]
        {
            "--size", "8",
            "--black-blanks", $"{numberOfBlackFields - 5}",
            "--black-numbers", "5",
            "--layout", $"{layout}",
        };

        // ACT
        var command = sut.Build();
        int exitCode = command.Invoke(args, this.console);
        var errorOutput = this.console.Error.Buffer.ToString();

        // ASSERT
        exitCode.Should().NotBe(0);
        var c = runner.InvokedCommand;
        c.Should().BeNull();
        errorOutput.Should().Match(expectedMessage);
    }

    private void ShouldHaveCorrectFileSystem(IFileInfo f)
    {
        f.Should().BeOfType<FileInfoWrapper>().Which.FileSystem.Should().BeSameAs(this.fileSystem);
    }
}