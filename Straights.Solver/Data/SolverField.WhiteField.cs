// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

using Straights.Solver.Simplification;

public partial class SolverField
{
    public sealed class WhiteField(WhiteFieldData data)
        : SolverField, IGetSnapshot<int>
    {
        public WhiteFieldData Data { get; } = data;

        public override SolverField Clone()
        {
            return new WhiteField(this.Data.Clone());
        }

        public int GetSnapshot()
        {
            return this.Data.Count;
        }

        public override WhiteFieldData? GetWhiteFieldData()
        {
            return this.Data;
        }
    }
}
