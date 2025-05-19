// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Console;

/// <summary>
/// Abstraction of a write-only color console.
/// </summary>
public interface IWriteOnlyConsole
{
    /// <summary>
    /// Gets or sets the foreground (= text) color of the console.
    /// </summary>
    ConsoleColor ForegroundColor { get; set; }

    /// <summary>
    /// Gets or sets the background color of the console.
    /// </summary>
    ConsoleColor BackgroundColor { get; set; }

    /// <summary>
    /// Writes the specified string to the console.
    /// </summary>
    /// <param name="s">The string to write.</param>
    void Write(string s);

    /// <summary>
    /// Writes the specified character to the console.
    /// </summary>
    /// <param name="c">The character to write.</param>
    void Write(char c);

    /// <summary>
    /// Writes a newline to the console.
    /// </summary>
    void WriteLine();

    /// <summary>
    /// Writes the specified string followed by a newline to the console.
    /// </summary>
    /// <param name="s">The string to write.</param>
    void WriteLine(string s)
    {
        this.Write(s);
        this.WriteLine();
    }

    /// <summary>
    /// Writes the specified error message to the console.
    /// </summary>
    /// <param name="s">The error message to write.</param>
    void WriteError(string s);

    /// <summary>
    /// Writes the specified error message followed by a newline to the console.
    /// </summary>
    /// <param name="s">The error message to write.</param>
    void WriteErrorLine(string s)
    {
        this.WriteError(s);
        this.WriteError(Environment.NewLine);
    }
}
