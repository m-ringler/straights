// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

public interface IBlackAndWhiteGridReader
{
    Cell[][] ReadGrid(Mat img);

    Cell[][] ReadGrid(string imagePath)
    {
        using var img = Cv2.ImRead(imagePath, ImreadModes.Grayscale);
        var size = img.Size();
        if (size.Width == 0 || size.Height == 0)
        {
            throw new IOException("Failed to read an image from " + imagePath);
        }

        return this.ReadGrid(img);
    }
}
