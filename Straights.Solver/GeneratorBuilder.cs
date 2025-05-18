// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver;

using Straights.Solver.Generator;
using Straights.Solver.Simplification;

/// <summary>
/// A builder of <see cref="IGridGenerator"/>s.
/// </summary>
/// <param name="empty">
/// The function used to create an empty grid generator.
/// </param>
public class GeneratorBuilder(GeneratorBuilder.BuildEmptyGridGenerator empty)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneratorBuilder"/> class
    /// with default empty grid generators.
    /// </summary>
    /// <seealso cref="GetEmptyGridGenerator"/>
    public GeneratorBuilder()
      : this(GetEmptyGridGenerator)
    {
    }

    /// <summary>
    /// A function that creates an empty grid generator.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <param name="layout">The grid layout to use.</param>
    /// <param name="gridParameters">The grid parameters to use.</param>
    /// <returns>
    /// An empty grid generator for the specified layout and parameters.
    /// </returns>
    public delegate IEmptyGridGenerator BuildEmptyGridGenerator(
        IRandom rng,
        GridLayout layout,
        GridParameters gridParameters);

    /// <summary>
    /// Gets the grid parameters to use.
    /// </summary>
    /// <remarks>Defaults to <see cref="GridParameters.DefaultParameters"/>.</remarks>
    public GridParameters GridParameters { get; init; } = GridParameters.DefaultParameters;

    /// <summary>
    /// Gets the grid layout to use.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="GridLayout.PointSymmetric"/>.
    /// </remarks>
    public GridLayout Layout { get; init; }
        = GridLayout.PointSymmetric;

    /// <summary>
    /// Gets the random number generator to use.
    /// </summary>
    /// <remarks>
    /// Defaults to a <see cref="SystemRandom"/> instance.
    /// </remarks>
    public IRandom Random { get; init; } = new SystemRandom();

    /// <summary>
    /// Gets the maximum number of attempts to generate a grid
    /// before giving up.
    /// </summary>
    public int Attempts { get; init; } = 30;

    /// <summary>
    /// Gets a threshold that determines when a grid generation
    /// attempt is aborted.
    /// </summary>
    /// <remarks>
    /// The default value is 50. Increase the value if you cannot
    /// generate a grid with a reasonable number of attempts.
    /// </remarks>
    public int FailureThreshold { get; init; } = 50;

    /// <summary>
    /// Gets the difficulty level of the generated grid.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="SimplifierStrength.DefaultStrength"/>.
    /// </remarks>
    public SimplifierStrength DifficultyLevel { get; init; } = SimplifierStrength.DefaultStrength;

    /// <summary>
    /// Gets the default empty grid generator for the specified layout and parameters.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <param name="layout">The grid layout to use.</param>
    /// <param name="gridParameters">The grid parameters to use.</param>
    /// <returns>
    /// An empty grid generator for the specified layout and parameters.
    /// </returns>
    /// <remarks>
    /// This is the function used by the <see cref="GeneratorBuilder()"/> constructor.
    /// </remarks>
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

    /// <summary>
    /// Builds a new <see cref="IGridGenerator"/> instance
    /// from the properties of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="IGridGenerator"/> instance.
    /// </returns>
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
