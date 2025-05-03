// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.IO.Abstractions;

using Straights.Image.GridReader;
using Straights.Solver;
using Straights.Solver.Builder;

internal sealed class GridLoader
{
    private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg"];

    public IDirectoryInfo? DebugDataFolder { get; init; }

    public (GridBuilder Builder, bool IsImage, string? SuggestedSavePath)
        LoadGrid(IFileInfo args)
    {
        string path = args.FullName;

        var fs = args.FileSystem;
        var savePath = fs.Path.ChangeExtension(path, ".txt");
        bool isImage = IsImage(args);
        var builder = isImage
            ? this.LoadGridFromImage(args)
            : LoadGridFromFile(args);
        return (builder, isImage, savePath);
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

    private static GridBuilder LoadGridFromFile(IFileInfo file)
    {
        return GridConverter.LoadFrom(file).Builder;
    }

    private GridBuilder LoadGridFromImage(IFileInfo file)
    {
        var factory = new GridReaderFactory();
        var cells = factory.CreateGridReader(this.DebugDataFolder?.FullName).ReadGrid(file.FullName);
        GridBuilder builder = CellsToGridBuilderAdapter.ToBuilder(cells);

        return builder;
    }
}
