// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

internal static class SimilarLinesFilter
{
    // TODO: Use cutline as in GridFilter
    public static LineSegmentPolar[] FilterSimilarLines(
        LineSegmentPolar[] lines,
        float rho_threshold,
        float theta_threshold
    )
    {
        Dictionary<int, int> numberOfSimilarLines = Enumerable
            .Range(0, lines.Length)
            .ToDictionary(i => i, _ => 0);

        int remainingCount = lines.Length;
        for (int i = 0; i < lines.Length - 1; i++)
        {
            var higherIndices = Enumerable.Range(i + 1, --remainingCount);
            foreach (int j in GetSimilarLines(lines, i, higherIndices))
            {
                numberOfSimilarLines[i]++;
                numberOfSimilarLines[j]++;
            }
        }

        bool[] keepLines = new bool[lines.Length];
        Array.Fill(keepLines, true);

        int[] lineIndicesOrdered =
        [
            .. from kvp in numberOfSimilarLines
            orderby kvp.Value descending
            select kvp.Key,
        ];

        remainingCount = lineIndicesOrdered.Length;
        for (int i = 0; i < lineIndicesOrdered.Length - 1; i++)
        {
            --remainingCount;
            var index = lineIndicesOrdered[i];
            if (!keepLines[index])
            {
                continue;
            }

            var candidateIndices =
                from j in Enumerable.Range(i + 1, remainingCount)
                select lineIndicesOrdered[j];
            foreach (int j in GetSimilarLines(lines, index, candidateIndices))
            {
                keepLines[j] = false;
            }
        }

        LineSegmentPolar[] filteredLines =
        [
            .. from i in Enumerable.Range(0, lines.Length)
            where keepLines[i]
            select lines[i],
        ];
        return filteredLines;

        // Helper function
        IEnumerable<int> GetSimilarLines(
            LineSegmentPolar[] lines,
            int i,
            IEnumerable<int> candidateIndices
        )
        {
            var a = Deconstruct(lines[i]);
            foreach (var item in candidateIndices)
            {
                var otherLine = lines[item];
                var b = Deconstruct(otherLine);
                if (IsSimilar(a, b))
                {
                    yield return item;
                }
            }
        }

        bool IsSimilar((float Rho, float Theta) a, (float Rho, float Theta) b)
        {
            return Math.Abs(a.Rho - b.Rho) < rho_threshold
                && Math.Abs(a.Theta - b.Theta) < theta_threshold;
        }
    }

    private static (float Rho, float Theta) Deconstruct(LineSegmentPolar line)
    {
        return (line.Rho, line.Theta);
    }
}
