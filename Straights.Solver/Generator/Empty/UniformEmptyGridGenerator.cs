// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator.Empty;

using System.Collections.Generic;
using Straights.Solver.Builder;
using static EmptyGridGenerator;

internal sealed class UniformEmptyGridGenerator(GridParameters gridParameters)
    : IEmptyGridGenerator
{
    public required IRandom RandomNumberGenerator { get; init; }

    public GridParameters GridParameters { get; } = gridParameters;

    public BuilderField?[][] GenerateGrid()
    {
        return EmptyGridGenerator.GenerateGrid(
            this.GridParameters,
            this.GenerateFieldIndices()
        );
    }

    private IEnumerable<FieldIndex> GenerateFieldIndices()
    {
        return PlaceFieldsUniform(
            this.RandomNumberGenerator,
            this.GridParameters.Size,
            this.GridParameters.TotalNumberOfBlackFields
        );
    }
}
