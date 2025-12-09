// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Builder;

using Straights.Solver.Builder;

/// <summary>
/// Tests for <see cref="FieldLocation"/>.
/// </summary>
public class FieldLocationTests
{
    [Fact]
    public void Validate_WhenXIsLessThanOne_ThrowsValidationException()
    {
        // ARRANGE
        var location = new FieldLocation(0, 1);

        // ACT
        Action act = () => location.Validate(5);

        // ASSERT
        act.Should()
            .Throw<ValidationException>()
            .WithMessage("X must not be less than one " + location);
    }

    [Fact]
    public void Validate_WhenYIsLessThanOne_ThrowsValidationException()
    {
        // ARRANGE
        var location = new FieldLocation(1, 0);

        // ACT
        Action act = () => location.Validate(5);

        // ASSERT
        act.Should()
            .Throw<ValidationException>()
            .WithMessage("Y must not be less than one " + location);
    }

    [Fact]
    public void Validate_WhenXIsGreaterThanSize_ThrowsValidationException()
    {
        // ARRANGE
        var location = new FieldLocation(6, 1);

        // ACT
        Action act = () => location.Validate(5);

        // ASSERT
        act.Should()
            .Throw<ValidationException>()
            .WithMessage("X must not be greater than 5 " + location);
    }

    [Fact]
    public void Validate_WhenYIsGreaterThanSize_ThrowsValidationException()
    {
        // ARRANGE
        var location = new FieldLocation(1, 6);

        // ACT
        Action act = () => location.Validate(5);

        // ASSERT
        act.Should()
            .Throw<ValidationException>()
            .WithMessage("Y must not be greater than 5 " + location);
    }

    [Fact]
    public void Validate_WhenXAndYAreWithinRange_DoesNotThrow()
    {
        // ARRANGE
        var location = new FieldLocation(3, 3);

        // ACT
        Action act = () => location.Validate(5);

        // ASSERT
        act.Should().NotThrow();
    }
}
