// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator.Empty;

using Straights.Solver.Builder;

internal sealed class VerticallySymmetricEmptyGridGenerator(
    GridParameters gridParameters,
    IRandom rng
) : IEmptyGridGenerator
{
    private readonly HorizontallySymmetricEmptyGridGenerator core = new(
        gridParameters
    )
    {
        RandomNumberGenerator = rng,
    };

    public IRandom RandomNumberGenerator => this.core.RandomNumberGenerator;

    public GridParameters GridParameters => this.core.GridParameters;

    public BuilderField?[][] GenerateGrid()
    {
        var fields =
            from row in this.core!.GenerateGrid()
            from field in row
            where field != null
            select field;
        var result = GridBuilder.AllocateFields(this.core.GridParameters.Size);
        foreach (var item in fields)
        {
            var location = item.Location;
            var loc = new FieldLocation(location.Y, location.X);
            var field = item with { Location = loc };
            result[loc.Y - 1][loc.X - 1] = field;
        }

        return result;
    }
}
