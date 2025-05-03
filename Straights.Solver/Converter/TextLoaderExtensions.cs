// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

public static class TextLoaderExtensions
{
    public static T Parse<T>(this ITextLoader<T> self, string text)
    {
        using var reader = new StringReader(text);
        return self.Load(reader);
    }
}
