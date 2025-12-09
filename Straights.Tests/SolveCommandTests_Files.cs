// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using Straights.Tests.Console;

/// <summary>
/// Tests for the <see cref="SolveCommand"/> class.
/// </summary>
public partial class SolveCommandTests
{
    private static readonly ImmutableArray<string> GridFileExtensions =
    [
        ".png",
        ".jpg",
        ".jpeg",
        ".txt",
    ];

    public static TheoryData<TestDataItem?> SolverTestData
    {
        get
        {
            var folder = GetTestDataFolder();
            if (folder == null)
            {
                return [null];
            }

            var fs = folder.FileSystem;
            var filesByBaseName = folder
                .GetFiles(
                    "*",
                    new EnumerationOptions
                    {
                        RecurseSubdirectories = true,
                        MaxRecursionDepth = 1,
                    }
                )
                .ToLookup(
                    f => fs.Path.GetFileNameWithoutExtension(f.Name),
                    f => f
                );
            var result =
                from g in filesByBaseName
                let textFile = g.FirstOrDefault(IsSolutionFile)
                where textFile != null
                from imageFile in g.Where(IsGridFile)
                select new TestDataItem(
                    ExpectedSolutionPath: textFile,
                    GridPath: imageFile
                );

            TheoryData<TestDataItem?> data = [.. result];
            if (data.Count == 0)
            {
                data.Add(new(null));
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(SolverTestData), DisableDiscoveryEnumeration = true)]
    public void CheckGridsInTestFolderAreCorrectlySolved(TestDataItem? item)
    {
        if (item != null)
        {
            Verify(item);
        }
    }

    private static bool IsGridFile(IFileInfo x)
    {
        return GridFileExtensions.Contains(x.Extension.ToLowerInvariant());
    }

    private static bool IsSolutionFile(IFileInfo x)
    {
        return x.Extension.Equals(
            ".solution",
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static void Verify(TestDataItem item)
    {
        // ARRANGE
        var fs = item.ExpectedSolutionPath.FileSystem;
        var console = new StringBuilderConsole();
        var program = new SolveCommand(fs)
        {
            Terminal = console,
            Interactive = false,
            Mode = SolveCommand.SolverMode.UniqueSolution,
            File = item.GridPath,
        };

        // ACT
        _ = program.Run().Should().Be(0, because: item.GridPath.FullName);
        var actual = console.ToString().Trim().ReplaceLineEndings();
        actual = actual[
            ..actual.LastIndexOf(Environment.NewLine, StringComparison.Ordinal)
        ]
            .Trim();

        // ASSERT
        try
        {
            var expected = fs
                .File.ReadAllText(item.ExpectedSolutionPath.FullName)
                .Trim()
                .ReplaceLineEndings();
            _ = actual
                .Should()
                .Be(
                    expected,
                    because: $"We expect solving {item.GridPath} to yield the text in {item.ExpectedSolutionPath}"
                );
        }
        catch
        {
            var received = item.ExpectedSolutionPath.FullName + ".received.txt";
            fs.File.WriteAllText(received, actual);
            System.Console.WriteLine("Writing actual to " + received);
            throw;
        }
    }

    private static IDirectoryInfo? GetTestDataFolder(
        [CallerFilePath] string? callerFilePath = null
    )
    {
        if (callerFilePath == null)
        {
            return null;
        }

        var fs = new FileSystem();
        var dir = fs.Path.GetDirectoryName(callerFilePath);
        if (dir == null || !fs.Directory.Exists(dir))
        {
            return null;
        }

        dir = fs.Path.Combine(dir, "..", "test");
        return fs.Directory.Exists(dir) ? fs.DirectoryInfo.New(dir) : null;
    }

    public record TestDataItem(
        IFileInfo GridPath,
        IFileInfo ExpectedSolutionPath
    );
}
