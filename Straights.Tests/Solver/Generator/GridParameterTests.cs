// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Generator;

using Straights.Solver.Generator;

/// <summary>
/// Tests for <see cref="GridParameters"/>.
/// </summary>
public class GridParameterTests
{
    [Fact]
    public void DefaultParameters_HaveCorrectValues()
    {
        var sut = GridParameters.DefaultParameters;

        sut.Size.Should().Be(9);
        sut.NumberOfBlackBlanks.Should().Be(13);
        sut.NumberOfBlackNumbers.Should().Be(5);
        sut.TotalNumberOfBlackFields.Should().Be(18);
    }
}
