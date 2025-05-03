// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Builder;

using Straights.Solver.Converter;

/// <summary>
/// Builds a square grid of black and white fields,
/// both of which are either blank or have a single value
/// in the rage 1 to <see cref="Size"/>.
/// </summary>
public sealed class GridBuilder
{
    private readonly BuilderField?[][] fields;

    public GridBuilder(int size)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 1);

        this.Size = size;
        BuilderField?[][] fields = AllocateFields(size);

        this.fields = fields;
    }

    public int Size { get; }

    public void Clear()
    {
        for (int i = 0; i < this.Size; i++)
        {
            Array.Clear(this.fields[i]);
        }
    }

    public void SetBlack(int y, int x)
    {
        this.Add(new(new(x, y), null));
    }

    public void SetBlack(int y, int x, int value)
    {
        this.Add(new(new(x, y), value));
    }

    public void SetWhite(int y, int x, int value)
    {
        this.Add(new(new(x, y), value) { IsWhite = true });
    }

    public void Clear(int y, int x)
    {
        this.Clear(new(x, y));
    }

    public void Clear(FieldLocation location)
    {
        location.Validate(this.Size);
        int ix = location.X - 1;
        int iy = location.Y - 1;
        this.fields[iy][ix] = null;
    }

    public void Add(BuilderField value)
    {
        value.Validate(this.Size);
        var loc = value.Location;
        int ix = loc.X - 1;
        int iy = loc.Y - 1;
        if (value.Value != null)
        {
            CheckAdd(this.fields[iy], value);
            CheckAdd(
                this.GetColumn(ix),
                value);
        }

        this.fields[iy][ix] = value;
    }

    public BuilderField?[][] GetFields()
    {
        return [.. this.fields.Select(x => (BuilderField?[])x.Clone())];
    }

    public IEnumerable<BuilderField?> EnumerateFields()
    {
        return from row in this.fields
               from field in row
               select field;
    }

    public override string ToString()
    {
        ITextPersister<GridBuilder> p = new GridBuilderTextPersister();
        return p.GetString(this);
    }

    internal static BuilderField?[][] AllocateFields(int size)
    {
        var fields = new BuilderField?[size][];
        for (int i = 0; i < size; i++)
        {
            fields[i] = new BuilderField?[size];
        }

        return fields;
    }

    private static void CheckAdd(
        IEnumerable<BuilderField?> target,
        BuilderField value)
    {
        if (value.Value == null)
        {
            return;
        }

        foreach (var item in target)
        {
            if (item != null && item != value && item.Value == value.Value)
            {
                throw new ValidationException($"Conflict: {item} : {value}");
            }
        }
    }

    private IEnumerable<BuilderField> GetColumn(int ix)
    {
        return from i in Enumerable.Range(0, this.Size)
               select this.fields[i][ix];
    }
}
