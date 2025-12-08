// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

/// <summary>
/// An exception that occurs when
/// the last possible number is removed from a field.
/// </summary>
/// <param name="message">The exception message.</param>
public class NotSolvableException(string message)
    : InvalidOperationException(message)
{
}
