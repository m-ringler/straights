// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

internal interface IBitWriter
{
    void WriteBit(bool bit);

    void WriteNumber(uint number, int numBits);

    void WriteBits(IEnumerable<bool> bits)
    {
        foreach (var bit in bits)
        {
            this.WriteBit(bit);
        }
    }
}
