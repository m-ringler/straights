// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Data;

using System.Collections;
using System.Globalization;
using System.Numerics;

public sealed class WhiteFieldData
    : IEquatable<WhiteFieldData>,
        IReadOnlyCollection<int>
{
    public const int MaxSize = sizeof(ulong) * 8;

    private const ulong LeftmostBit = 1UL << 63;

    private ulong bitField;

    public WhiteFieldData(int size)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(size, 2);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(size, MaxSize);

        ulong bit = 1;
        for (int i = 0; i < size; i++)
        {
            this.bitField |= bit;
            bit <<= 1;
        }

        this.Size = size;
    }

    private WhiteFieldData(WhiteFieldData template)
    {
        this.bitField = template.bitField;
        this.Size = template.Size;
    }

    public int Size { get; }

    public int Count => BitOperations.PopCount(this.bitField);

    public int Min => this.First();

    public int Max
    {
        get
        {
            var shifted = this.bitField;
            shifted <<= 64 - this.Size;
            for (int result = this.Size; result != 0; result--)
            {
                if ((shifted & LeftmostBit) == LeftmostBit)
                {
                    return result;
                }

                shifted <<= 1;
            }

            throw new InvalidOperationException("Empty");
        }
    }

    public bool IsSolved => this.Count == 1;

    public static WhiteFieldData CreateSolved(int n, int size)
    {
        var result = new WhiteFieldData(size);
        result.Solve(n);
        return result;
    }

    public bool Contains(int n)
    {
        if (n < 0 || n > this.Size)
        {
            return false;
        }

        ulong bitmask = GetBitMask(n);
        bool result = (this.bitField & bitmask) == bitmask;
        return result;
    }

    public bool Remove(int n)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(n, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(n, this.Size);
        ulong bitmask = GetBitMask(n);
        bool result = (this.bitField & bitmask) == bitmask;
        this.bitField &= ~bitmask;

        if (this.Count == 0)
        {
            throw new NotSolvableException($"Last value {n} cannot be removed");
        }

        return result;
    }

    public void Remove(IEnumerable<int> numbers)
    {
        foreach (var n in numbers)
        {
            _ = this.Remove(n);
        }
    }

    public void Solve(int n)
    {
        this.bitField = GetBitMask(n);
    }

    public WhiteFieldData Clone()
    {
        return new WhiteFieldData(this);
    }

#if UNUSED
    public WhiteFieldData Intersect(WhiteFieldData other)
    {
        if (this.Size != other.Size)
        {
            throw new ArgumentException(
                $"You cannot intersect instances with different sizes: {this.Size} {other.Size}"
            );
        }

        var result = this.Clone();
        result.bitField &= other.bitField;
        return result;
    }
#endif

    public WhiteFieldData Union(WhiteFieldData other)
    {
        if (this.Size != other.Size)
        {
            throw new ArgumentException(
                $"You cannot union instances with different sizes: {this.Size} {other.Size}"
            );
        }

        var result = this.Clone();
        result.bitField |= other.bitField;
        return result;
    }

    public override string ToString()
    {
        return $"{this.bitField:B}/{this.Size}";
    }

    public bool Equals(WhiteFieldData? other)
    {
        return other != null
            && this.Size == other.Size
            && this.bitField == other.bitField;
    }

    public override bool Equals(object? obj)
    {
        return obj is WhiteFieldData other && this.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            typeof(WhiteFieldData),
            this.Size,
            this.bitField
        );
    }

    public string ToCompactString()
    {
        // We encode at most six numbers per Braille character
        int nchars = (this.Size + 5) / 6;
        if (this.Count == 1)
        {
            return this.Min.ToString(CultureInfo.InvariantCulture);
        }

        char[] result = new char[nchars];
        Array.Fill(result, ' ');
        int[] dots = [0x1, 0x2, 0x4, 0x8, 0x10, 0x20];

        const int BraillePatternBlank = 0x2800;
        int charValue = BraillePatternBlank;
        int ichar = 0;
        int k = 0;

        ulong shifted = this.bitField;
        for (int i = 1; i <= this.Size; i++)
        {
            if ((shifted & 1UL) == 1UL)
            {
                charValue += dots[k];
            }

            shifted >>= 1;

            k++;
            if (k == 6)
            {
                result[ichar] =
                    charValue == BraillePatternBlank ? ' ' : (char)charValue;
                charValue = BraillePatternBlank;
                k = 0;
                ichar++;
            }
        }

        if (charValue != BraillePatternBlank)
        {
            result[ichar] = (char)charValue;
        }

        return new string(result);
    }

    public IEnumerator<int> GetEnumerator()
    {
        ulong shifted = this.bitField;
        for (int i = 1; i <= this.Size; i++)
        {
            if ((shifted & 1UL) == 1UL)
            {
                yield return i;
            }

            shifted >>= 1;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    private static ulong GetBitMask(int n)
    {
        return 1UL << (n - 1);
    }
}
