namespace Straights.Solver;

using Straights.Solver.Generator;
using Straights.Solver.Simplification;

public static class Play
{
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
