// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Builder;

/// <summary>
/// An exception that occurs when
/// the addition of a field to a <see cref="GridBuilder"/>
/// is not valid.
/// </summary>
/// <param name="message">The exception message.</param>
public class ValidationException(string message) : Exception(message) { }
