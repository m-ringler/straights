// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

internal interface IBitReader
{
    bool ReadBit();

    uint ReadNumber(int numBits);

    void ReadBits(bool[] result, int count)
    {
        if (result.Length < count)
        {
            throw new ArgumentException(
                message: $"Result array is too small. Expected {count} bits, but got {result.Length}.",
                paramName: nameof(result));
        }

        for (int i = 0; i < count; i++)
        {
            result[i] = this.ReadBit();
        }
    }
}
