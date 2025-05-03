// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Data;

using FluentAssertions.Equivalency;

using Straights.Solver;
using Straights.Solver.Data;

public class SolverGridTests
{
    [Fact]
    public void Clone_WhenInvoked_CreatesExactCopy()
    {
        string gridText =
"""
3
_,_,w3
b3,b2,_
b2,_,b

""";
        SolverGrid sut = CreateSut(gridText);

        // ACT
        var clone = sut.CreateCopy();

        // ASSERT
        _ = clone.Should().BeEquivalentTo(
            sut,
            TreatSolverColumnAsSeveralBlocks);
    }

    [Fact]
    public void Clone_WhenOriginalIsModified_CloneRemainsUnchanged()
    {
        string gridText =
"""
3
_,_,w3
b3,b2,_
b2,_,b

""";
        SolverGrid sut = CreateSut(gridText);

        // ACT
        var clone = sut.CreateCopy();
        _ = sut.Rows[0].Blocks[0].Fields[1].Remove(2);

        // ASSERT
        _ = clone.Rows[0].Blocks[0].Fields[1].Should().Contain(2);
    }

    private static SolverGrid CreateSut(string builderText)
    {
        return GridConverter.ParseBuilderText(builderText).SolverGrid;
    }

    private static EquivalencyOptions<SolverGrid>
        TreatSolverColumnAsSeveralBlocks(EquivalencyOptions<SolverGrid> cfg)
    {
        return cfg
            .Using<SolverColumn>(
                ctx =>
                {
                    IEnumerable<SolverBlock> subject = ctx.Subject;
                    IEnumerable<SolverBlock> expectation = ctx.Expectation;
                    _ = subject.Should().BeEquivalentTo(expectation);
                })
            .WhenTypeIs<SolverColumn>();
    }
}
