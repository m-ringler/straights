// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

internal class BinaryStringReader(string data) : IBitReader
{
    private readonly string data = data;
    private int index;

    public bool ReadBit()
    {
        if (this.index >= this.data.Length)
        {
            throw new EndOfStreamException();
        }

        return this.data[this.index++] == '1';
    }

    public uint ReadNumber(int numBits)
    {
        uint number = 0;
        for (int i = numBits - 1; i >= 0; i--)
        {
            if (this.ReadBit())
            {
                number |= (uint)(1 << i);
            }
        }

        return number;
    }
}
