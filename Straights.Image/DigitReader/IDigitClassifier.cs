// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.DigitReader;

public interface IDigitClassifier
{
    int? PredictImage(Mat grayImage);
}
