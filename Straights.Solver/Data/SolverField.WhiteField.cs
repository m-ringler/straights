// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

public partial class SolverField
{
    public sealed class WhiteField(WhiteFieldData data)
        : SolverField
    {
        public WhiteFieldData Data { get; } = data;

        public override SolverField Clone()
        {
            return new WhiteField(this.Data.Clone());
        }

        public override WhiteFieldData? GetWhiteFieldData()
        {
            return this.Data;
        }
    }
}
