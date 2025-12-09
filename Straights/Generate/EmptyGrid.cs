// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Generate;

using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using Straights;
using Straights.Solver.Generator;
using static Straights.Solver.Generator.GridParameters;

internal class EmptyGrid
{
    public const GridLayout DefaultLayout = GridLayout.Uniform;
    public static readonly int DefaultSize = DefaultParameters.Size;

    private const string SizeOptionDescription = """
        The size of the grid to generate.
        """;

    private const string TemplateOptionDescription = """
        A file (.png, .jpg, .txt, .json) with a straights grid
        to use as a template. The numbers in the grid will be ignored,
        but its layout will be reused.
        """;

    private const string LayoutOptionDescription = """
        The strategy used to place the black fields on the grid.
        """;

    private static readonly string BlackBlanksOptionDescription = $"""
        The number of black blanks to generate. Must be less than or equal to
        {MaximumPercentageOfBlackBlanks} % of the number of fields.
        The default value of this option depends on the size and layout.
        """;

    private static readonly string BlackNumbersOptionDescription = $"""
        The number of black numbers to generate. Must be less than or equal to
        {MaximumPercentageOfBlackNumbers} % of the number of fields.
        The default value of this option depends on the size and layout.
        """;

    private readonly Option<int> sizeOption = new(name: "--size")
    {
        Description = SizeOptionDescription,
        DefaultValueFactory = _ => DefaultSize,
        Arity = ArgumentArity.ExactlyOne,
        HelpName = "number greater than 3",
    };

    private readonly Option<int?> blackBlanksOption = new(
        name: "--black-blanks"
    )
    {
        Description = BlackBlanksOptionDescription,
        DefaultValueFactory = _ => null,
        Arity = ArgumentArity.ExactlyOne,
        HelpName = "non-negative number",
    };

    private readonly Option<int?> blackNumbersOption = new(
        name: "--black-numbers"
    )
    {
        Description = BlackNumbersOptionDescription,
        DefaultValueFactory = _ => null,
        Arity = ArgumentArity.ExactlyOne,
        HelpName = "non-negative number",
    };

    private readonly Option<FileInfo?> templateOption = new(name: "--template")
    {
        Description = TemplateOptionDescription,
        DefaultValueFactory = _ => null,
        Arity = ArgumentArity.ExactlyOne,
        HelpName = "file",
    };

    private readonly Option<GridLayout> layoutOption = new(name: "--layout")
    {
        Description = LayoutOptionDescription,
        DefaultValueFactory = _ => DefaultLayout,
        Arity = ArgumentArity.ExactlyOne,
    };

    public EmptyGrid()
    {
        this.NonTemplateOptions =
        [
            this.sizeOption,
            this.blackBlanksOption,
            this.blackNumbersOption,
            this.layoutOption,
        ];

        this.Options = [.. this.NonTemplateOptions, this.templateOption];
    }

    public ImmutableArray<Option> Options { get; }

    private ImmutableArray<Option> NonTemplateOptions { get; }

    public void GetGridParameters(
        SymbolResult r,
        out GridParameters? gridParameters,
        out GridLayout layout,
        out FileInfo? template
    )
    {
        template = r.GetValue(this.templateOption);
        if (template != null)
        {
            layout = DefaultLayout;
            gridParameters = null;
        }
        else
        {
            layout = r.GetValue(this.layoutOption);

            gridParameters = (GridParameters)
                GridConfigurationBuilder.GetUnvalidatedGridParameters(
                    layout: layout,
                    size: r.GetValue(this.sizeOption),
                    blackBlanksRaw: r.GetValue(this.blackBlanksOption),
                    blackNumbersRaw: r.GetValue(this.blackNumbersOption)
                );
        }
    }

    public IEnumerable<string> GetValidationMessages(SymbolResult r)
    {
        return new Validator(this, r).GetErrors();
    }

    private void GetUnvalidatedGridParameters(
        SymbolResult r,
        out UnvalidatedGridConfiguration? gridParameters,
        out FileInfo? template
    )
    {
        T? GetValue<T>(Option<T> o)
        {
            var result = r.GetResult(o);
            return result == null
                ? (T?)o.GetDefaultValue()
                : result.GetValueOrDefault<T>();
        }

        template = GetValue(this.templateOption);
        gridParameters =
            template != null
                ? default
                : GridConfigurationBuilder.GetUnvalidatedGridParameters(
                    GetValue(this.layoutOption),
                    GetValue(this.sizeOption),
                    GetValue(this.blackBlanksOption),
                    GetValue(this.blackNumbersOption)
                );
    }

    private class Validator(EmptyGrid owner, SymbolResult r)
    {
        public IEnumerable<string> GetErrors()
        {
            owner.GetUnvalidatedGridParameters(
                r,
                out var gridParameters,
                out var template
            );

            if (template == null)
            {
                Debug.Assert(
                    gridParameters != null,
                    "Non-null when template is null."
                );
                return
                [
                    .. this.GetGridParameterMinMaxErrors(gridParameters.Value),
                    .. SymmetryValidator.GetSymmetryViolationErrors(
                        gridParameters.Value
                    ),
                ];
            }
            else
            {
                return this.GetTemplateOptionErrors();
            }
        }

        private IEnumerable<string> GetGridParameterMinMaxErrors(
            UnvalidatedGridConfiguration gridParameters
        )
        {
            int size = gridParameters.Size;

            int numberOfFields = size * size;
            int maxBlackBlanks =
                MaximumPercentageOfBlackBlanks * numberOfFields / 100;
            int maxBlackNumbers =
                MaximumPercentageOfBlackNumbers * numberOfFields / 100;
            string whenSize = $" for size {size}";

            return
            [
                .. owner.sizeOption.RequireMinMax(
                    size,
                    (MinimumSize, MaximumSize)
                ),
                .. owner.blackBlanksOption.RequireMinMax(
                    gridParameters.NumberOfBlackBlanks,
                    (0, maxBlackBlanks),
                    whenSize
                ),
                .. owner.blackNumbersOption.RequireMinMax(
                    gridParameters.NumberOfBlackNumbers,
                    (0, maxBlackNumbers),
                    whenSize
                ),
            ];
        }

        private IReadOnlyCollection<string> GetTemplateOptionErrors()
        {
            return
            [
                .. from o in owner.NonTemplateOptions
                where this.IsUserOption(o)
                select $"You cannot use the {o.Name} and {owner.templateOption.Name} options together.",
            ];
        }

        private bool IsUserOption(Option o)
        {
            return r.GetResult(o)?.Tokens.Count > 0;
        }
    }
}
