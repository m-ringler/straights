// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

public record Cell
{
    private Cell() { }

    public interface IHasNumber
    {
        int Number { get; }
    }

    public sealed record WhiteNumber(int Number) : Cell(), IHasNumber;

    public sealed record BlackNumber(int Number) : Cell(), IHasNumber;

    public sealed record BlackBlank() : Cell();

    public sealed record WhiteBlank() : Cell();
}
