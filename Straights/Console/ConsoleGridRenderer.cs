// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Console;

using System.Globalization;
using Straights.Solver.Data;

/// <summary>
/// Writes a square grid of <see cref="SolverField"/>s
/// to a color console.
/// </summary>
/// <param name="console">The console to write to.</param>
public sealed class ConsoleGridRenderer(IWriteOnlyConsole console)
{
    /// <summary>
    /// Writes the specified grid to the console.
    /// </summary>
    /// <param name="grid">The grid fields.</param>
    public void WriteToConsole(IReadOnlyCollection<SolverField> grid)
    {
        int size = (int)Math.Sqrt(grid.Count);
        var fields = grid.Select(GetRenderData).ToList();
        var maxLength = fields.Max(x => x.Text.Length);
        var e = fields.GetEnumerator();

        var neutral = ColorPair.Capture(console);
        for (int row = 0; row < size; row++)
        {
            this.PrintCellBoundary();

            for (int col = 0; col < size; col++)
            {
                _ = e.MoveNext();
                var (text, colors) = e.Current;

                this.Print(text, maxLength, colors);
                this.PrintCellBoundary();
            }

            neutral.SetTo(console);
            console.WriteLine();
        }
    }

    private static (string Text, ColorPair Colors) GetRenderData(SolverField sf)
    {
        return sf switch
        {
            SolverField.BlackBlank => (" ", ColorPair.WhiteOnBlack),

            SolverField.BlackNumber bn => (
                bn.Number.ToString(CultureInfo.InvariantCulture),
                ColorPair.WhiteOnBlack
            ),

            SolverField.WhiteField wf => (
                wf.GetWhiteFieldData()!.ToCompactString(),
                ColorPair.BlackOnWhite
            ),

            _ => throw new InvalidOperationException(
                "Unknown field type:" + sf.GetType()
            ),
        };
    }

    private void PrintCellBoundary()
    {
        ColorPair.BlackOnWhite.SetTo(console);
        console.Write('│');
    }

    private void Print(string text, int width, ColorPair colors)
    {
        colors.SetTo(console);
        console.Write(' ');

        console.BackgroundColor = colors.AltBackground;
        for (int i = 0; i < width; i++)
        {
            var c = i < text.Length ? text[i] : ' ';
            console.Write(c);
        }

        colors.SetTo(console);
        console.Write(' ');
    }

    private readonly record struct ColorPair(
        ConsoleColor Foreground,
        ConsoleColor Background
    )
    {
        public ConsoleColor AltBackground =>
            this.Background == ConsoleColor.White
                ? ConsoleColor.Gray
                : this.Background;

        public static ColorPair WhiteOnBlack { get; } =
            new(ConsoleColor.White, ConsoleColor.Black);

        public static ColorPair BlackOnWhite { get; } =
            new(ConsoleColor.Black, ConsoleColor.White);

        public static ColorPair Capture(IWriteOnlyConsole c)
        {
            return new(c.ForegroundColor, c.BackgroundColor);
        }

        public void SetTo(IWriteOnlyConsole c)
        {
            c.ForegroundColor = this.Foreground;
            c.BackgroundColor = this.Background;
        }
    }
}
