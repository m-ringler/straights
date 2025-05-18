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

using Xunit.Abstractions;

using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

/// <summary>
/// Tests for <see cref="GenerateCommand"/>.
/// </summary>
public class GenerateCommandTests
{
    public static TheoryData<TestConfig> GetConfigurations()
    {
        const string expectationHV =
"""
9
_,_,_,_,_,_,_,_,_
_,_,w2,b,_,b9,w8,_,_
_,b,_,_,_,_,w7,b9,_
b,w7,_,w3,_,_,_,w4,b
_,w8,b,_,b,_,b,w5,_
b1,_,_,_,_,_,w2,_,b
w7,b,_,_,_,_,_,b,_
_,_,_,b,_,b8,_,_,w3
_,_,_,w9,_,_,_,_,_

""";
        var configHV = new TestConfig(
            GridLayout.HorizontallyAndVerticallySymmetric,
            "Pcg32-6f1987e8d8374b4b-9ce293ec9c374996",
            new GridParameters(9, 11, 4),
            expectationHV);

        const string expectationP =
"""
9
_,_,_,_,_,_,_,w1,_
_,b1,w5,_,_,w4,_,b,_
_,_,b,_,b,_,_,_,b
b,b8,w9,b6,_,_,b1,_,_
_,_,_,_,_,_,_,_,_
w5,_,b,_,w3,b,_,b,b
b,_,_,_,b9,_,b,w8,_
w9,b,w1,_,_,_,_,b,_
_,_,_,_,_,_,_,w9,_

""";
        var configP = new TestConfig(
            GridLayout.PointSymmetric,
            "Pcg32-f9e95fdae687c07b-3a06f7c8ca46c11e",
            GridParameters.DefaultParameters,
            expectationP);

        string[] expectationD = [
"""
9
b5,b,_,_,b,_,w7,_,_
b,_,_,_,w3,w8,_,_,b
_,_,b,_,_,b1,_,_,w6
_,w4,w2,b,_,_,_,_,w5
b1,_,_,w9,w2,_,_,_,_
_,_,b,_,w7,b,_,_,_
_,_,_,_,_,_,b1,b,b3
_,w5,_,_,w4,_,b,_,w2
_,b,_,_,_,_,b,_,b

""",
"""
9
b5,b,_,_,b,_,w7,_,_
b,_,_,_,w3,w8,_,_,b
_,_,b,_,_,b1,_,_,_
_,_,_,b,_,_,_,_,w5
b1,_,_,_,_,w6,_,_,_
_,_,b,_,w7,b,_,_,_
_,_,_,_,_,_,b1,b,b3
_,w5,_,_,w4,_,b,_,w2
w9,b,_,_,_,_,b,_,b

""",
"""
9
b5,b,_,_,b,_,w7,_,_
b,_,_,_,w3,_,_,_,b
_,_,b,_,_,b1,_,_,_
_,_,_,b,_,_,_,_,w5
b1,_,_,_,_,w6,_,_,_
_,_,b,_,w7,b,_,_,_
_,_,_,_,_,w4,b1,b,b3
_,w5,_,_,_,_,b,_,w2
_,b,_,_,_,_,b,_,b

""",
"""
9
b5,b,_,_,b,_,w7,_,_
b,_,_,_,_,w8,_,_,b
_,_,b,_,_,b1,w8,_,_
_,_,_,b,_,_,_,_,w5
b1,_,_,_,_,_,_,_,_
_,_,b,_,w7,b,_,_,_
_,_,_,_,_,w4,b1,b,b3
_,w5,_,_,_,_,b,_,w2
_,b,_,_,_,_,b,_,b

""",
        ];

        var configD = Enumerable.Range(0, 4).Select(i =>
        new TestConfig(
            GridLayout.DiagonallySymmetric,
            "Pcg32-4368209fd6a5338e-b7df2ec45a0ab806",
            GridParameters.DefaultParameters,
            i,
            expectationD[i]));

        // In this test case, the grid generator
        // produces an unsolvable grid in the first attempt.
        const string expectationV =
            """
            9
            _,_,b,_,_,_,_,_,_
            _,_,_,w7,_,_,_,_,_
            _,_,b,_,b2,_,b,b,w1
            _,b1,_,_,_,w9,_,b2,b4
            _,w8,_,b,_,b,_,_,_
            _,b,_,_,_,_,_,b,b
            _,_,b,_,b,_,b,b9,_
            _,_,_,_,_,_,_,w8,_
            _,_,b,_,w6,_,_,_,_
            
            """;
        var configV = new TestConfig(
            GridLayout.VerticallySymmetric,
            "Pcg32-61a37e47ddf84678-0425b97f3425a026",
            GridParameters.DefaultParameters,
            (SimplifierStrength)3,
            expectationV);

        return
        [
            configHV,
            configP,
            .. configD,
            configV,
        ];
    }

