// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

using System.Text;

internal class BinaryStringWriter : IBitWriter
{
    private readonly StringBuilder builder = new();

    public void WriteBit(bool bit)
    {
        _ = this.builder.Append(bit ? '1' : '0');
    }

    public void WriteNumber(uint number, int numBits)
    {
        for (int i = numBits - 1; i >= 0; i--)
        {
            this.WriteBit((number & (1 << i)) != 0);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.builder.ToString();
    }
}
