// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

using System.Globalization;
using Straights.Solver.Builder;

/// <summary>
/// Saves and loads <see cref="GridBuilder"/>s as plain text.
/// </summary>
public sealed class GridBuilderTextPersister
    : ITextPersister<GridBuilder>
{
    public void Save(GridBuilder grid, TextWriter writer)
    {
        var fields = grid.GetFields();
        writer.WriteLine(grid.Size);
        for (int i = 0; i < grid.Size; i++)
        {
            writer.WriteLine(GetStringForSave(fields[i]));
        }
    }

    public GridBuilder Load(TextReader reader)
    {
        string sizeLine = ReadLine(reader)
         ?? throw new IOException("End of input while looking for grid size.");

        int size = int.Parse(sizeLine.Trim(), CultureInfo.InvariantCulture);
        var result = new GridBuilder(size);
        for (int i = 0; i < size; i++)
        {
            LoadRow(result, i, ReadLine(reader) ?? string.Empty);
        }

        return result;
    }

    private static string? ReadLine(TextReader reader)
    {
        string? line;
        do
        {
            line = reader.ReadLine();
        }
        while (line?.StartsWith('#') == true);

        return line;
    }

    private static void LoadRow(GridBuilder result, int i, string v)
    {
        var fields = v.Split(',');
        for (int k = 0; k < fields.Length; k++)
        {
            var field = fields[k];
            if (field.Length == 0)
            {
                continue;
            }

            int y = i + 1;
            int x = k + 1;
            if (field[0] == 'w' && int.TryParse(field.AsSpan(1), out int value))
            {
                result.SetWhite(y, x, value);
            }

            if (field[0] == 'b')
            {
                if (field.Length == 1)
                {
                    result.SetBlack(y, x);
                }
                else if (int.TryParse(field.AsSpan(1), out value))
                {
                    result.SetBlack(y, x, value);
                }
            }
        }
    }

    private static string GetStringForSave(BuilderField?[] builderFields)
    {
        return string.Join(
            ",",
            from f in builderFields select GetStringForSave(f));
    }

    private static string GetStringForSave(BuilderField f)
    {
        if (f == null)
        {
            return "_";
        }

        if (f.IsWhite)
        {
            return f.Value == null ? "_" : "w" + f.Value;
        }

        if (f.Value.HasValue)
        {
            return "b" + f.Value;
        }

        return "b";
    }
}
