// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System;
using System.Diagnostics;
using System.IO.Abstractions;
using Straights.Console;
using Straights.Play;
using Straights.Solver;
using Straights.Solver.Builder;
using Straights.Solver.Converter;
using Straights.Solver.Generator;
using Straights.Solver.Simplification;

public sealed class GenerateCommand
{
    public required GridParameters GridParameters { get; init; }

    public GridLayout Layout { get; init; } =
        GenerateCommandBuilder.DefaultLayout;

    public required (IRandom Rng, string Seed) Random { get; init; }

    public int Attempts { get; init; } = GenerateCommandBuilder.DefaultAttempts;

    public int FailureThreshold { get; init; } =
        GenerateCommandBuilder.DefaultFailureThreshold;

    public IFileInfo? OutputFile { get; init; }

    public IFileInfo? Template { get; init; }

    public SimplifierStrength DifficultyLevel { get; init; } =
        SimplifierStrength.DefaultStrength;

    public IWriteOnlyConsole Terminal { get; init; } = new Terminal();

    public Func<string?> ReadLine { get; init; } = System.Console.ReadLine;

    public Uri PlayUri { get; init; } = PlayUrl.DefaultBaseUri;

    public int Run()
    {
        Stopwatch watch = new();
        GridBuilder? result = default;
        try
        {
            var generator = new GeneratorBuilder(
                this.GetBuildEmptyGridGenerator()
            )
            {
                GridParameters = this.GridParameters,
                Layout = this.Layout,
                Random = this.Random.Rng,
                Attempts = this.Attempts,
                FailureThreshold = this.FailureThreshold,
                DifficultyLevel = this.DifficultyLevel,
            }.Build();

            watch.Start();
            result = generator.GenerateGrid();
        }
        catch (Exception)
        {
            this.PrintSeed();
            throw;
        }
        finally
        {
            watch.Stop();
        }

        if (result == null)
        {
            this.PrintStatus(false, watch.Elapsed);
            return 1;
        }

        this.PrintStatus(true, watch.Elapsed);

        var convertibleGrid = new ConvertibleGrid(result);

        this.WriteOutput(convertibleGrid);

        new PlayUrl { BaseUri = this.PlayUri }.PrintPlayUrl(
            this.Terminal,
            convertibleGrid.SolverGrid
        );

        return 0;
    }

    private GeneratorBuilder.BuildEmptyGridGenerator GetBuildEmptyGridGenerator()
    {
        return this.Template == null
            ? GeneratorBuilder.GetEmptyGridGenerator
            : (_, _, _) => this.LoadTemplate();
    }

    private IEmptyGridGenerator LoadTemplate()
    {
        var (grid, _, _) = new GridLoader().LoadGrid(this.Template!);

        return new FixedEmptyGridGenerator(grid.Builder);
    }

    private void WriteOutput(ConvertibleGrid grid)
    {
        var file = this.OutputFile;
        this.PrintGrid(grid);
        this.PrintSeed();
        if (file != null)
        {
            grid.WriteTo(file);
        }
    }

    private void PrintSeed()
    {
        this.Terminal.WriteLine($"Seed: {this.Random.Seed}");
    }

    private void PrintStatus(bool success, TimeSpan elapsedTime)
    {
        var elapsedMillis = elapsedTime.TotalMilliseconds;
        string msg = success
            ? $"Generated a grid in {elapsedMillis} ms."
            : $"Failed to generate a grid in {elapsedMillis} ms.";
        this.Terminal.WriteLine(msg);
    }

    private void PrintGrid(ConvertibleGrid data)
    {
        var renderer = new ConsoleGridRenderer(this.Terminal);
        renderer.WriteToConsole(data.SolverFieldGrid.Fields);
    }
}
