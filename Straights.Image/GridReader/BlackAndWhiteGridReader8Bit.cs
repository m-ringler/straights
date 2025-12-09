// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

using Straights.Image.DigitReader;

public class BlackAndWhiteGridReader8Bit(
    IGridCellExtractor extractor,
    ICellClassifier cellClassifier,
    IDigitReader digitReader
)
{
    private readonly Cell.BlackBlank blackBlank = new();
    private readonly Cell.WhiteBlank whiteBlank = new();

    public Cell[][] ReadGrid(Mat img, ImageGrid grid)
    {
        using Mat imgUint8 = GetNormalized8BitImage(img);

        return grid.BuildArray(cell =>
        {
            using var cellMat = extractor.ExtractGridCell(imgUint8, cell);
            var cellType = cellClassifier.GetCellType(cellMat);
            return this.GetCell(cellMat, cellType);
        });
    }

    private static Mat GetNormalized8BitImage(Mat img)
    {
        var imgFloat = new Mat();
        try
        {
            img.MinMaxLoc(out double minVal, out double maxVal);
            img.ConvertTo(
                imgFloat,
                MatType.CV_8UC1,
                255 / maxVal,
                -minVal / maxVal
            );
            return imgFloat;
        }
        catch
        {
            imgFloat.Dispose();
            throw;
        }
    }

    private Cell GetCell(Mat cellMat, CellType cellType)
    {
        return cellType switch
        {
            CellType.WhiteBlank => this.whiteBlank,
            CellType.BlackBlank => this.blackBlank,
            CellType.WhiteNumber => this.GetBlackOnWhiteNumber(cellMat),
            CellType.BlackNumber => this.GetWhiteOnBlackNumber(cellMat),
            _ => throw new ArgumentOutOfRangeException(nameof(cellType)),
        };
    }

    private Cell GetBlackOnWhiteNumber(Mat cellMat)
    {
        using var mat1 = Mat.Ones(cellMat.Size(), cellMat.Type()) * 255;
        using var blackMat = mat1 - cellMat;
        int? result = digitReader.TryReadDigit(blackMat);
        return result == null
            ? this.whiteBlank
            : new Cell.WhiteNumber(result.Value);
    }

    private Cell GetWhiteOnBlackNumber(Mat cellMat)
    {
        int? result = digitReader.TryReadDigit(cellMat);
        return result == null
            ? this.blackBlank
            : new Cell.BlackNumber(result.Value);
    }
}
