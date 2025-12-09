// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

using System.Collections.Immutable;

internal sealed class SolverColumnBuilder()
{
    private readonly ImmutableArray<SolverBlock>.Builder column = ImmutableArray.CreateBuilder<SolverBlock>();

    private readonly ImmutableArray<WhiteFieldData>.Builder currentBlock = ImmutableArray.CreateBuilder<WhiteFieldData>();

    public ImmutableArray<SolverColumn> CreateMany(
        IEnumerable<IEnumerable<SolverField>> columns)
    {
        return [.. from column in columns
                       select this.Create(column)];
    }

    public SolverColumn Create(
        IEnumerable<SolverField> column)
    {
        this.Clear();
        foreach (var field in column)
        {
            this.AddField(field);
        }

        return this.Drain();
    }

    public void Clear()
    {
        this.column.Clear();
        this.currentBlock.Clear();
    }

    public void AddField(SolverField field)
    {
        if (field is not SolverField.WhiteField)
        {
            this.FinishBlock();
        }

        var data = field.GetWhiteFieldData();
        if (data == null)
        {
            return;
        }

        this.currentBlock.Add(data);
        if (field is not SolverField.WhiteField)
        {
            this.FinishBlock();
        }
    }

    public SolverColumn Drain()
    {
        this.FinishBlock();
        var result = new SolverColumn
        {
            Blocks = this.column.ToImmutable(),
        };

        this.column.Clear();
        return result;
    }

    private void FinishBlock()
    {
        if (this.currentBlock.Count != 0)
        {
            SolverBlock newBlock = new()
            {
                Fields = this.currentBlock.ToImmutable(),
            };

            this.currentBlock.Clear();
            this.column.Add(newBlock);
        }
    }
}
