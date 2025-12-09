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
    public int Size => this.solverGrid?.Grid.Size ?? this.Builder.Size;

    /// <summary>
    /// Gets the builder representation of the
    /// grid.
    /// </summary>
    public GridBuilder Builder
    {
        get =>
            this.builder ??= SolverGridToBuilderConverter.ToBuilder(
                this.SolverFieldGrid
            );
    }

    /// <summary>
    /// Gets the <see cref="Data.SolverGrid"/> representation.
    /// </summary>
    public SolverGrid SolverGrid
    {
        get =>
            this.solverGrid ??= SolverGrid.FromFieldGrid(this.SolverFieldGrid);
    }

    /// <summary>
    /// Gets the 'square grid of
    /// <see cref="SolverField"/>s' representation.
    /// </summary>
    public Grid<SolverField> SolverFieldGrid
    {
        get =>
            this.solverFieldGrid ??=
                this.solverGrid?.Grid
                ?? BuilderToSolverGridConverter.ToSolverFields(
                    this.builder!.GetFields()
                );
    }

    /// <summary>
    /// Gets the text representation of the <see cref="Builder"/>.
    /// </summary>
    /// <remarks>
    /// The builder text format is described
    /// in <see cref="GridBuilderTextPersister"/>.
    /// </remarks>
    /// <returns>
    /// The grid (excluding notes) as builder text.
    /// </returns>
    public string ToBuilderText()
    {
        return this.Builder.ToString();
    }

    /// <summary>
    /// Gets a static HTML representation of the grid.
    /// </summary>
    /// <remarks>
    /// Use this method to get an easily readable visual
    /// representation of the grid.
    /// </remarks>
    /// <returns>
    /// The grid (including notes if present) as a static
    /// HTML document.
    /// </returns>
    public string ToHtml()
    {
        var htmlRenderer = new HtmlGridRenderer();
        return htmlRenderer.GetString(this.SolverGrid.Grid);
    }

    /// <summary>
    /// Gets the JSON array representation of the grid (including notes).
    /// </summary>
    /// <remarks>
    /// The JSON array representation is the JSON serialization
    /// of the 3D integer array representation described in
    /// <see cref="GridConverter.Convert(int[][][])"/>.
    /// </remarks>
    /// <returns>
    /// The JSON array representation of the grid.
    /// </returns>
    public string ToJson()
    {
        return GridToIntArraysConverter.ToJson(this.SolverGrid.Grid);
    }

    /// <summary>
    /// Gets the integer array representation of the grid (including notes).
    /// </summary>
    /// <remarks>
    /// The 3D integer array representation is
    /// described in
    /// <see cref="GridConverter.Convert(int[][][])"/>.
    /// </remarks>
    /// <returns>
    /// The JSON array representation of the grid.
    /// </returns>
    public int[][][] ToIntArrays()
    {
        return GridToIntArraysConverter.ToIntArrays(this.SolverGrid.Grid);
    }

    /// <summary>
    /// Gets a value indicating whether the grid can be
    /// written to the specified file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method looks at the extension of
    /// <paramref name="file"/> to determine wether the format is
    /// supported. It does not check whether the file
    /// is writable.
    /// </para>
    /// <para>
    /// The file format is determined by the file extension.
    /// Supported formats are:
    /// <list type="bullet">
    /// <item>.txt</item>
    /// <description>The builder text format.</description>
    /// <item>.json</item>
    /// <description>The JSON array format.</description>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="file">The file to write to.</param>
    /// <returns>
    /// True if the file can be written, false otherwise.
    /// </returns>
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

    /// <summary>
    /// Writes the grid to the specified file.
    /// </summary>
    /// <param name="file">
    /// The file to write to.
    /// </param>
    /// <remarks>
    ///  The file will be overwritten if it exists.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Thrown when the file format inferred from the file extension is not supported.
    /// </exception>
    /// <exception cref="IOException">
    /// Thrown when an I/O error occurs.
    /// </exception>
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
                message: "Unsupported output format: " + extension
            );
        }
        else
        {
            fs.File.WriteAllText(file.FullName, gridAsString, Encoding.UTF8);
        }
    }
}
