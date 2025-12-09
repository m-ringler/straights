// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

public static class TextSaverExtensions
{
    public static string GetString<T>(this ITextSaver<T> self, T value)
    {
        using var writer = new StringWriter();
        self.Save(value, writer);
        writer.Flush();
        return writer.ToString();
    }
}
