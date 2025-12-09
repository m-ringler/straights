// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

internal static class AngleFilter
{
    public static (
        List<LineSegmentPolar> Horizontal,
        List<LineSegmentPolar> Vertical
    ) FilterAngles(IEnumerable<LineSegmentPolar> lines, float thresholdRad)
    {
        var result = ClusterAngles(lines);
        Filter(result, thresholdRad);

        return result;
    }

    private static void Filter(
        (
            List<LineSegmentPolar> Horizontal,
            List<LineSegmentPolar> Vertical
        ) result,
        float thresholdRad
    )
    {
        var (linesH, linesV) = result;
        var medianH = linesH[linesH.Count / 2].Theta;
        var medianV = linesV[linesH.Count / 2].Theta;
        Remove(linesH, l => Math.Abs(l.Theta - medianH) > thresholdRad);
        Remove(linesV, l => Math.Abs(l.Theta - medianV) > thresholdRad);
    }

    private static (
        List<LineSegmentPolar> Horizontal,
        List<LineSegmentPolar> Vertical
    ) ClusterAngles(IEnumerable<LineSegmentPolar> lines)
    {
        const float V = 0;
        const float H = (float)(Math.PI / 2);

        List<LineSegmentPolar> linesH = [];
        List<LineSegmentPolar> linesV = [];
        foreach (var line in lines)
        {
            var theta = line.Theta;
            float deltaH = Math.Abs(theta - H);
            float deltaV = Math.Abs(theta - V);
            var list = deltaH < deltaV ? linesH : linesV;
            list.Add(line);
        }

        SortBy(linesH, l => l.Theta);
        SortBy(linesV, l => l.Theta);
        return (linesH, linesV);
    }

    private static void Remove<T>(IList<T> items, Func<T, bool> predicate)
    {
        for (int k = items.Count - 1; k >= 0; k--)
        {
            if (predicate(items[k]))
            {
                items.RemoveAt(k);
            }
        }
    }

    private static void SortBy<T, TK>(List<T> items, Func<T, TK> getSortKey)
        where TK : IComparable<TK>
    {
        items.Sort((x, y) => getSortKey(x).CompareTo(getSortKey(y)));
    }
}
