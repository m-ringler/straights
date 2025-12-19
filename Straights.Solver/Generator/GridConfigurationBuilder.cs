// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator;

using static Straights.Solver.Generator.GridParameters;

public static class GridConfigurationBuilder
{
    public static UnvalidatedGridConfiguration GetUnvalidatedGridParameters(
        GridLayout layout,
        int size,
        int? blackBlanksRaw,
        int? blackNumbersRaw
    )
    {
        var divisor = SymmetryValidator.GetDivisor(layout, size);

        int blackBlanks;
        int blackNumbers;
        if (divisor == 1 || (blackBlanksRaw != null && blackNumbersRaw != null))
        {
            blackBlanks = blackBlanksRaw ?? GetDefaultBlackBlanks(size);
            blackNumbers = blackNumbersRaw ?? GetDefaultBlackNumbers(size);
        }
        else if (blackBlanksRaw != null)
        {
            blackBlanks = blackBlanksRaw.Value;
            blackNumbers = GetDefaultBlackNumbers(size);
            Adjust(blackBlanks, divisor, ref blackNumbers);
        }
        else
        {
            blackNumbers = blackNumbersRaw ?? GetDefaultBlackNumbers(size);
            blackBlanks = GetDefaultBlackBlanks(size);
            Adjust(blackNumbers, divisor, ref blackBlanks);
        }

        return new(size, blackBlanks, blackNumbers, layout);
    }

    private static int GetDefaultBlackBlanks(int size)
    {
        return ScaleDefaultNumberOfFields(
            DefaultParameters.NumberOfBlackBlanks,
            size
        );
    }

    private static int GetDefaultBlackNumbers(int size)
    {
        return ScaleDefaultNumberOfFields(
            DefaultParameters.NumberOfBlackNumbers,
            size
        );
    }

    private static int ScaleDefaultNumberOfFields(int n, int size)
    {
        var numberOfFields = size * size;
        var defaultNumberOfFields =
            DefaultParameters.Size * DefaultParameters.Size;

        return n * numberOfFields / defaultNumberOfFields;
    }

    /// <summary>
    /// Adjusts variableCount so that
    /// <c><paramref name="fixedCount"/> + <paramref name="variableCount"/></c>
    /// is a multiple of <paramref name="divisor"/>.
    /// </summary>
    /// <param name="fixedCount">The fixed count.</param>
    /// <param name="divisor">The positive divisor.</param>
    /// <param name="variableCount">The variable count to adjust.</param>
    private static void Adjust(
        int fixedCount,
        int divisor,
        ref int variableCount
    )
    {
        int totalCountRaw = fixedCount + variableCount;
        int multiplier = (int)Math.Round(totalCountRaw * 1.0 / divisor);
        int totalCount = divisor * multiplier;
        variableCount += totalCount - totalCountRaw;
        if (variableCount <= 0)
        {
            variableCount += divisor;
        }
    }
}
