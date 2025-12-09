// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Solver.Generator;

public readonly record struct UnvalidatedGridConfiguration(int Size, int NumberOfBlackBlanks, int NumberOfBlackNumbers, GridLayout Layout)
{
    public int TotalNumberOfBlackFields => this.NumberOfBlackBlanks + this.NumberOfBlackNumbers;

    public static explicit operator GridParameters(UnvalidatedGridConfiguration value)
    {
        return new GridParameters(value.Size, value.NumberOfBlackBlanks, value.NumberOfBlackNumbers);
    }
}
