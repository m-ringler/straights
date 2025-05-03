// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using Straights.Solver.Data;

/// <summary>
/// A simplifier of <see cref="SolverColumn"/>s that finds
/// the certain ranges of values of all blocks in the column
/// and removes these values from the other blocks in the column.
/// </summary>
/// <remarks>
/// The certain range of a block with N fields is the intersection
/// between the smallest and greatest consecutive sequences of N values
/// among the admissible values of the block's fields.
/// </remarks>
public sealed class ColumnRemoveForeignRanges
: ISimplify<SolverColumn>
{
    public void Simplify(SolverColumn item)
    {
        if (item.Blocks.Length < 2)
        {
            return;
        }

        var certainRanges = item.Blocks
            .Select(block => (block, range: block.FindCertainRange()))
            .Where(x => !x.range.IsEmpty)
            .ToArray();

        if (certainRanges.Length == 0)
        {
            return;
        }

        foreach (var currentBlock in item.Blocks)
        {
            foreach (var otherBlockCertainRange in
                certainRanges.Where(x => x.block != currentBlock)
                .Select(x => x.range))
            {
                currentBlock.Remove(otherBlockCertainRange);
            }
        }
    }
}
