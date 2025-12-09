// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using Straights.Solver.Data;

/// <summary>
/// Detects fields that contain exactly two admissible values
/// whose difference is greater than or equal to the number of fields
/// in the <see cref="SolverBlock"/>, and eliminates these values
/// from the other fields in the block.
/// </summary>
/// <remarks>
/// This is a special case of the
/// <see cref="BlockNFieldsWithNValuesInCertainRange"/>
/// rule.
/// </remarks>
public sealed class BlockTwoValuesFarApart : ISimplify<SolverBlock>
{
    public void Simplify(SolverBlock item)
    {
        var twoValuesFarApart = FindFieldsWithTwoValuesFarApart(item);

        foreach (var (field, min, max) in twoValuesFarApart)
        {
            item.Fields.Where(x => x != field).Remove([min, max]);
        }
    }

    private static IReadOnlyCollection<TwoValueField> FindFieldsWithTwoValuesFarApart(
        SolverBlock item
    )
    {
        int n = item.Count;
        var twoValuesFarApart =
            from field in item
            where field.Count == 2
            select new TwoValueField(
                field,
                field.Min,
                field.Max
            ) into twoValueField
            where twoValueField.Max - twoValueField.Min >= n
            select twoValueField;
        return [.. twoValuesFarApart];
    }

    private readonly record struct TwoValueField(
        WhiteFieldData Field,
        int Min,
        int Max
    );
}