    [Fact]
    public void Template_ProducesExpected()
    {
        // ARRANGE
        const string Seed = "Pcg32-8095ab65ad9a0966-976c179e64e07a18";
        const string Template =
"""
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
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { templatePath, new MockFileData(Template) },
            { XFS.Path(@"c:\output\foo"), new MockFileData([]) },
        });

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
        ShouldBeSolvableWithSimplifier(output, SimplifierStrength.DefaultStrength);
        var generatedGrid = output.FileSystem.File.ReadAllText(output.FullName);
        _ = generatedGrid.Should().Be(
"""
9
b1,_,_,_,b,_,_,w3,w5
_,w7,_,_,_,_,b,_,_
_,_,b,_,_,_,_,_,_
w7,_,_,_,_,_,_,_,b1
b,_,_,_,_,w5,w3,_,_
_,w4,b,_,_,_,b9,_,w7
w2,_,_,_,b,_,_,_,_
_,_,_,b,_,b,_,w4,_
_,_,_,_,_,w8,_,_,b

""");
    }

    [Theory]
    [MemberData(nameof(GetConfigurations))]
    public void Run_ProducesExpectedOutput(TestConfig c)
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { XFS.Path(@"c:\output\foo"), new MockFileData([]) },
        });

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
        ShouldBeSolvableWithSimplifier(output, SimplifierStrength.DefaultStrength);

        var generatedGrid = output.FileSystem.File.ReadAllText(output.FullName);
        _ = generatedGrid.Should().Be(c.ExpectedBuilderText);

        var builder = GridConverter.ParseBuilderText(generatedGrid).Builder;
        BlackFieldCount.Of(builder).Should().Be((BlackFieldCount)c.Grid);
    }

#pragma warning disable xUnit1004
    [Fact(Skip = "Test is very slow.")]
#pragma warning restore xUnit1004
    public void UncaughtExceptionBug()
    {
        const string seed = "Pcg32-cb74f8685fd30b6e-dfda3e356a9ba51f";

        var rng = new RandNRandomFactory().CreatePcg32(seed);
        var sut = new GenerateCommand
        {
            Terminal = Mock.Of<IWriteOnlyConsole>(),
            ReadLine = Mock.Of<Func<string>>(),
            GridParameters = new GridParameters(14, 12, 6),
            Layout = GridLayout.PointSymmetric,
            Attempts = 1000,
            FailureThreshold = 120,
            Random = (rng, rng.Seed),
            DifficultyLevel = 3,
        };

        // ACT // ASSERT
        new Action(() => sut.Run()).Should().NotThrow();
    }

    private static void ShouldBeSolvableWithSimplifier(IFileInfo f, SimplifierStrength strength)
    {
        var grid = GridConverter.LoadFrom(f);
        var solverGrid = grid.SolverGrid;
        var simplifier = GridSimplifierFactory.BuildIterativeSimplifier(strength);
        simplifier.Simplify(solverGrid);
        _ = solverGrid.IsSolved.Should().BeTrue();
    }

    public record struct TestConfig(
        GridLayout Layout,
        string Seed,
        GridParameters Grid,
        SimplifierStrength Difficulty,
        string ExpectedBuilderText) : IXunitSerializable
    {
        public TestConfig(
            GridLayout layout,
            string seed,
            GridParameters grid,
            string expectedBuilderText)
            : this(layout, seed, grid, SimplifierStrength.DefaultStrength, expectedBuilderText)
        {
        }

        public void Deserialize(IXunitSerializationInfo info)
        {
            this.Layout = (GridLayout)info.GetValue<int>(nameof(this.Layout));
            this.Seed = info.GetValue<string>(nameof(this.Seed));
            this.Grid = new GridParameters(
                info.GetValue<int>(nameof(GridParameters.Size)),
                info.GetValue<int>(nameof(GridParameters.NumberOfBlackBlanks)),
                info.GetValue<int>(nameof(GridParameters.NumberOfBlackNumbers)));
            this.Difficulty = info.GetValue<int>(nameof(this.Difficulty));
            this.ExpectedBuilderText = info.GetValue<string>(nameof(this.ExpectedBuilderText));
        }

        public readonly void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(this.Layout), (int)this.Layout);
            info.AddValue(nameof(this.Seed), this.Seed);
            info.AddValue(nameof(GridParameters.Size), this.Grid.Size);
            info.AddValue(nameof(GridParameters.NumberOfBlackBlanks), this.Grid.NumberOfBlackBlanks);
            info.AddValue(nameof(GridParameters.NumberOfBlackNumbers), this.Grid.NumberOfBlackNumbers);
            info.AddValue(nameof(this.Difficulty), this.Difficulty.Value);
            info.AddValue(nameof(this.ExpectedBuilderText), this.ExpectedBuilderText);
        }
    }
}
