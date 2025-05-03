// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

using System.Numerics;
using Straights.Solver.Data;

internal static class BinaryGameSerializer
{
    public const byte EncodingVersion = 0b10000000;

    public static void ToBinary(
        in Game game,
        IBitWriter writer)
    {
        var (solved, unsolved) = game;
        byte size = (byte)solved.Size;
        CheckSize(size);

        writer.WriteNumber(EncodingVersion, 8);
        writer.WriteNumber(size, 5);
        int bitsPerNumber = BitsPerNumber(size);
        foreach (var index in solved.AllFieldIndices())
        {
            var field = solved.GetField(index);

            var unsolvedField = unsolved.GetField(index);

            bool black = field is not SolverField.WhiteField;
            bool isKnown = field is SolverField.BlackNumber ||
                (field is SolverField.WhiteField && unsolvedField is SolverField.WhiteField wf && wf.Data.IsSolved);
            var value = field.GetWhiteFieldData();
            uint number = value switch
            {
                null => uint.MaxValue,
                WhiteFieldData x when x.IsSolved => (uint)(x.Min - 1),
                _ => throw new ArgumentException(
                    paramName: nameof(game),
                    message: $"There must not be any unsolved field in {nameof(game.Solved)}."),
            };

            writer.WriteBit(black);
            writer.WriteBit(isKnown);
            writer.WriteNumber(number, bitsPerNumber);
        }
    }

    public static Game FromBinary(IBitReader reader)
    {
        uint version = reader.ReadNumber(8);
        if (version != EncodingVersion)
        {
            throw new InvalidDataException(
                $"Unsupported encoding version: {version}");
        }

        uint size = reader.ReadNumber(5);
        if (size is < 1 or > 31)
        {
            throw new InvalidDataException(
                $"Invalid grid size: {size}");
        }

        int numFields = (int)(size * size);

        bool[] black = new bool[numFields];
        bool[] known = new bool[numFields];
        int[] numbers = new int[numFields];

        var bitsPerNumber = BitsPerNumber(size);
        for (int i = 0; i < numFields; i++)
        {
            black[i] = reader.ReadBit();
            known[i] = reader.ReadBit();
            numbers[i] = (int)reader.ReadNumber(bitsPerNumber) + 1;
        }

        // Construct grid
        return ConstructGrid(size, black, known, numbers);
    }

    private static int BitsPerNumber(uint size)
    {
        uint maxValue = size - 1;
        var bitsPerNumber = BitOperations.Log2(maxValue) + 1;
        return bitsPerNumber;
    }

    private static void CheckSize(int size)
    {
        if (size is < 1 or > 31)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size),
                "Grid size must be between 1 and 31.");
        }
    }

    private static Game ConstructGrid(
        uint size,
        bool[] black,
        bool[] known,
        int[] numbers)
    {
        var solved = ImmutableArray.CreateBuilder<SolverField>();
        var unsolved = ImmutableArray.CreateBuilder<SolverField>();
        int sz = (int)size;

        for (int i = 0; i < black.Length; i++)
        {
            var data = (black[i], known[i]);
            SolverField unsolvedField = data switch
            {
                (true, false) => new SolverField.BlackBlank(),
                (true, true) => new SolverField.BlackNumber(numbers[i], sz),
                _ => new SolverField.WhiteField(new WhiteFieldData(sz)),
            };

            if (known[i])
            {
                unsolvedField.GetWhiteFieldData()?.Solve(numbers[i]);
            }

            SolverField solvedField = unsolvedField.Clone();
            solvedField.GetWhiteFieldData()?.Solve(numbers[i]);

            unsolved.Add(unsolvedField);
            solved.Add(solvedField);
        }

        return new(
            new(solved.DrainToImmutable()),
            new(unsolved.DrainToImmutable()));
    }
}
