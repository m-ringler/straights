// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.DigitReader;

public class DigitFinderThreshold(int padding) : IDigitFinder
{
    public Mat? FindDigit(Mat gridCell)
    {
        using var binary = gridCell.Threshold(128, 255, ThresholdTypes.Binary);
        var binaryNormalized = binary;

        var contours = binaryNormalized.FindContoursAsArray(RetrievalModes.External, ContourApproximationModes.ApproxSimple);
        if (contours.Length == 0)
        {
            return null;
        }

        var rect = Cv2.BoundingRect(from c in contours from p in c select p);
        if (padding != 0)
        {
            rect.Inflate(padding, padding);
            rect = rect.Intersect(new Rect(0, 0, gridCell.Width, gridCell.Height));
        }

        return new Mat(gridCell, rect);
    }
}
