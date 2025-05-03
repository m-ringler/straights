// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

public sealed class IterativeSimplifier<TTarget, TSnapshottable>(
        Func<IChangeDetector<TSnapshottable>> newSnapshotFunc,
        ChangeDetectorProxy<TSnapshottable> changeDetector,
        ISimplify<TTarget> coreSimplifier,
        Action<int> onBeginIteration,
        Action<int> onEndIteration)
    : ISimplify<TTarget>
    where TTarget : TSnapshottable
{
    public void Simplify(TTarget data)
    {
        // Initialize the previous state variable from the
        // passed-in change detector proxy, so that the
        // assignment of changeDetector.Core is a NOOP in
        // the first iteration.
        var previousState = changeDetector.Core;
        int i = 1;
        do
        {
            onBeginIteration(i);

            // Supply the passed-in change detector proxy with the
            // state prior to the last iteration. This information
            // can be used by the coreSimplifier to skip sections of
            // the data that have not changed.
            changeDetector.Core = previousState;
            previousState = newSnapshotFunc();
            coreSimplifier.Simplify(data);
            onEndIteration(i);
            i++;
        }
        while (previousState.HasChanged(data));
    }
}