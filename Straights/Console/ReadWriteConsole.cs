// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Console;

/// <summary>
/// A structure that represents a console that can be read from and written to.
/// </summary>
/// <param name="Terminal">The write interface of the console.</param>
/// <param name="ReadLine">The read-line function of the console.</param>
public readonly record struct ReadWriteConsole(
    IWriteOnlyConsole Terminal,
    Func<string?> ReadLine);