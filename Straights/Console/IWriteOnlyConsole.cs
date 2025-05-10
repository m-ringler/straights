// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Console;

/// <summary>
/// Abstraction of a write-only color console.
/// </summary>
public interface IWriteOnlyConsole
{
    ConsoleColor ForegroundColor { get; set; }

    ConsoleColor BackgroundColor { get; set; }

    void Write(string s);

    void Write(char c);

    void WriteLine();

    void WriteLine(string s)
    {
        this.Write(s);
        this.WriteLine();
    }

    void WriteError(string s);

    void WriteErrorLine(string s)
    {
        this.WriteError(s);
        this.WriteError(Environment.NewLine);
    }
}
