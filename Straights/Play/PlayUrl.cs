// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Play;

using System;

using Straights.Console;
using Straights.Solver;
using Straights.Solver.Data;

internal class PlayUrl
{
    public static readonly Uri DefaultBaseUri = new("http://mospace.de/str8ts/?code=");

    public Uri BaseUri { get; init; } = DefaultBaseUri;

    public Uri GetPlayUri(
        SolverGrid unsolved,
        Func<SolverGrid> solved)
    {
        if (IsGridSizeSupported(unsolved.Grid.Size))
        {
            return new Uri(
                this.BaseUri +
                GridConverter
                    .ToUrlParameter(solved().Grid, unsolved.Grid, 128));
        }

        throw new ArgumentException(
            paramName: nameof(unsolved),
            message: $"Grid size {unsolved.Grid.Size} is not supported.");
    }

    public Uri GetPlayUri(
        SolverGrid unsolved)
    {
        return this.GetPlayUri(unsolved, () => Solve(unsolved));
    }

    public void PrintPlayUrl(
        IWriteOnlyConsole terminal,
        SolverGrid unsolved,
        Func<SolverGrid> solved)
    {
        if (IsGridSizeSupported(unsolved.Grid.Size))
        {
            Uri url;
            try
            {
                url = this.GetPlayUri(unsolved, solved);
            }
            catch (NotSolvableException)
            {
                return;
            }

            terminal.WriteLine("Play this grid at:");
            terminal.WriteLine(url.ToString());
        }
    }

    public void PrintPlayUrl(
        IWriteOnlyConsole terminal,
        SolverGrid unsolved,
        ISolver solver)
    {
        this.PrintPlayUrl(
            terminal,
            unsolved,
            () => solver.Solve(unsolved));
    }

    public void PrintPlayUrl(
        IWriteOnlyConsole terminal,
        SolverGrid unsolved)
    {
        this.PrintPlayUrl(
            terminal,
            unsolved,
            () => Solve(unsolved));
    }

    private static bool IsGridSizeSupported(int size)
    {
        return size is >= 4 and <= 12;
    }

    private static SolverGrid Solve(SolverGrid unsolved)
    {
        var solver = new EliminatingSolver();
        return solver.Solve(unsolved);
    }
}
