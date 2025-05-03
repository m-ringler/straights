// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

public interface ITextSaver<T>
{
    void Save(T value, TextWriter writer);
}
