// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

using Straights.Solver.Builder;

using static Straights.Solver.Data.SolverField;

internal sealed class SolverFieldBuilder(int size)
{
    private readonly WhiteFieldData unsolved = new(size);

    private readonly BlackBlank blackBlank = new();

    public SolverField ToSolverField(BuilderField? source)
    {
        return source switch
        {
            null => this.WhiteUnsolved(),
            BuilderField f => this.ToSolverFieldImpl(f),
        };
    }

    public WhiteField WhiteNumber(int n)
    {
        var result = WhiteFieldData.CreateSolved(n, size);
        return new WhiteField(result);
    }

    private SolverField ToSolverFieldImpl(
        BuilderField f)
    {
        return (f.IsWhite, f.Value.HasValue) switch
        {
            (true, true) => this.WhiteNumber(f.Value!.Value),
            (true, false) => this.WhiteUnsolved(),
            (false, true) => new BlackNumber(f.Value!.Value, size),
            (false, false) => this.blackBlank,
        };
    }

    private WhiteField WhiteUnsolved()
    {
        var result = this.unsolved.Clone();
        return new WhiteField(result);
    }
}