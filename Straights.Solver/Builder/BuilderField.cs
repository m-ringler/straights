// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Builder;

/// <summary>
/// A field of the <see cref="GridBuilder"/>.
/// </summary>
/// <param name="Location">The location on the field.</param>
/// <param name="Value">The value of the field, or <c>null</c> if it is blank.</param>
public sealed partial record BuilderField(FieldLocation Location, int? Value)
{
    public BuilderField(FieldLocation location)
    : this(location, null)
    {
    }

    public bool IsWhite { get; init; }

    public void Validate(int size)
    {
        this.Location.Validate(size);
        this.ValidateValue(size);
    }

    private void ValidateValue(int size)
    {
        if (this.Value == null)
        {
            return;
        }

        var number = this.Value.Value;
        if (number < 1)
        {
            throw new ValidationException("Value must not be less than 1 " + this);
        }

        if (number > size)
        {
            throw new ValidationException($"Value must not be greater than {size} " + this);
        }
    }
}
