// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image;

internal static class ImUtils
{
    public static void Close(this Mat edges, int kernelsize)
    {
        using var tmp = new Mat();
        using var kernel = Mat.Ones(
            (MatType)MatType.CV_8U,
            kernelsize,
            kernelsize
        );
        Cv2.Dilate(edges, tmp, kernel);
        Cv2.Erode(tmp, edges, kernel);
    }
}
