// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.Diagnostics;
using System.IO.Abstractions;

using Straights.Console;
using Straights.Solver;
using Straights.Solver.Builder;
using Straights.Solver.Converter;
using Straights.Solver.Data;
using Straights.Solver.Simplification;

public sealed class SolveCommand(IFileSystem fileSystem)
{
    public enum SolverMode
    {
        SimplifyOnly,
        UniqueSolution,
        AnySolution,
    }

    public bool MultiThreaded { get; init; }

    public bool PrintIterations { get; init; }

    public bool Interactive { get; init; }

    public IFileInfo? File { get; init; }

    public SolverMode Mode { get; init; } = SolverMode.UniqueSolution;

    public IDirectoryInfo? DebugDataFolder { get; init; }

    public IFileInfo? OutputFile { get; init; }

    public IWriteOnlyConsole Terminal { get; init; } = new Terminal();

    public Func<string?> ReadLine { get; init; } = System.Console.ReadLine;

    private ReadWriteConsole Console => new(this.Terminal, this.ReadLine);

    public int Run()
    {
        var (grid, fromImage, suggestedPath)
            = this.GridInitializer().InitializeGrid(this.File);

        if (this.Interactive)
        {
            var builder = grid.Builder;
            bool askToSave = this.EditGrid(builder) || fromImage;

            if (askToSave)
            {
                new InteractiveGridSaver(fileSystem, this.Console)
                    .SaveGrid(builder, suggestedPath);
            }

            grid = builder.Convert();
        }

        SolverGrid data = grid.SolverGrid;
        var gridPrinter = this.BuildGridPrinter(() => data);
        if (!this.Interactive)
        {
            gridPrinter.Invoke();
        }

        var iterativeGridSimplifier = this.BuildSimplifier(
            this.PrintIterations ? gridPrinter : null);

        Stopwatch watch = new();
        try
        {
            watch.Start();
            data = this.Solve(data, iterativeGridSimplifier);
            watch.Stop();
        }
        finally
        {
            this.Terminal.WriteLine();
            gridPrinter.Invoke();
            this.Terminal.WriteLine();
        }

        this.PrintStatus(data, watch.Elapsed);

        this.WriteOutput(data);

        return data.IsSolved ? 0 : 1;
    }

    private InteractiveGridInitializer GridInitializer()
    {
        return new InteractiveGridInitializer(this.Console)
        {
            DebugDataFolder = this.DebugDataFolder,
        };
    }

    private void WriteOutput(SolverGrid data)
    {
        if (this.OutputFile != null)
        {
            ConvertibleGrid grid = new(data);
            grid.WriteTo(this.OutputFile);
        }
    }

    private SolverGrid Solve(SolverGrid dataIn, ISimplify<SolverGrid> iterativeGridSimplifier)
    {
        dataIn = iterativeGridSimplifier.ToSolver().Solve(dataIn);

        if (this.Mode == SolverMode.SimplifyOnly || dataIn.IsSolved)
        {
            return dataIn;
        }

        this.Terminal.WriteLine();
        this.Terminal.WriteLine($"Mode {SolverMode.SimplifyOnly} did not find a solution.");
        var solver1 = new EliminatingSolver(iterativeGridSimplifier);
        var data1 = solver1.Solve(dataIn);

        if (this.Mode == SolverMode.UniqueSolution || data1.IsSolved)
        {
            return data1;
        }

        this.Terminal.WriteLine();
        this.Terminal.WriteLine($"Mode {SolverMode.UniqueSolution} did not find a solution.");
        var solver2 = new RecursiveTrialAndErrorSolver(iterativeGridSimplifier)
        {
            RandomNumberGenerator = new RandNRandomFactory().CreatePcg32(),
        };

        var data2 = solver2.Solve(dataIn);

        return data2;
    }

    private bool EditGrid(GridBuilder builder)
    {
        return new InteractiveGridEditor(this.Console)
                        .Edit(builder);
    }

    private void PrintStatus(SolverGrid data, TimeSpan elapsedTime)
    {
        var elapsedMillis = elapsedTime.TotalMilliseconds;
        string msg = data.IsSolved
            ? $"Solved in {elapsedMillis} ms."
            : $"Failed to solve grid (grid unchanged after {elapsedMillis} ms).";
        this.Terminal.WriteLine(msg);
    }

    private ISimplify<SolverGrid> BuildSimplifier(
        Action? printGrid)
    {
        Action<int> onBeginIteration;
        Action<int> onEndIteration;
        int k = 0;
        if (printGrid == null)
        {
            onBeginIteration = i => this.Terminal.Write($"\rIteration {++k}");
            onEndIteration = _ => { };
        }
        else
        {
            onBeginIteration = i => this.Terminal.WriteLine($"\rIteration {++k}");
            onEndIteration = _ => printGrid.Invoke();
        }

        var opts = new SimplifierOptions { MultiThreaded = this.MultiThreaded };
        var factory = new GridSimplifierFactory(opts);
        return factory.BuildIterativeSimplifier(
            SimplifierStrength.MaxStrength,
            onBeginIteration,
            onEndIteration);
    }

    private Action BuildGridPrinter(Func<SolverGrid> data)
    {
        var renderer = new ConsoleGridRenderer(this.Terminal);

        void WriteToConsole()
        {
            renderer.WriteToConsole(data().Grid.Fields);
        }

        var folder = this.DebugDataFolder;
        if (folder == null)
        {
            return WriteToConsole;
        }
        else
        {
            int i = 1;
            void WriteHtml()
            {
                var path = fileSystem.Path.Combine(
                    folder.FullName,
                    $"grid_{i++:0000}.html");
                var file = fileSystem.FileInfo.New(path);
                data().Convert().WriteTo(file);
            }

            return () =>
            {
                WriteToConsole();
                WriteHtml();
            };
        }
    }
}
