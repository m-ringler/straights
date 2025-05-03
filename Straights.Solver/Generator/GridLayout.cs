// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator;

public enum GridLayout
{
    // Renaming these is a breaking change!
    Random,
    Uniform,
    UniformIndependent,
    DiagonallySymmetric,
    HorizontallySymmetric,
    VerticallySymmetric,
    HorizontallyAndVerticallySymmetric,
    PointSymmetric,
}
