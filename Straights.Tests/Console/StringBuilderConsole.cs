// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Console;

using System.Text;
using Straights.Console;

internal sealed class StringBuilderConsole : IWriteOnlyConsole
{
    private readonly StringBuilder sb = new();

    public ConsoleColor ForegroundColor { get; set; }

    public ConsoleColor BackgroundColor { get; set; }

    public void Write(string s)
    {
        _ = this.sb.Append(s);
    }

    public void Write(char c)
    {
        _ = this.sb.Append(c);
    }

    public void WriteLine()
    {
        _ = this.sb.AppendLine();
    }

    public override string ToString()
    {
        return this.sb.ToString();
    }

    public void WriteError(string s)
    {
        this.Write(s);
    }
}
