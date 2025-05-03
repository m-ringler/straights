// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

public partial class SolverField
{
    public sealed class BlackNumber(int number, int size)
        : SolverField
    {
        private WhiteFieldData? data;

        public int Number { get; } = number;

        public int Size { get; } = size;

        public override WhiteFieldData GetWhiteFieldData()
        {
            return this.data ??= this.CreateWhiteFieldData();
        }

        public override SolverField Clone()
        {
            return new BlackNumber(this.Number, this.Size);
        }

        private WhiteFieldData CreateWhiteFieldData()
        {
            // Generator.
            if (this.Number == 0)
            {
                return new WhiteFieldData(this.Size);
            }

            return WhiteFieldData.CreateSolved(this.Number, this.Size);
        }
    }
}
