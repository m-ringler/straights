// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System;
using System.IO.Abstractions;
using Straights.Console;
using Straights.Play;
using Straights.Solver.Converter;

public sealed class ConvertCommand
{
    public required IFileInfo OutputFile { get; init; }

    public required IFileInfo InputFile { get; init; }

    public IWriteOnlyConsole Terminal { get; init; } = new Terminal();

    public Func<string?> ReadLine { get; init; } = System.Console.ReadLine;

    public Uri PlayUri { get; init; } = PlayUrl.DefaultBaseUri;

    public int Run()
    {
        var (convertibleGrid, _, _) = new GridLoader().LoadGrid(this.InputFile);
        this.WriteOutput(convertibleGrid);
        this.PrintPlayUrl(convertibleGrid);
        this.Terminal.WriteLine();
        this.Terminal.WriteLine("=> " + this.OutputFile);
        return 0;
    }

    private void PrintPlayUrl(ConvertibleGrid unsolved)
    {
        new PlayUrl { BaseUri = this.PlayUri }.PrintPlayUrl(
            this.Terminal,
            unsolved.SolverGrid
        );
    }

    private void WriteOutput(ConvertibleGrid grid)
    {
        var renderer = new ConsoleGridRenderer(this.Terminal);
        renderer.WriteToConsole(grid.SolverFieldGrid.Fields);
        grid.WriteTo(this.OutputFile);
    }
}
