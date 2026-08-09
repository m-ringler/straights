// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using Straights.Console;
using Straights.Solver;
using Straights.Solver.Generator;
using Straights.Solver.Simplification;
using Xunit.Sdk;
using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

/// <summary>
/// Tests for <see cref="GenerateCommand"/>.
/// </summary>
public class GenerateCommandTests
{
    public static TheoryData<TestConfig> GetConfigurations()
    {
        var configHV = new TestConfig(
            GridLayout.HorizontallyAndVerticallySymmetric,
            "Pcg32-6f1987e8d8374b4b-9ce293ec9c374996",
            new GridParameters(9, 11, 4),
            "GenerateCommandTests.HorizontallyAndVerticallySymmetric"
        );

        var configP = new TestConfig(
            GridLayout.PointSymmetric,
            "Pcg32-f9e95fdae687c07b-3a06f7c8ca46c11e",
            GridParameters.DefaultParameters,
            "GenerateCommandTests.PointSymmetric"
        );

        var configD = Enumerable
            .Range(0, 4)
            .Select(i => new TestConfig(
                GridLayout.DiagonallySymmetric,
                "Pcg32-4368209fd6a5338e-b7df2ec45a0ab806",
                GridParameters.DefaultParameters,
                i,
                $"GenerateCommandTests.DiagonallySymmetric.{i}"
            ));

        // In this test case, the grid generator
        // produces an unsolvable grid in the first attempt.
        var configV = new TestConfig(
            GridLayout.VerticallySymmetric,
            "Pcg32-61a37e47ddf84678-0425b97f3425a026",
            GridParameters.DefaultParameters,
            (SimplifierStrength)3,
            "GenerateCommandTests.VerticallySymmetric"
        );

        return [configHV, configP, .. configD, configV];
    }

    [Fact]
    public async Task Template()
    {
        // ARRANGE
        const string Seed = "Pcg32-8095ab65ad9a0966-976c179e64e07a18";
        const string Template = """
9
b7,_,_,_,b,_,_,_,_
_,_,_,_,_,_,b,_,_
_,_,b,_,_,_,_,_,_
_,_,_,_,_,_,_,_,b3
b,_,_,_,_,_,_,_,_
_,_,b,_,_,_,b1,_,_
_,_,_,_,b,_,_,_,_
_,_,_,b,_,b,_,_,_
_,_,_,_,_,_,_,_,b

""";

        var templatePath = XFS.Path(@"c:\templates\template.txt");
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { templatePath, new MockFileData(Template) },
                { XFS.Path(@"c:\output\foo"), new MockFileData([]) },
            }
        );

        var template = fileSystem.FileInfo.New(templatePath);
        var output = fileSystem.FileInfo.New(XFS.Path(@"C:\output\grid.txt"));
        var rng = new RandNRandomFactory().CreatePcg32(Seed);
        var sut = new GenerateCommand
        {
            Terminal = Mock.Of<IWriteOnlyConsole>(),
            ReadLine = Mock.Of<Func<string>>(),
            GridParameters = GridParameters.DefaultParameters,
            Template = template,
            OutputFile = output,
            Random = (rng, Seed),
        };

        // ACT
        var result = sut.Run();

        // ASSERT
        _ = result.Should().Be(0);
        ShouldBeSolvableWithSimplifier(
            output,
            SimplifierStrength.DefaultStrength
        );
        var generatedGrid = output.FileSystem.File.ReadAllText(output.FullName);
        await Verify(generatedGrid);
    }

    [Theory]
    [MemberData(nameof(GetConfigurations))]
    public async Task Run_ProducesExpectedOutput(TestConfig c)
    {
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                { XFS.Path(@"c:\output\foo"), new MockFileData([]) },
            }
        );

        var output = fileSystem.FileInfo.New(XFS.Path(@"C:\output\grid.txt"));
        var rng = new RandNRandomFactory().CreatePcg32(c.Seed);
        var sut = new GenerateCommand
        {
            Terminal = Mock.Of<IWriteOnlyConsole>(),
            ReadLine = Mock.Of<Func<string>>(),
            GridParameters = c.Grid,
            Layout = c.Layout,
            OutputFile = output,
            Random = (rng, rng.Seed),
            DifficultyLevel = c.Difficulty,
        };

        // ACT
        var result = sut.Run();

        // ASSERT
        _ = result.Should().Be(0);
        ShouldBeSolvableWithSimplifier(
            output,
            SimplifierStrength.DefaultStrength
        );

        var generatedGrid = output.FileSystem.File.ReadAllText(output.FullName);
        await Verify(generatedGrid).UseFileName(c.VerifyFileName);

        var builder = GridConverter.ParseBuilderText(generatedGrid).Builder;
        BlackFieldCount.Of(builder).Should().Be((BlackFieldCount)c.Grid);
    }

    private static void ShouldBeSolvableWithSimplifier(
        IFileInfo f,
        SimplifierStrength strength
    )
    {
        var grid = GridConverter.LoadFrom(f);
        var solverGrid = grid.SolverGrid;
        var simplifier = GridSimplifierFactory.BuildIterativeSimplifier(
            strength
        );
        simplifier.Simplify(solverGrid);
        _ = solverGrid.IsSolved.Should().BeTrue();
    }

    public record struct TestConfig(
        GridLayout Layout,
        string Seed,
        GridParameters Grid,
        SimplifierStrength Difficulty,
        string VerifyFileName
    ) : IXunitSerializable
    {
        public TestConfig(
            GridLayout layout,
            string seed,
            GridParameters grid,
            string verifyFileName
        )
            : this(
                layout,
                seed,
                grid,
                SimplifierStrength.DefaultStrength,
                verifyFileName
            ) { }

        public void Deserialize(IXunitSerializationInfo info)
        {
            this.Layout = (GridLayout)info.GetValue<int>(nameof(this.Layout));
            this.Seed =
                info.GetValue<string>(nameof(this.Seed))
                ?? throw new ArgumentException(
                    $"{nameof(this.Seed)} cannot be null.",
                    nameof(info)
                );
            this.Grid = new GridParameters(
                info.GetValue<int>(nameof(GridParameters.Size)),
                info.GetValue<int>(nameof(GridParameters.NumberOfBlackBlanks)),
                info.GetValue<int>(nameof(GridParameters.NumberOfBlackNumbers))
            );
            this.Difficulty = info.GetValue<int>(nameof(this.Difficulty));
            this.VerifyFileName =
                info.GetValue<string>(nameof(this.VerifyFileName))
                ?? throw new ArgumentException(
                    $"{nameof(this.VerifyFileName)} cannot be null.",
                    nameof(info)
                );
        }

        public readonly void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(this.Layout), (int)this.Layout);
            info.AddValue(nameof(this.Seed), this.Seed);
            info.AddValue(nameof(GridParameters.Size), this.Grid.Size);
            info.AddValue(
                nameof(GridParameters.NumberOfBlackBlanks),
                this.Grid.NumberOfBlackBlanks
            );
            info.AddValue(
                nameof(GridParameters.NumberOfBlackNumbers),
                this.Grid.NumberOfBlackNumbers
            );
            info.AddValue(nameof(this.Difficulty), this.Difficulty.Value);
            info.AddValue(nameof(this.VerifyFileName), this.VerifyFileName);
        }
    }
}
