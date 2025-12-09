// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

using System.Collections;
using System.Collections.Immutable;
using System.Text;
using Straights.Solver.Simplification;

public class SolverColumn
    : IEnumerable<SolverBlock>,
        IGetSnapshot<int>,
        IReadOnlyCollection<WhiteFieldData>
{
    public ImmutableArray<SolverBlock> Blocks { get; init; }

    public IEnumerable<WhiteFieldData> Fields =>
        this.Blocks.SelectMany(x => x.Fields);

    int IReadOnlyCollection<WhiteFieldData>.Count =>
        this.Blocks.Sum(x => x.Count);

    public static SolverColumn Create(int gridSize, int[][][] blocks)
    {
        var fullRange = Enumerable.Range(1, gridSize);

        var blockBuilder = ImmutableArray.CreateBuilder<WhiteFieldData>();
        var columnBuilder = ImmutableArray.CreateBuilder<SolverBlock>();
        foreach (var block in blocks)
        {
            foreach (var field in block)
            {
                var f = new WhiteFieldData(gridSize);
                f.Remove(fullRange.Except(field));
                blockBuilder.Add(f);
            }

            var b = new SolverBlock { Fields = blockBuilder.ToImmutable() };
            columnBuilder.Add(b);
            blockBuilder.Clear();
        }

        return new SolverColumn { Blocks = columnBuilder.ToImmutable() };
    }

    public IEnumerator<SolverBlock> GetEnumerator()
    {
        IEnumerable<SolverBlock> blocks = this.Blocks;
        return blocks.GetEnumerator();
    }

    public int GetSnapshot()
    {
        return this.TotalCount();
    }

    public string DumpCode()
    {
        var builder = new StringBuilder("SolverColumn.Create(");
        _ = builder.Append(this.GridSize());
        _ = builder.AppendLine(",");
        _ = builder.Append('[');
        if (this.Blocks.Any())
        {
            AppendBlock(builder, this.Blocks[0]);
            foreach (var block in this.Blocks.Skip(1))
            {
                _ = builder.AppendLine(",");
                AppendBlock(builder, block);
            }
        }

        _ = builder.Append("])");
        return builder.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    IEnumerator<WhiteFieldData> IEnumerable<WhiteFieldData>.GetEnumerator()
    {
        return this.Fields.GetEnumerator();
    }

    private static void AppendBlock(StringBuilder builder, SolverBlock block)
    {
        _ = builder.Append('[');
        if (block.Fields.Any())
        {
            AppendField(builder, block.Fields[0]);
            foreach (var field in block.Fields.Skip(1))
            {
                _ = builder.AppendLine(",");
                AppendField(builder, field);
            }
        }

        _ = builder.Append(']');
    }

    private static void AppendField(
        StringBuilder builder,
        WhiteFieldData whiteFieldData
    )
    {
        _ = builder.Append('[');
        _ = builder.AppendJoin(", ", whiteFieldData);
        _ = builder.Append(']');
    }
}
