// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

public sealed class ThreeMeansCellClassifier : ICellClassifier
{
    public CellType GetCellType(Mat cellMat)
    {
        var means = GetThreeMeans(cellMat);
        int outerCellCount = means.Length - 1;
        var outerStats = MinMaxMean(means.Take(outerCellCount));
        bool isBlack = outerStats.Mean < 128;
        bool isDigit;
        if (isBlack)
        {
            double cmp = (2 * outerStats.Max) - outerStats.Min;
            isDigit = means[^1] > Math.Max(cmp, 10);
        }
        else
        {
            double cmp = (2 * outerStats.Min) - outerStats.Max;
            isDigit = means[^1] < Math.Min(cmp, 245);
        }

        return (isDigit, isBlack) switch
        {
            (true, true) => CellType.BlackNumber,
            (true, false) => CellType.WhiteNumber,
            (false, true) => CellType.BlackBlank,
            (false, false) => CellType.WhiteBlank,
        };
    }

    private static (double Min, double Max, double Mean) MinMaxMean(
        IEnumerable<double> values
    )
    {
        double sum = 0;
        int count = 0;
        double min = double.PositiveInfinity;
        double max = double.NegativeInfinity;
        foreach (var value in values)
        {
            if (value < min)
            {
                min = value;
            }

            if (value > max)
            {
                max = value;
            }

            sum += value;
            count++;
        }

        return (min, max, sum / count);
    }

    /// <summary>
    /// Gets the means of the cells resulting from subdividing the image
    /// into 3 horizontal cells. The center cell is returned last.
    /// </summary>
    /// <param name="img">The image.</param>
    /// <returns>The means of the 3 cells, with the mean of the center cell last.</returns>
    private static double[] GetThreeMeans(Mat img)
    {
        const int margin = 2;
        var s = img.Size();
        var w = s.Width / 5;
        var h = s.Height / 3;
        int[] xx = [margin, s.Width - w - margin, w];
        int[] ww = [w, w, 2 * w];
        double[] result = new double[3];
        int i = 0;
        for (int ix = 0; ix < 3; ix++)
        {
            using Mat subcellMat = new(img, new Rect(xx[ix], h, ww[ix], h));
            result[i++] = subcellMat.Mean().Val0;
        }

        return result;
    }
}
