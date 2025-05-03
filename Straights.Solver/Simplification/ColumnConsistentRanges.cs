// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using System.Collections.Generic;

using Straights.Solver.Data;

/// <summary>
/// A simplifier of columns that looks at the possible ranges of all blocks
/// in the column. It retains only those ranges for which the other blocks
/// can still be solved.
/// </summary>
public sealed class ColumnConsistentRanges
    : ISimplify<SolverColumn>
{
    public void Simplify(SolverColumn item)
    {
        var unsolvedBlocks = item.Blocks.Where(b => !b.IsSolved()).ToArray();
        if (unsolvedBlocks.Length <= 1)
        {
            return;
        }

        HashSet<int>[] remainingNumbers = GetConsistentValues(unsolvedBlocks);

        for (int i = 0; i < unsolvedBlocks.Length; i++)
        {
            var impossibleValues = unsolvedBlocks[i]
                .Union()!
                .Where(v => !remainingNumbers[i].Contains(v));

            unsolvedBlocks[i].Remove(impossibleValues);
        }
    }

    /// <summary>
    /// For all unsolved blocks this method calculates the values that
    /// are consistent with the other blocks' values.
    /// </summary>
    /// <param name="unsolvedBlocks">The blocks to analyze.</param>
    /// <returns>
    /// For each of the <paramref name="unsolvedBlocks"/> the set of
    /// allowed values.
    /// </returns>
    private static HashSet<int>[] GetConsistentValues(SolverBlock[] unsolvedBlocks)
    {
        // We identify a block with its possible ranges here.
        // For example, a length-3 block with values in the 2 .. 5 range,
        // is represented as [[2, 3, 4], [3, 4, 5]].
        IntRun[][] blocks = [.. from b in unsolvedBlocks
                                select b.GetAllRanges().ToArray()];
        var remainingNumbers = new HashSet<int>[blocks.Length];
        for (int i = 0; i < blocks.Length; i++)
        {
            var block = blocks[i];
            var otherBlocks = new LinkedList<IntRun[]>(
                from b in blocks
                where b != block
                select b);
            var consistentRanges = from range in block
                                   where IsConsistent([range], otherBlocks.First!)
                                   select range;
            remainingNumbers[i] = [.. consistentRanges.SelectMany(x => x)];
        }

        return remainingNumbers;
    }

    private static bool IsConsistent(
        IEnumerable<IntRun> selectedRanges,
        LinkedListNode<IntRun[]> remainingBlocks)
    {
        var currentBlock = remainingBlocks.Value;
        var currentBlockAllowedRanges = currentBlock.Where(
            x => !selectedRanges.Any(r => r.Intersects(x)));

        var nextIterationRemainingBlocks = remainingBlocks.Next;
        if (nextIterationRemainingBlocks == null)
        {
            return currentBlockAllowedRanges.Any();
        }

        return currentBlockAllowedRanges.Any(
            range => IsConsistent(
                selectedRanges.Append(range),
                nextIterationRemainingBlocks));
    }
}