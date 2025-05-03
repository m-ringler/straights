// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

/// <summary>
/// A square grid of fields based on a
/// one-dimensional immutable array.
/// </summary>
/// <typeparam name="TField">
/// The type of the fields.
/// </typeparam>
public class Grid<TField>
{
    public Grid(ImmutableArray<TField> fields)
    {
        int n2 = fields.Length;
        int n = (int)Math.Sqrt(n2);
        if (n2 != n * n)
        {
            throw new ArgumentException("The number of fields must be a square number.");
        }

        this.Size = n;
        this.Fields = fields;
    }

    public ImmutableArray<TField> Fields { get; }

    public int Size { get; }

    /// <summary>
    /// Gets the field with the specified zero-based column and row indices.
    /// </summary>
    /// <param name="ix">The zero-based column index.</param>
    /// <param name="iy">The zero-based row index.</param>
    /// <returns>The field at the specified index.</returns>
    public TField GetField(int ix, int iy)
    {
        int index = (iy * this.Size) + ix;
        return this.Fields[index];
    }

    public TField GetField(FieldIndex index)
    {
        return this.GetField(index.X, index.Y);
    }

    public IEnumerable<FieldIndex> AllFieldIndices()
    {
        for (int iy = 0; iy < this.Size; iy++)
        {
            for (int ix = 0; ix < this.Size; ix++)
            {
                yield return new(ix, iy);
            }
        }
    }

    public IEnumerable<IEnumerable<TField>> GetColumns()
    {
        var range = Enumerable.Range(0, this.Size);
        return from ix in range
               select from iy in range
                      select this.GetField(ix, iy);
    }

    public IEnumerable<IEnumerable<TField>> GetRows()
    {
        var range = Enumerable.Range(0, this.Size);
        return from iy in range
               select from ix in range
                      select this.GetField(ix, iy);
    }
}
