// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator;

public record class GridParameters
{
    public const int MinimumSize = 4;
    public const int MaximumSize = 24;

    public const int MaximumPercentageOfBlackBlanks = 25;

    public const int MaximumPercentageOfBlackNumbers = 10;

    public GridParameters(
        int size,
        int numberOfBlackBlanks,
        int numberOfBlackNumbers)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 4);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(size, 24);
        this.Size = size;

        int numberOfFields = size * size;

        ArgumentOutOfRangeException.ThrowIfLessThan(numberOfBlackBlanks, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            numberOfBlackBlanks,
            numberOfFields * MaximumPercentageOfBlackBlanks / 100);
        this.NumberOfBlackBlanks = numberOfBlackBlanks;

        ArgumentOutOfRangeException.ThrowIfLessThan(numberOfBlackNumbers, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(
            numberOfBlackNumbers,
            numberOfFields * MaximumPercentageOfBlackNumbers / 100);
        this.NumberOfBlackNumbers = numberOfBlackNumbers;
    }

    public static GridParameters DefaultParameters { get; } = new GridParameters(9, 13, 5);

    public int Size { get; }

    public int NumberOfBlackBlanks { get; }

    public int NumberOfBlackNumbers { get; }

    public int TotalNumberOfBlackFields => this.NumberOfBlackBlanks + this.NumberOfBlackNumbers;
}