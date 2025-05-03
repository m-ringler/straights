// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Generator;

using Straights.Solver.Builder;

public interface IGridGenerator
{
    GridBuilder? GenerateGrid();
}
