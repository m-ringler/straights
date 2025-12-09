// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using Straights.Solver.Builder;
using Straights.Solver.Generator;

internal record struct BlackFieldCount(int Numbers, int Blanks)
{
    public static implicit operator BlackFieldCount(GridParameters p)
    {
        return new BlackFieldCount(
            p.NumberOfBlackNumbers,
            p.NumberOfBlackBlanks
        );
    }

    public static BlackFieldCount operator +(
        BlackFieldCount a,
        BlackFieldCount b
    )
    {
        return new BlackFieldCount(a.Numbers + b.Numbers, a.Blanks + b.Blanks);
    }

    public static BlackFieldCount Of(BuilderField? bf)
    {
        return (bf?.IsWhite, bf?.Value) switch
        {
            (false, null) => new(0, 1),
            (false, _) => new(1, 0),
            _ => new(0, 0),
        };
    }

    public static BlackFieldCount Of(GridBuilder builder)
    {
        return Of(builder.EnumerateFields());
    }

    public static BlackFieldCount Of(BuilderField?[][] builderFields)
    {
        return Of(builderFields.SelectMany(x => x));
    }

    public static BlackFieldCount Of(IEnumerable<BuilderField?> builderFields)
    {
        BlackFieldCount zero = new(0, 0);
        var count = builderFields.Aggregate(zero, (cnt, bf) => cnt + Of(bf));
        return count;
    }
}
