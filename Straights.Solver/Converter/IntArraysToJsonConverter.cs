// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

using System.Diagnostics;
using System.Globalization;
using System.Text;

internal static class IntArraysToJsonConverter
{
    private const string JsonIndent = "  ";

    public static string ToJson(int[][][] rows)
    {
        var result = new StringBuilder();
        _ = result.Append('[');
        string rowDelimiter = string.Empty;
        for (int iy = 0; iy < rows.Length; iy++)
        {
            _ = result.AppendLine(rowDelimiter);
            var row = rows[iy];
            AppendRow(result, row);
            rowDelimiter = ",";
        }

        _ = result.AppendLine();
        _ = result.AppendLine("]");
        return result.ToString();
    }

    public static int[][][] IntArraysFromJson(string json)
    {
        var result = new List<int[][]>();

        int bracketCount = 0;
        List<int[]> row = [];
        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];
            if (char.IsWhiteSpace(c))
            {
                continue;
            }

            if (bracketCount < 3)
            {
                int newBracketCount = GetNewBracketCount(bracketCount, c);
                bool bracketClosed = newBracketCount < bracketCount;
                bracketCount = newBracketCount;
                if (bracketClosed)
                {
                    switch (bracketCount)
                    {
                        case 0: // grid complete
                            return [.. result];
                        case 1: // row complete
                            result.Add([.. row]);
                            row = [];
                            break;
                        default:
                            Debug.Fail("We should never get here.");
                            break;
                    }
                }

                continue;
            }

            if (bracketCount == 3)
            {
                int begin = i;
                bool found = ScanToClosingBracket(json, ref i);
                if (!found)
                {
                    break;
                }

                int n = i - begin;
                bracketCount--;

                int[] field;
                if (n == 0)
                {
                    field = [];
                }
                else
                {
                    var array = json.AsSpan().Slice(begin, n);
                    field = [.. ReadNumbers(array)];
                }

                row.Add(field);
            }
        }

        throw new ArgumentException(
            paramName: nameof(json),
            message: $"Missing closing bracket ]."
        );
    }

    private static IEnumerable<int> ReadNumbers(ReadOnlySpan<char> array)
    {
        var items = array.Split(',');
        var result = new List<int>();
        foreach (Range range in items)
        {
            var item = array[range];
            var token = SkipLeadingWhitespace(item);
            if (token.Length != 0)
            {
                result.Add(int.Parse(token, CultureInfo.InvariantCulture));
            }
        }

        return result;
    }

    private static ReadOnlySpan<char> SkipLeadingWhitespace(
        ReadOnlySpan<char> token
    )
    {
        int j = 0;
        for (; j < token.Length; j++)
        {
            if (!char.IsWhiteSpace(token[j]))
            {
                break;
            }
        }

        token = token[j..];
        return token;
    }

    private static bool ScanToClosingBracket(string json, ref int i)
    {
        bool found = false;
        for (; i < json.Length; i++)
        {
            char x = json[i];
            if (x == ']')
            {
                found = true;
                break;
            }
        }

        return found;
    }

    private static int GetNewBracketCount(int bracketCount, char c)
    {
        return c switch
        {
            '[' => bracketCount + 1,
            ']' => bracketCount - 1,
            ',' => bracketCount,
            _ => throw new ArgumentException(
                $"Unexpected character {c}: Expecting one of '[],'."
            ),
        };
    }

    private static void AppendRow(StringBuilder result, int[][] row)
    {
        // Fully solved row
        if (row.All(x => x.Length == 1))
        {
            _ = result.Append(JsonIndent).Append('[');
            var fieldDelimiter = string.Empty;
            foreach (var field in row)
            {
                _ = result.Append(fieldDelimiter);
                _ = result.Append('[').Append(field[0]).Append(']');
                fieldDelimiter = ", ";
            }

            _ = result.Append(']');
            return;
        }

        // Partially unsolved row
        var fieldDelim = string.Empty;
        _ = result.Append(JsonIndent).Append('[');
        foreach (var field in row)
        {
            _ = result.AppendLine(fieldDelim);
            _ = result.Append(JsonIndent).Append(JsonIndent);
            fieldDelim = ",";
            _ = result.Append('[');
            _ = result.Append(string.Join(", ", field));
            _ = result.Append(']');
        }

        _ = result.AppendLine().Append(JsonIndent).Append(']');
    }
}
