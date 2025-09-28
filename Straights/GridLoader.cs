// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.IO.Abstractions;

using Straights.Image.GridReader;
using Straights.Solver;
using Straights.Solver.Builder;
using Straights.Solver.Converter;

internal sealed class GridLoader
{
    private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg"];

    public IDirectoryInfo? DebugDataFolder { get; init; }

    public (ConvertibleGrid Grid, bool IsImage, string? SuggestedSavePath)
        LoadGrid(IFileInfo args)
    {
        string path = args.FullName;

        var fs = args.FileSystem;
        var savePath = fs.Path.ChangeExtension(path, ".txt");
        bool isImage = IsImage(args);
        var grid = isImage
            ? this.LoadGridFromImage(args)
            : LoadGridFromFile(args);
        return (grid, isImage, savePath);
    }

    private static bool HasExtension(IFileInfo file, string extension)
    {
        return file.Extension.Equals(
            extension, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsImage(IFileInfo file)
    {
        return ImageExtensions.Any(ext => HasExtension(file, ext));
    }

    private static ConvertibleGrid LoadGridFromFile(IFileInfo file)
    {
        return GridConverter.LoadFrom(file);
    }

    private ConvertibleGrid LoadGridFromImage(IFileInfo file)
    {
        var factory = new GridReaderFactory();
        var cells = factory.CreateGridReader(this.DebugDataFolder?.FullName).ReadGrid(file.FullName);
        GridBuilder builder = CellsToGridBuilderAdapter.ToBuilder(cells);

        return new ConvertibleGrid(builder);
    }
}
