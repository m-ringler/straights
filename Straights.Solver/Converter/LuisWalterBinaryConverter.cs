// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

using Straights.Solver.Data;

internal static class LuisWalterBinaryConverter
{
    public const byte EncodingVersion = 0b00000010;

    public static void ToBinary(Game game, IBitWriter writer)
    {
        var (solved, unsolved) = game;
        writer.WriteNumber(EncodingVersion, 8);
        if (solved.Size != 9)
        {
            throw new ArgumentException(
                paramName: nameof(game),
                message: $"The size of the grid must be 9."
            );
        }

        foreach (var index in solved.AllFieldIndices())
        {
            var field = solved.GetField(index);

            var unsolvedField = unsolved.GetField(index);

            bool black = field is not SolverField.WhiteField;

            bool isKnown =
                field is SolverField.BlackNumber
                || (
                    field is SolverField.WhiteField
                    && unsolvedField is SolverField.WhiteField wf
                    && wf.Data.IsSolved
                );
            writer.WriteBit(black);
            writer.WriteBit(isKnown);
            var value = field?.GetWhiteFieldData();
            if (value == null)
            {
                writer.WriteNumber(uint.MaxValue, 4);
            }
            else
            {
                if (!value.IsSolved)
                {
                    throw new ArgumentException(
                        paramName: nameof(game),
                        message: $"There must not be any unsolved field in {nameof(game.Solved)}."
                    );
                }

                writer.WriteNumber((uint)(value.Min - 1), 4);
            }
        }
    }
}
