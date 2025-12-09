// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

using System.Collections;
using Straights.Solver.Simplification;

public class SolverBlock
    : IGetSnapshot<int>,
        IReadOnlyCollection<WhiteFieldData>
{
    public required ImmutableArray<WhiteFieldData> Fields { get; init; }

    public int Count => this.Fields.Length;

    public IEnumerator<WhiteFieldData> GetEnumerator()
    {
        return ((IEnumerable<WhiteFieldData>)this.Fields).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    int IGetSnapshot<int>.GetSnapshot()
    {
        return this.TotalCount();
    }
}
