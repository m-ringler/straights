// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

/// <summary>
/// Represents a hint for solving a grid, containing the field that can be simplified
/// and the simplifier that can be used.
/// </summary>
/// <param name="NumberToRemove">The number that can be removed from the field's possibilities.</param>
/// <param name="Location">The location of the field that can be simplified.</param>
/// <param name="Simplifier">The simplifier that can be used.</param>
/// <param name="IsRow">Whether the simplifier was applied to a row (true) or a column (false).</param>
public record struct Hint(
    int NumberToRemove,
    FieldIndex Location,
    Type Simplifier,
    bool IsRow);
