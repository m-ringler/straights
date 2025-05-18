namespace Straights.Solver;

using Straights.Solver.Generator;
using Straights.Solver.Simplification;

/// <summary>
/// Generates game codes for Straights.Web.
/// </summary>
public static class Play
{
    /// <summary>
    /// Generates a game code for the given size and difficulty.
    /// </summary>
    /// <param name="size">The size of the grid, must be greater than or equal to 4.</param>
    /// <param name="difficulty">The difficulty level, must be grater than zero.</param>
    /// <returns>A game code.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a grid could not be generated.
    /// </exception>
    public static string GenerateGameCode(int size, int difficulty)
    {
        var difficultyLevel = (SimplifierStrength)difficulty;
        var generator = new GeneratorBuilder
        {
            GridParameters = GetGridParameters(size),
            DifficultyLevel = difficultyLevel,
            Attempts = 100,
        }
        .Build();

        var grid = generator.GenerateGrid()
            ?? throw new InvalidOperationException("Failed to generate a grid.");

        var unsolved = grid.Convert().SolverGrid;
        var solver = new EliminatingSolver();
        var solved = solver.Solve(unsolved).Convert();

        var code = GridConverter.ToUrlParameter(
            solved.SolverFieldGrid, unsolved.Grid, 128);

        return code;
    }

    private static GridParameters GetGridParameters(int size)
    {
        var result = GridConfigurationBuilder.GetUnvalidatedGridParameters(
            size: size,
            blackBlanksRaw: null,
            blackNumbersRaw: null,
            layout: GridLayout.PointSymmetric);
        return (GridParameters)result;
    }
}
