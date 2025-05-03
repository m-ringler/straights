// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver;

using Straights.Solver.Generator;
using Straights.Solver.Simplification;

/// <summary>
/// Builds a grid generator.
/// </summary>
public class GeneratorBuilder(GeneratorBuilder.BuildEmptyGridGenerator empty)
{
    public GeneratorBuilder()
      : this(GetEmptyGridGenerator)
    {
    }

    public delegate IEmptyGridGenerator BuildEmptyGridGenerator(
        IRandom rng,
        GridLayout layout,
        GridParameters gridParameters);

    public GridParameters GridParameters { get; init; } = GridParameters.DefaultParameters;

    public GridLayout Layout { get; init; }
        = GridLayout.PointSymmetric;

    public IRandom Random { get; init; } = new SystemRandom();

    public int Attempts { get; init; } = 30;

    public int FailureThreshold { get; init; } = 50;

    public SimplifierStrength DifficultyLevel { get; init; } = SimplifierStrength.DefaultStrength;

    public static IEmptyGridGenerator GetEmptyGridGenerator(
        IRandom rng,
        GridLayout layout,
        GridParameters gridParameters)
    {
        var factory = new RandomEmptyGridGeneratorFactory(rng);
        Func<GridParameters, IEmptyGridGenerator> factoryMethod =
        layout switch
        {
            GridLayout.Random => factory.GetRandom,
            GridLayout.Uniform => factory.GetUniform,
            GridLayout.UniformIndependent => factory.GetUniformIndependent,
            GridLayout.DiagonallySymmetric => factory.GetDiagonallySymmetric,
            GridLayout.HorizontallySymmetric => factory.GetHorizontallySymmetric,
            GridLayout.VerticallySymmetric => factory.GetVerticallySymmetric,
            GridLayout.HorizontallyAndVerticallySymmetric => factory.GetHorizontallyAndVerticallySymmetric,
            GridLayout.PointSymmetric => factory.GetPointSymmetric,
            _ => throw new InvalidOperationException("Unknown grid layout " + layout),
        };

        return factoryMethod(gridParameters);
    }

    public IGridGenerator Build()
    {
        var solver = new RecursiveTrialAndErrorSolver
        {
            RandomNumberGenerator = this.Random,
            MaximumNumberOfRecursions = this.FailureThreshold,
        };

        var difficultyTuner = new DifficultyAdjuster(
            GridSimplifierFactory.BuildIterativeSimplifier(this.DifficultyLevel).ToSolver())
        {
            RandomNumberGenerator = this.Random,
        };

        var generator =
            difficultyTuner.Decorate(
                new GridGenerator(
                    solver,
                    empty(this.Random, this.Layout, this.GridParameters)));

        return generator;
    }
}
