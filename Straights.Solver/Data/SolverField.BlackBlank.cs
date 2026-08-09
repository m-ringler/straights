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

        internal override void ResetFrom(SolverField other)
        {
            if (other is not BlackBlank sourceBlank)
            {
                throw new ArgumentException(
                    "Cannot reset a black blank field from a different field type.",
                    nameof(other)
                );
            }

            this.ResetFrom(sourceBlank);
        }

        private void ResetFrom(BlackBlank other)
        {
            // No internal state to reset for BlackBlank.
            _ = other;
        }
    }
}
