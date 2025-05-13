// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

using System.Globalization;
using System.Text;

using Straights.Solver.Data;

/// <summary>
/// Renders solver-field grids as HTML. The specific markup produced
/// may change in future versions.
/// The CSS is currently optimized for 9 x 9 grids.
/// </summary>
public sealed class HtmlGridRenderer
    : ITextSaver<Grid<SolverField>>
{
    private const string HtmlHead = """
            <head>
            <title>Straights</title>
            <meta name="description" content="Straights Puzzle for Printing">
            <meta name="keywords" content="Straights, Str8ts, Puzzle">
            <style>
            * {
              box-sizing: border-box;
            }
            
            div.col {
                float: left;
            }
            
            div.col div {
              float: left;
              clear: left;
              padding-left: 4.5pt;
              padding-top: 2pt;
              padding-right: 4.5pt;
              padding-bottom: 2pt;
              color: #404090;
            }
            
            table.grid {
               border-collapse: collapse;
               font-family: LucidaSans, Helvetica, Arial, sans-serif;
               font-size: 21pt;
            }
            
            tr.grid-row td
            {
               border: solid 1px #909090;
               margin: 0;
               text-align: center;
               height: 50pt;
               min-width: 50pt;
            }
            
            tr.grid-row
            {
               margin: 0;
            }
            
            td.white-field-solved
            {
               background-color: white;
               color: black;
            }
            
            td.white-field-unsolved
            {
               font-size: 8pt;
               padding-left: 0.37em;
               padding-right: 0.37em;
            }
            
            td.black-field
            {
                background-color: black;
                color: white;
            }
            </style>
            </head>
            """;

    public void Save(Grid<SolverField> grid, TextWriter writer)
    {
        writer.WriteLine(
            """
            <!DOCTYPE html>
            <html lang="en">
            """);
        writer.WriteLine(HtmlHead);

        writer.WriteLine("<body><table class=\"grid\">");
        foreach (var row in grid.GetRows())
        {
            writer.WriteLine("<tr class=\"grid-row\">");
            foreach (var field in row)
            {
                writer.Write($"<td class=\"{GetClass(field)}\">");
                writer.Write(GetCellInnerHtml(field));
                writer.WriteLine("</td>");
            }

            writer.WriteLine("</tr>");
        }

        writer.WriteLine("</table></body>");
        writer.WriteLine("</html>");
    }

    private static string GetCellInnerHtml(SolverField field)
    {
        return field switch
        {
            SolverField.WhiteField wf when wf.Data.IsSolved => wf.Data.Min.ToString(CultureInfo.InvariantCulture),
            SolverField.WhiteField wf => GetCellInnerHtml(wf.Data),
            SolverField.BlackBlank => "&nbsp;",
            SolverField.BlackNumber bn => bn.Number.ToString(CultureInfo.InvariantCulture),
            _ => throw UnknownFieldType(field),
        };
    }

    private static string GetCellInnerHtml(WhiteFieldData fieldData)
    {
        var result = new StringBuilder();
        int n = fieldData.Size;
        for (int i = 1; i <= n; i++)
        {
            if (i % 3 == 1)
            {
                _ = result.Append("<div class=\"col\">");
            }

            _ = result
                .Append("<div>")
                .Append(fieldData.Contains(i) ? i.ToString(CultureInfo.InvariantCulture) : "&nbsp;")
                .Append("</div>");

            if (i % 3 == 0 || i == n)
            {
                _ = result.AppendLine("</div>");
            }
        }

        return result.ToString();
    }

    private static string GetClass(SolverField field)
    {
        return field switch
        {
            SolverField.WhiteField wf when wf.Data.IsSolved => "white-field-solved",
            SolverField.WhiteField => "white-field-unsolved",
            SolverField.BlackBlank or SolverField.BlackNumber => "black-field",
            _ => throw UnknownFieldType(field),
        };
    }

    private static ArgumentException UnknownFieldType(SolverField field)
    {
        return new ArgumentException(
                        paramName: nameof(field),
                        message: "Unknown type of solver field: " + field.GetType());
    }
}
