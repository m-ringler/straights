// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator;

using Straights.Solver.Generator.Empty;

public class RandomEmptyGridGeneratorFactory(IRandom randomNumberGenerator)
{
    public IRandom RandomNumberGenerator { get; } = randomNumberGenerator;

    public IEmptyGridGenerator GetRandom(GridParameters gridParameters)
    {
        return new RandomEmptyGridGenerator(gridParameters)
        {
            RandomNumberGenerator = this.RandomNumberGenerator,
        };
    }

    public IEmptyGridGenerator GetUniform(GridParameters gridParameters)
    {
        return new UniformEmptyGridGenerator(gridParameters)
        {
            RandomNumberGenerator = this.RandomNumberGenerator,
        };
    }

    public IEmptyGridGenerator GetUniformIndependent(GridParameters gridParameters)
    {
        return new UniformIndependentEmptyGridGenerator(gridParameters)
        {
            RandomNumberGenerator = this.RandomNumberGenerator,
        };
    }

    public IEmptyGridGenerator GetDiagonallySymmetric(GridParameters gridParameters)
    {
        return new DiagonallySymmetricEmptyGridGenerator(gridParameters)
        {
            RandomNumberGenerator = this.RandomNumberGenerator,
        };
    }

    public IEmptyGridGenerator GetHorizontallySymmetric(GridParameters gridParameters)
    {
        return new HorizontallySymmetricEmptyGridGenerator(gridParameters)
        {
            RandomNumberGenerator = this.RandomNumberGenerator,
        };
    }

    public IEmptyGridGenerator GetVerticallySymmetric(GridParameters gridParameters)
    {
        return new VerticallySymmetricEmptyGridGenerator(
            gridParameters,
            this.RandomNumberGenerator);
    }

    public IEmptyGridGenerator GetHorizontallyAndVerticallySymmetric(GridParameters gridParameters)
    {
        return new HorizontallyAndVerticallySymmetricEmptyGridGenerator(
            gridParameters)
        {
            RandomNumberGenerator = this.RandomNumberGenerator,
        };
    }

    public IEmptyGridGenerator GetPointSymmetric(GridParameters gridParameters)
    {
        return new PointSymmetricEmptyGridGenerator(
            gridParameters)
        {
            RandomNumberGenerator = this.RandomNumberGenerator,
        };
    }
}
