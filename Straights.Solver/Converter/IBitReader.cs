// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

internal interface IBitReader
{
    bool ReadBit();

    uint ReadNumber(int numBits);
}
