// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

/// <summary>
/// An object that can save and load instances of <typeparamref name="T"/>
/// to and from plain text.
/// </summary>
/// <typeparam name="T">
/// The type of the serialized instances.
/// </typeparam>
public interface ITextPersister<T> : ITextSaver<T>, ITextLoader<T> { }
