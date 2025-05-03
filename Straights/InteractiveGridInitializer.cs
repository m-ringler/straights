// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.IO.Abstractions;

using Straights.Console;
using Straights.Solver.Builder;
using Straights.Solver.Data;

internal sealed class InteractiveGridInitializer(ReadWriteConsole console)
{
    public IDirectoryInfo? DebugDataFolder { get; init; }

    public (GridBuilder Builder, bool IsImage, string? SuggestedSavePath)
        InitializeGrid(IFileInfo? args)
    {
        string? path = args?.FullName;

        return string.IsNullOrWhiteSpace(path)
            ? this.NewEmptyGridBuilder()
            : new GridLoader { DebugDataFolder = this.DebugDataFolder }.LoadGrid(args!);
    }

    public (GridBuilder Builder, bool IsImage, string? SuggestedPath) NewEmptyGridBuilder()
    {
        return (new GridBuilder(this.ReadSize()), false, null);
    }

    private int ReadSize()
    {
        var (terminal, readLine) = console;
        while (true)
        {
            terminal.Write("Size: ");
            var sizeAsString = readLine();
            if (int.TryParse(sizeAsString, out int size)
                     && size > 0
                     && size <= WhiteFieldData.MaxSize)
            {
                return size;
            }
        }
    }
}
