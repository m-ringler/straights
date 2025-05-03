// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator;

using Straights.Solver.Builder;

using static Straights.Solver.Generator.Empty.EmptyGridGenerator;

public class FixedEmptyGridGenerator(GridBuilder template)
    : IEmptyGridGenerator
{
    public GridBuilder Template { get; } = template;

    public BuilderField?[][] GenerateGrid()
    {
        var result = new GridBuilder(this.Template.Size).GetFields();

        var blackFields = from row in this.Template.GetFields()
                          from field in row
                          where field?.IsWhite == false
                          select field!;

        foreach (var field in blackFields)
        {
            var location = field.Location;
            FieldIndex index = new(location.X - 1, location.Y - 1);
            if (field.Value != null)
            {
                SetBlackNumber(result, index);
            }
            else
            {
                SetBlackBlank(result, index);
            }
        }

        return result;
    }
}
