// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Console;

using Core = System.Console;

/// <summary>
/// An implementation of <see cref="IWriteOnlyConsole"/>
/// based on the static <see cref="System.Console"/>.
/// </summary>
public sealed class Terminal : IWriteOnlyConsole
{
    public ConsoleColor ForegroundColor
    {
        get => Core.ForegroundColor;
        set => Core.ForegroundColor = value;
    }

    public ConsoleColor BackgroundColor
    {
        get => Core.BackgroundColor;
        set => Core.BackgroundColor = value;
    }

    public void Write(string s)
    {
        Core.Write(s);
    }

    public void Write(char c)
    {
        Core.Write(c);
    }

    public void WriteLine()
    {
        Core.WriteLine();
    }

    public void WriteLine(string s)
    {
        Core.WriteLine(s);
    }

    public void WriteError(string s)
    {
        Core.Error.Write(s);
    }

    public void WriteErrorLine(string s)
    {
        Core.Error.WriteLine(s);
    }
}