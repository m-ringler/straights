// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests.Solver.Converter;

using Straights.Solver;
using Straights.Solver.Converter;
using Straights.Solver.Data;
using Straights.Solver.Simplification;

/// <summary>
/// Tests for <see cref="HtmlGridRenderer"/>.
/// </summary>
public class HtmlGridRendererTests
{
    internal const string Grid9x9 =
"""
9
w1,_,_,b,_,_,_,b,b
_,_,_,_,_,_,_,w9,_
b,_,_,_,w1,b,b9,_,_
_,_,b,b2,_,_,_,w7,_
_,b1,_,w9,_,_,_,b,_
w7,_,_,_,_,b,b,_,_
_,_,b,b,_,_,_,w3,b8
_,_,_,_,_,_,_,_,_
b,b,_,_,_,b5,_,_,_

""";

    [Fact]
    public void Write_ProducesCorrectString()
    {
        // ARRANGE
        var grid = ToGrid(Grid9x9);
        var solverGrid = SolverGrid.FromFieldGrid(grid);
        var simplifier = new ColumnRemoveSolvedNumbers();
        foreach (var column in solverGrid.Columns)
        {
            simplifier.Simplify(column);
        }

        using var writer = new StringWriter();

        // ACT
        var actual = solverGrid.Convert().ToHtml();

        // ASSERT
        _ = actual.Should().Be(
"""
<!DOCTYPE html>
<html>
<head>
<title>Straights</title>
<style>
* {
  box-sizing: border-box;
}

div.col {
    float: left;
}

div.col div {
  float: left;
  clear: left;
  padding-left: 4.5pt;
  padding-top: 2pt;
  padding-right: 4.5pt;
  padding-bottom: 2pt;
  color: #404090;
}

table.grid {
   border-collapse: collapse;
   font-family: LucidaSans, Helvetica, Arial, sans-serif;
   font-size: 21pt;
}

tr.grid_row td
{
   border: solid 1px #909090;
   margin: 0;
   text-align: center;
   height: 50pt;
   min-width: 50pt;
}

tr.grid_row
{
   margin: 0;
}

td.white_field_solved
{
   background-color: white;
   color: black;
}

td.white_field_unsolved
{
   font-size: 8pt;
   padding-left: 0.37em;
   padding-right: 0.37em;
}

td.black_field
{
    background-color: black;
    color: white;
}
</style>
</head>
<body><table class="grid">
<tr class="grid_row">
<td class="white_field_solved">1</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="black_field">&nbsp;</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>&nbsp;</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="black_field">&nbsp;</td>
<td class="black_field">&nbsp;</td>
</tr>
<tr class="grid_row">
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>&nbsp;</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>&nbsp;</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>&nbsp;</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_solved">9</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>&nbsp;</div><div>9</div></div>
</td>
</tr>
<tr class="grid_row">
<td class="black_field">&nbsp;</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>&nbsp;</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_solved">1</td>
<td class="black_field">&nbsp;</td>
<td class="black_field">9</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>&nbsp;</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>&nbsp;</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>&nbsp;</div><div>9</div></div>
</td>
</tr>
<tr class="grid_row">
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>&nbsp;</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="black_field">&nbsp;</td>
<td class="black_field">2</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>&nbsp;</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_solved">7</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>&nbsp;</div><div>9</div></div>
</td>
</tr>
<tr class="grid_row">
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>&nbsp;</div><div>8</div><div>9</div></div>
</td>
<td class="black_field">1</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_solved">9</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>&nbsp;</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="black_field">&nbsp;</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>&nbsp;</div><div>9</div></div>
</td>
</tr>
<tr class="grid_row">
<td class="white_field_solved">7</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>&nbsp;</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="black_field">&nbsp;</td>
<td class="black_field">&nbsp;</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>&nbsp;</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>&nbsp;</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>&nbsp;</div><div>9</div></div>
</td>
</tr>
<tr class="grid_row">
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>&nbsp;</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="black_field">&nbsp;</td>
<td class="black_field">&nbsp;</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>&nbsp;</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_solved">3</td>
<td class="black_field">8</td>
</tr>
<tr class="grid_row">
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>&nbsp;</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>&nbsp;</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>&nbsp;</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>&nbsp;</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>&nbsp;</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>&nbsp;</div><div>9</div></div>
</td>
</tr>
<tr class="grid_row">
<td class="black_field">&nbsp;</td>
<td class="black_field">&nbsp;</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>&nbsp;</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>&nbsp;</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>9</div></div>
</td>
<td class="black_field">5</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>&nbsp;</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>&nbsp;</div><div>8</div><div>&nbsp;</div></div>
</td>
<td class="white_field_unsolved"><div class="col"><div>1</div><div>2</div><div>3</div></div>
<div class="col"><div>4</div><div>5</div><div>6</div></div>
<div class="col"><div>7</div><div>&nbsp;</div><div>9</div></div>
</td>
</tr>
</table></body>
</html>

""");
    }

    private static Grid<SolverField> ToGrid(string grid)
    {
        return GridConverter.ParseBuilderText(grid).SolverFieldGrid;
    }
}
