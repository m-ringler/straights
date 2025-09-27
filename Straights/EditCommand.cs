// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.IO.Abstractions;

using Straights.Console;
using Straights.Play;
using Straights.Solver;
using Straights.Solver.Builder;

public sealed class EditCommand(IFileSystem fileSystem)
{
    public IWriteOnlyConsole Terminal { get; init; } = new Terminal();

    public Func<string?> ReadLine { get; init; } = System.Console.ReadLine;

    public IFileInfo? File { get; init; }

    private ReadWriteConsole Console => new(this.Terminal, this.ReadLine);

    public int Run()
    {
        var (builder, suggestedSavePath) = this.InitializeGrid(this.File);

        _ = this.EditGrid(builder);

        this.SaveGrid(builder, suggestedSavePath);

        new PlayUrl().PrintPlayUrl(
            this.Terminal,
            builder.Convert().SolverGrid);

        return 0;
    }

    private void SaveGrid(GridBuilder builder, string? suggestedPath)
    {
        var saver =
        new InteractiveGridSaver(fileSystem, this.Console);

        while (true)
        {
            try
            {
                saver.SaveGrid(builder, suggestedPath);
                return;
            }
            catch (IOException ex)
            {
                this.Terminal.WriteErrorLine(ex.Message);
            }
        }
    }

    private bool EditGrid(GridBuilder builder)
    {
        return new InteractiveGridEditor(this.Console)
            .Edit(builder);
    }

    private (GridBuilder Builder, string? SuggestedSavePath)
        InitializeGrid(IFileInfo? args)
    {
        var (grid, _, suggestedSavePath) =
            new InteractiveGridInitializer(this.Console)
            .InitializeGrid(args);

        return (grid.Builder, suggestedSavePath);
    }
}
