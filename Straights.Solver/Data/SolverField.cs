// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

/// <summary>
/// A <see cref="SolverField"/> is a union type
/// of
/// <see cref="BlackBlank"/>,
/// <see cref="BlackNumber" />, and
/// <see cref="WhiteField"/>.
/// </summary>
public abstract partial class SolverField
{
    private SolverField() { }

    public abstract WhiteFieldData? GetWhiteFieldData();

    public abstract SolverField Clone();
}
