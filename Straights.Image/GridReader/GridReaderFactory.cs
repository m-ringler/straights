// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.GridReader;

using Straights.Image.DigitReader;

public class GridReaderFactory
{
    public int DigitPadding { get; set; } = 4;

    public string ModelName { get; set; } = "bekhzod-olimov-printed-digits";

    public IBlackAndWhiteGridReader CreateGridReader(
        string? debugFolder = null)
    {
        // Relying on finalizer to clean up the InferenceSession.
        var digitClassifier = new DigitClassifierOnnx(
            this.ModelName);

        IDebugInfoWriter infoWriter = debugFolder == null
            ? new NullDebugInfoWriter()
            : new DebugInfoWriter(debugFolder);

        IGridCellExtractor cellExtractor
            = new CombinedGridCellExtractor();
        if (debugFolder != null)
        {
            cellExtractor = new DebugGridCellExtractor(
                cellExtractor,
                infoWriter);
        }

        var gridReader = new BlackAndWhiteGridReader8Bit(
            cellExtractor,
            new ThreeMeansCellClassifier(),
            new DigitReader1to9(
                new DigitFinderThreshold(this.DigitPadding),
                digitClassifier));

        var gridFinder = new GridFinder(infoWriter);
        return new FullGridReader(gridFinder, gridReader);
    }

    private sealed record class FullGridReader(
        GridFinder GridFinder,
        BlackAndWhiteGridReader8Bit GridReader)
        : IBlackAndWhiteGridReader
    {
        public Cell[][] ReadGrid(Mat img)
        {
            var grid = this.GridFinder.FindGrid(img);
            var cells = this.GridReader.ReadGrid(img, grid);
            return cells;
        }
    }
}