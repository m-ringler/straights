// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using Straights.Solver.Data;

using ChangeDetectorProxy = ChangeDetectorProxy<IGetSnapshot<int>>;
using IChangeDetector = IChangeDetector<IGetSnapshot<int>>;

/// <summary>
/// A factory that builds the default iterative grid simplifier.
/// </summary>
/// <param name="options">The options to use.</param>
public sealed class GridSimplifierFactory(SimplifierOptions options)
{
    public static ISimplify<SolverGrid> BuildIterativeSimplifier(SimplifierStrength strength)
    {
        return new GridSimplifierFactory(new())
            .BuildIterativeSimplifier(strength, _ => { }, _ => { });
    }

    public ISimplify<ISolverGrid> BuildIterativeSimplifier(
        SimplifierStrength strength,
        Action<int> onBeginIteration,
        Action<int> onEndIteration)
    {
        void Simplify(ISolverGrid data)
        {
            var newSnapshotFunc = GetNewSnapshotFunc(data);
            var changeDetector = new ChangeDetectorProxy(
                defaultValue: true);

            var coreSimplifier = this.BuildGridSimplifier(changeDetector, strength);
            ISimplify<ISolverGrid> gridSimplifier = coreSimplifier;

            var iterativeGridSimplifier = new IterativeSimplifier<ISolverGrid, IGetSnapshot<int>>(
                newSnapshotFunc,
                changeDetector,
                gridSimplifier,
                onBeginIteration,
                onEndIteration);
            iterativeGridSimplifier.Simplify(data);
        }

        return Simplifier.Create<ISolverGrid>(Simplify).WithName(
            $"Strength-{strength.Value} Iterative Grid Simplifier");
    }

    private static Func<IChangeDetector>
        GetNewSnapshotFunc(ISolverGrid data)
    {
        IEnumerable<IGetSnapshot<int>> trackables = [
                data,
                ..data.Columns,
                ..data.Rows,
                ..data.Rows.SelectMany(x => x.Blocks)];

        IChangeDetector<IGetSnapshot<int>> NewSnapshot()
        {
            return new StateComparer<int>(trackables);
        }

        return NewSnapshot;
    }

    private static IEnumerable<ISimplify<SolverColumn>> GetColumnSimplifiers(
        SimplifierStrength strength,
        ISimplify<SolverBlock> blockSimplifier)
    {
        yield return new ColumnRemoveSolvedNumbers();
        if (strength >= 1)
        {
            yield return new ColumnRemoveForeignRanges();
        }

        if (strength >= 3)
        {
            yield return new ColumnNFieldsWithNNumbers();
        }

        yield return blockSimplifier.AsAggregateSimplifier();

        if (strength >= 2)
        {
            yield return new ColumnConsistentRanges();
        }
    }

    private static IEnumerable<ISimplify<SolverBlock>> GetBlockSimplifiers(
        SimplifierStrength strength)
    {
        yield return new BlockRestrictRange();

        if (strength >= 1)
        {
            yield return new BlockNoNeighbors();
        }

        if (strength >= 2)
        {
            yield return new BlockTwoValuesFarApart();
            yield return new BlockDisjunctSubsets();
        }

        if (strength >= 3)
        {
            yield return new BlockNFieldsWithNValuesInCertainRange();
        }
    }

    private ISimplify<ISolverGrid> BuildGridSimplifier(
        IChangeDetector changeDetector,
        SimplifierStrength strength)
    {
        var blockSimplifier = Simplifier
        .Combine(
            GetBlockSimplifiers(strength))
        .WithShortcut(
            changeDetector);

        var columnSimplifier = Simplifier
        .Combine(
            GetColumnSimplifiers(strength, blockSimplifier))
        .WithShortcut(changeDetector);

        ISimplify<ISolverGrid> result = options.MultiThreaded
            ? new ParallelGridSimplifier(columnSimplifier)
            : new GridSimplifier(columnSimplifier);

        return result.WithShortcut(changeDetector);
    }
}
