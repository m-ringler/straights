// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

using Straights.Solver.Builder;
using Straights.Solver.Data;

internal static class BuilderToSolverGridConverter
{
    public static Grid<SolverField> ToSolverFields(
        BuilderField?[][] builderFields)
    {
        int size = builderFields.Length;
        var solverFields = ImmutableArray
            .CreateBuilder<SolverField>(size * size);

        var fieldBuilder = new SolverFieldBuilder(size);
        solverFields
            .AddRange(
                from row in builderFields
                from f in row
                select fieldBuilder.ToSolverField(f));

        var grid = new Grid<SolverField>(solverFields.ToImmutable());

        return grid;
    }
}
