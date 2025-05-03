// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Builder;

using Straights.Solver.Builder;

/// <summary>
/// Tests for <see cref="BuilderField"/>.
/// </summary>
public class BuilderFieldTests
{
    [Fact]
    public void Validate_WhenValueIsNull_DoesNotThrow()
    {
        // ARRANGE
        var field = new BuilderField(new FieldLocation(1, 1), null);

        // ACT
        Action act = () => field.Validate(5);

        // ASSERT
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WhenValueIsLessThanOne_ThrowsValidationException()
    {
        // ARRANGE
        var field = new BuilderField(new FieldLocation(1, 1), Value: 0);

        // ACT
        Action act = () => field.Validate(size: 5);

        // ASSERT
        act.Should().Throw<ValidationException>()
            .WithMessage("Value must not be less than 1 *");
    }

    [Fact]
    public void Validate_WhenValueIsGreaterThanSize_ThrowsValidationException()
    {
        // ARRANGE
        var field = new BuilderField(new FieldLocation(1, 1), 6);

        // ACT
        Action act = () => field.Validate(5);

        // ASSERT
        act.Should().Throw<ValidationException>()
            .WithMessage("Value must not be greater than 5 *");
    }

    [Fact]
    public void Validate_WhenValueIsWithinRange_DoesNotThrow()
    {
        // ARRANGE
        var field = new BuilderField(new FieldLocation(1, 1), 3);

        // ACT
        Action act = () => field.Validate(5);

        // ASSERT
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WhenLocationIsInvalid_ThrowsValidationException()
    {
        // ARRANGE
        var field = new BuilderField(new FieldLocation(6, 1), 3);

        // ACT
        Action act = () => field.Validate(5);

        // ASSERT
        act.Should().Throw<ValidationException>();
    }
}