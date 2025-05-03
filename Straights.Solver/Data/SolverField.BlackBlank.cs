// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

public partial class SolverField
{
    public sealed class BlackBlank : SolverField
    {
        public override SolverField Clone()
        {
            // Immutable => safe to return this.
            return this;
        }

        public override WhiteFieldData? GetWhiteFieldData()
        {
            return null;
        }
    }
}
