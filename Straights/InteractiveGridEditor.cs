// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.Globalization;
using System.Text.RegularExpressions;
using Straights.Console;
using Straights.Solver;
using Straights.Solver.Builder;

public sealed partial class InteractiveGridEditor(ReadWriteConsole console)
{
    public static bool TryParseBuilderField(
        string input,
        out BuilderField? value
    )
    {
        var match = MyRegex().Match(input);
        if (!match.Success)
        {
            value = default;
            return false;
        }

        int y = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        int x = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        var loc = new FieldLocation(x, y);

        int? fieldValue = null;
        bool isWhite = false;
        if (match.Groups[4].Success)
        {
            fieldValue = int.Parse(
                match.Groups[4].Value,
                CultureInfo.InvariantCulture
            );
            isWhite = match.Groups[3].Success;
        }

        value = new BuilderField(loc, fieldValue) { IsWhite = isWhite };
        return true;
    }

    public bool Edit(GridBuilder builder)
    {
        bool modified = false;
        var printer = this.BuildPrinter(builder);

        var (writeConsole, readLine) = console;
        while (true)
        {
            printer.Invoke();
            writeConsole.WriteLine(
                "Add black field [<y> <x> <n>?] or white field [<y> <x> w<n>]"
            );
            writeConsole.WriteLine("or [<x> <y> 0] to clear a field");
            writeConsole.WriteLine("or press ENTER when done: ");
            var str = readLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(str))
            {
                break;
            }

            if (!TryParseBuilderField(str, out var bf))
            {
                continue;
            }

            try
            {
                var value = bf!.Value;
                if (value == 0)
                {
                    builder.Clear(bf.Location);
                }
                else
                {
                    builder.Add(bf!);
                }

                modified = true;
            }
            catch (ValidationException ex)
            {
                writeConsole.WriteErrorLine(ex.Message);
            }
        }

        return modified;
    }

    [GeneratedRegex(
        @"^\s*(\d+)[\s,]+(\d+)(?:[\s,]+([wW])?(\d+))?\s*$",
        RegexOptions.Compiled
    )]
    private static partial Regex MyRegex();

    private Action BuildPrinter(GridBuilder b)
    {
        var printer = new ConsoleGridRenderer(console.Terminal);
        void Print()
        {
            var grid = b.Convert().SolverFieldGrid;
            printer.WriteToConsole(grid.Fields);
        }

        return Print;
    }
}
