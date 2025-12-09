// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator.Empty;

using Straights.Solver.Builder;
using static EmptyGridGenerator;

internal sealed class UniformIndependentEmptyGridGenerator(
    GridParameters gridParameters
) : IEmptyGridGenerator
{
    public required IRandom RandomNumberGenerator { get; init; }

    public GridParameters GridParameters { get; } = gridParameters;

    public BuilderField?[][] GenerateGrid()
    {
        var result = GridBuilder.AllocateFields(this.GridParameters.Size);

        var nfields = this.GridParameters.NumberOfBlackBlanks;
        var blackBlankLocations = this.PlaceFields(nfields, null);
        foreach (var index in blackBlankLocations)
        {
            SetBlackBlank(result, index);
        }

        nfields = this.GridParameters.NumberOfBlackNumbers;
        foreach (
            var index in this.PlaceFields(nfields, [.. blackBlankLocations])
        )
        {
            SetBlackNumber(result, index);
        }

        return result;
    }

    private IEnumerable<FieldIndex> PlaceFields(
        int nfields,
        HashSet<FieldIndex>? occupied
    )
    {
        return PlaceFieldsUniform(
            this.RandomNumberGenerator,
            this.GridParameters.Size,
            nfields,
            occupied == null ? _ => false : occupied.Contains
        );
    }
}
