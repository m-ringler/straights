// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Simplification;

using System.Runtime.ExceptionServices;

/// <summary>
/// Provides methods to create, decorate, and invoke
/// instances of <see cref="ISimplify{T}"/> conveniently.
/// </summary>
public static class Simplifier
{
    public static ISimplify<T> WithShortcut<T>(
        this ISimplify<T> simplifier,
        IChangeDetector<T> changeDetector)
    {
        return new ShortcutSimplifier<T>(simplifier, changeDetector);
    }

    public static void SimplifyMany<T>(
        this ISimplify<T> simplifier,
        IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            simplifier.Simplify(item);
        }
    }

    public static void SimplifyManyParallel<T>(
        this ISimplify<T> simplifier,
        IEnumerable<T> items)
    {
        try
        {
            _ = Parallel.ForEach(items, simplifier.Simplify);
        }
        catch (AggregateException ex)
        {
            var coreExs =
                from x in ex.Flatten().InnerExceptions
                where x is not OperationCanceledException
                select x;
            var core = coreExs.FirstOrDefault();
            {
                if (core == null)
                {
                    throw;
                }

                ExceptionDispatchInfo.Capture(core).Throw();
            }
        }
    }

    public static ISimplify<T> Combine<T>(
        IEnumerable<ISimplify<T>> simplifiers)
    {
        return new CompositeSimplifier<T>(simplifiers);
    }

    public static ISimplify<IEnumerable<T>> AsAggregateSimplifier<T>(
        this ISimplify<T> simplifier)
    {
        return new AggregateSimplifier<T>(simplifier);
    }

    public static ISimplify<T> Create<T>(Action<T> simplify)
    {
        return new DelegateSimplifier<T>(simplify);
    }

    public static ISimplify<T> WithName<T>(
        this ISimplify<T> simplifier,
        string name)
    {
        return new NamedSimplifier<T>(simplifier, name);
    }
}
