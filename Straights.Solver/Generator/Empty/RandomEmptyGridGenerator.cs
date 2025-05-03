// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator.Empty;

using Straights.Solver.Builder;

using static EmptyGridGenerator;

internal sealed class RandomEmptyGridGenerator(GridParameters gridParameters)
    : IEmptyGridGenerator
{
    public required IRandom RandomNumberGenerator { get; init; }

    public GridParameters GridParameters { get; } = gridParameters;

    public BuilderField?[][] GenerateGrid()
    {
        var result = GridBuilder.AllocateFields(this.GridParameters.Size);

        for (int i = 0; i < this.GridParameters.NumberOfBlackBlanks; i++)
        {
            var index = this.GetFreeRandomLocation(result);
            SetBlackBlank(result, index);
        }

        for (int i = 0; i < this.GridParameters.NumberOfBlackNumbers; i++)
        {
            var index = this.GetFreeRandomLocation(result);
            SetBlackNumber(result, index);
        }

        return result;
    }

    private FieldIndex GetFreeRandomLocation(BuilderField?[][] fields)
    {
        return EmptyGridGenerator.GetFreeRandomLocation(
            this.RandomNumberGenerator,
            this.GridParameters.Size,
            index => fields[index.Y][index.X] != null);
    }
}
