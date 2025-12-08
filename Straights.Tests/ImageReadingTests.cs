// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;

using Straights.Image.GridReader;

public class ImageReadingTests
{
    private static readonly ImmutableArray<string> ImageExtensions = [".png", ".jpg", ".jpeg"];

    public static TheoryData<TestImage?> TestImages
    {
        get
        {
            var folder = GetTestImageFolder();
            if (folder == null)
            {
                return [null];
            }

            var filesByBaseName = folder.GetFiles(
                "*",
                new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 1 })
            .ToLookup(
                f => folder.FileSystem.Path.GetFileNameWithoutExtension(f.Name),
                f => f);
            var result = from g in filesByBaseName
                         let textFile = g.FirstOrDefault(IsTextFile)
                         where textFile != null
                         from imageFile in g.Where(IsImageFile)
                         select new TestImage(
                            ExpectedTextPath: textFile,
                            ImagePath: imageFile);

            TheoryData<TestImage?> data = [.. result];
            if (data.Count == 0)
            {
                data.Add(new(null));
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(TestImages), DisableDiscoveryEnumeration = true)]
    public void CheckImagesInTestFolderAreCorrectlyRead(TestImage? testImage)
    {
        if (testImage != null)
        {
            Verify(testImage);
        }
    }

    private static bool IsImageFile(IFileInfo x)
    {
        return ImageExtensions.Contains(x.Extension.ToLowerInvariant());
    }

    private static bool IsTextFile(IFileInfo x)
    {
        return x.Extension.Equals(".txt", StringComparison.OrdinalIgnoreCase);
    }

    private static void Verify(TestImage testImage)
    {
        // ARRANGE
        var fs = testImage.ImagePath.FileSystem;
        var readerFactory = new GridReaderFactory();
        var reader = readerFactory.CreateGridReader();

        // ACT
        var grid1 = reader.ReadGrid(testImage.ImagePath.FullName);
        var builder1 = CellsToGridBuilderAdapter.ToBuilder(grid1);
        var actual = builder1.ToString().ReplaceLineEndings();

        // ASSERT
        var expected = fs.File.ReadAllText(
            testImage.ExpectedTextPath.FullName)
            .ReplaceLineEndings();

        _ = actual.Should().Be(
            expected,
            because: $"We expect reading {testImage.ImagePath} to yield the text in {testImage.ExpectedTextPath}");
    }

    private static IDirectoryInfo? GetTestImageFolder([CallerFilePath] string? callerFilePath = null)
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
        return fs.Directory.Exists(dir)
            ? fs.DirectoryInfo.New(dir)
            : null;
    }

    public record TestImage(IFileInfo ImagePath, IFileInfo ExpectedTextPath);
}
