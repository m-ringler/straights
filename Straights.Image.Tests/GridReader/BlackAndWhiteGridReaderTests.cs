// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.Tests.GridReader;

using Argon;
using OpenCvSharp;
using Straights.Image.DigitReader;
using Straights.Image.GridReader;

public class BlackAndWhiteGridReaderTests
{
    private IDebugInfoWriter DebugInfoWriter { get; set; } =
        Directory.Exists("/tmp/straights")
            ? new DebugInfoWriter("/tmp/straights")
            : new NullDebugInfoWriter();

    [Theory]
    [InlineData("GridReader/grid-numbers1.png")]
    [InlineData("GridReader/grid-numbers2.png")]
    ////[InlineData("GridReader/grid-numbers3.png")]
    [InlineData("GridReader/grid-numbers4.png")]
    ////[InlineData("GridReader/grid-numbers5.png")]
    public void ReadGrid(string fileName)
    {
        // ARRANGE
        var png = TestData.GetPath(fileName);

        // ACT
        using var img = Cv2.ImRead(png, ImreadModes.Grayscale);
        var grid = new GridFinder(this.DebugInfoWriter).FindGrid(img);
        var gridReader = new BlackAndWhiteGridReader8Bit(
            new CombinedGridCellExtractor(),
            new ThreeMeansCellClassifier(),
            Mock.Of<IDigitReader>(x => x.TryReadDigit(It.IsAny<Mat>()) == 1)
        );
        var result = gridReader.ReadGrid(img, grid);
        var textLines =
            from row in result
            select new string([.. row.Select(ToChar)]);
        var actual = string.Join(Environment.NewLine, textLines);
        var expected = """
B_b_____b
__b#_#__B
_B_#__bbb
#__B_B___
_#_b___Bb
__#_B_B__
___#b____
_B__#__b_
_____#__b
""".ReplaceLineEndings();
        _ = actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("GridReader/grid-numbers1.png")]
    [InlineData("GridReader/grid-numbers2.png")]
    ////[InlineData("GridReader/grid-numbers3.png")]
    [InlineData("GridReader/grid-numbers4.png")]
    [InlineData("GridReader/grid-numbers5.png")]
    public void ReadGridAndNumbers(string fileName)
    {
        // ARRANGE
        var png = TestData.GetPath(fileName);

        // ACT
        using var img = Cv2.ImRead(png, ImreadModes.Grayscale);
        var grid = new GridFinder(this.DebugInfoWriter).FindGrid(img);

        using var digitClassifier = new DigitClassifierOnnx(
            "bekhzod-olimov-printed-digits"
        );
        var gridReader = new BlackAndWhiteGridReader8Bit(
            new CombinedGridCellExtractor(),
            new ThreeMeansCellClassifier(),
            new DigitReader1to9(new DigitFinderThreshold(4), digitClassifier)
        );
        var result = gridReader.ReadGrid(img, grid);
        var textLines =
            from row in result
            select new string([.. row.Select(ToChar)]);
        var actual = string.Join(Environment.NewLine, textLines);
        var expected = """
B_b_____b
__b#_#__B
_B_#__bbb
#__B_B___
_#_b___Bb
__#_B_B__
___#b____
_B__#__b_
_____#__b
""".ReplaceLineEndings();
        _ = actual.Should().Be(expected);

        var numbers = result
            .SelectMany(cells =>
                cells.OfType<Cell.IHasNumber>().Select(x => x.Number)
            )
            .ToArray();
        _ = numbers
            .Should()
            .Equal([5, 9, 8, 6, 1, 7, 1, 2, 9, 2, 3, 3, 8, 7, 4, 4, 5, 6]);
    }

    [Fact]
    public Task ReadmeSampleCode()
    {
        var pathToPng = TestData.GetPath("GridReader/grid-numbers1.png");

        IBlackAndWhiteGridReader reader =
            new GridReaderFactory().CreateGridReader();
        Cell[][] grid = reader.ReadGrid(pathToPng);

        var serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Objects,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };
        return Verify(JsonConvert.SerializeObject(grid, serializerSettings));
    }

    private static char ToChar(Cell cell)
    {
        return cell switch
        {
            Cell.BlackBlank => 'b',
            Cell.BlackNumber => 'B',
            Cell.WhiteNumber => '#',
            _ => '_',
        };
    }
}
