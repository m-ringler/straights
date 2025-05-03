// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Console;

public readonly record struct ReadWriteConsole(
    IWriteOnlyConsole Terminal,
    Func<string?> ReadLine);