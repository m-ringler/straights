// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.IO.Abstractions.TestingHelpers;

using Moq;

using Straights.Console;

using XFS = System.IO.Abstractions.TestingHelpers.MockUnixSupport;

/// <summary>
/// Tests for the <see cref="SolveCommand"/> class.
/// </summary>
public partial class SolveCommandTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Run_ProducesExpectedOutput(bool multithreaded)
    {
        // ARRANGE
        const string Input =
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

""";

        var inputFilePath = XFS.Path(@"c:\straights\input.txt");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { inputFilePath, new MockFileData(Input) },
            { XFS.Path(@"c:\output\foo"), new MockFileData([]) },
        });

        var input = fileSystem.FileInfo.New(inputFilePath);
        var output = fileSystem.FileInfo.New(XFS.Path(@"C:\output\grid.txt"));
        var sut = new SolveCommand(fileSystem)
        {
            Terminal = Mock.Of<IWriteOnlyConsole>(),
            ReadLine = Mock.Of<Func<string>>(),
            MultiThreaded = multithreaded,
            OutputFile = output,
            Interactive = false,
            File = input,
        };

        // ACT
        var result = sut.Run();

        // ASSERT
        _ = result.Should().Be(0);
        var generatedGrid = output.FileSystem.File.ReadAllText(output.FullName);
        _ = generatedGrid.Should().Be(
"""
9
b1,w8,w9,w7,b,w6,w4,w3,w5
w6,w7,w8,w9,w5,w4,b,w2,w3
w8,w9,b,w6,w3,w2,w1,w5,w4
w7,w6,w5,w8,w4,w3,w2,w9,b1
b,w2,w6,w4,w1,w5,w3,w7,w8
w5,w4,b,w3,w2,w1,b9,w6,w7
w2,w3,w4,w5,b,w7,w6,w8,w9
w3,w1,w2,b,w7,b,w5,w4,w6
w4,w5,w3,w2,w6,w8,w7,w1,b

""");
    }
}
