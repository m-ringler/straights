// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver.Converter;

using System.IO.Abstractions;
using System.Text;

using Straights.Solver.Builder;
using Straights.Solver.Data;

/// <summary>
/// A convertible representation of a straights grid,
/// usually created via <see cref="GridConverter"/>.
/// </summary>
public class ConvertibleGrid
{
    private GridBuilder? builder;

    private Grid<SolverField>? solverFieldGrid;

    private SolverGrid? solverGrid;

    public ConvertibleGrid(GridBuilder builder)
    {
        this.builder = builder;
    }

    public ConvertibleGrid(SolverGrid solverGrid)
    {
        this.solverGrid = solverGrid;
    }

    public ConvertibleGrid(Grid<SolverField> solverFieldGrid)
    {
        this.solverFieldGrid = solverFieldGrid;
    }

    /// <summary>
    /// Gets the size of the grid.
    /// </summary>
    public int Size => this.solverGrid?.Grid.Size
        ?? this.Builder.Size;

    /// <summary>
    /// Gets the builder representation of the
    /// grid.
    /// </summary>
    public GridBuilder Builder
    {
        get => this.builder ??=
            SolverGridToBuilderConverter.ToBuilder(this.SolverFieldGrid);
    }

    /// <summary>
    /// Gets the <see cref="Data.SolverGrid"/> representation.
    /// </summary>
    public SolverGrid SolverGrid
    {
        get => this.solverGrid ??=
            SolverGrid.FromFieldGrid(this.SolverFieldGrid);
    }

    public Grid<SolverField> SolverFieldGrid
    {
        get => this.solverFieldGrid
            ??= this.solverGrid?.Grid
            ?? BuilderToSolverGridConverter.ToSolverFields(this.builder!.GetFields());
    }

    public string ToBuilderText()
    {
        return this.Builder.ToString();
    }

    public string ToHtml()
    {
        var htmlRenderer = new HtmlGridRenderer();
        return htmlRenderer.GetString(this.SolverGrid.Grid);
    }

    public string ToJson()
    {
        return GridToIntArraysConverter.ToJson(
                this.SolverGrid.Grid);
    }

    public int[][][] ToIntArrays()
    {
        return GridToIntArraysConverter.ToIntArrays(
            this.SolverGrid.Grid);
    }

    public virtual bool CanWriteTo(IFileInfo file)
    {
        var fs = file.FileSystem;
        var extension = fs.Path.GetExtension(file.FullName).ToLowerInvariant();
        return extension switch
        {
            ".html" or ".htm" => true,
            ".txt" => true,
            ".json" => true,
            _ => false,
        };
    }

    public virtual void WriteTo(IFileInfo file)
    {
        var fs = file.FileSystem;
        var extension = fs.Path.GetExtension(file.FullName).ToLowerInvariant();
        string? gridAsString = extension switch
        {
            ".html" or ".htm" => this.ToHtml(),
            ".txt" => this.ToBuilderText(),
            ".json" => this.ToJson(),
            _ => null,
        };

        if (gridAsString == null)
        {
            throw new ArgumentException(
                paramName: nameof(file),
                message: "Unsupported output format: " + extension);
        }
        else
        {
            fs.File.WriteAllText(file.FullName, gridAsString, Encoding.UTF8);
        }
    }
}
