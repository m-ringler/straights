// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.DigitReader;

public class DigitReader1to9(IDigitFinder finder, IDigitClassifier classifier) : IDigitReader
{
    public int? TryReadDigit(Mat img)
    {
        using var digitMat = finder.FindDigit(img);
        if (digitMat == null)
        {
            return null;
        }

        var result = classifier.PredictImage(digitMat);
        return result == 0 ? null : result;
    }
}
