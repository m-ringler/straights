// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Solver;

using System.IO.Abstractions;

using Straights.Solver.Builder;
using Straights.Solver.Converter;
using Straights.Solver.Data;

/// <summary>
/// Converts bewtween different grid representations.
/// </summary>
public static class GridConverter
{
    /// <summary>
    /// Parses a grid from a builder text representation.
    /// </summary>
    /// <param name="builderText">The builder text to parse.</param>
    /// <returns>A convertible grid.</returns>
    public static ConvertibleGrid ParseBuilderText(string builderText)
    {
        var loader = new GridBuilderTextPersister();
        var builder = loader.Parse(builderText);
        return Convert(builder);
    }

    /// <summary>
    /// Parses a grid from a JSON representation.
    /// </summary>
    /// <param name="json">The JSON string to parse.</param>
    /// <returns>A convertible grid.</returns>
    public static ConvertibleGrid ParseJson(string json)
    {
        var grid = GridToIntArraysConverter.GridFromJson(json);
        return Convert(grid);
    }

    /// <summary>
    /// Converts a 3D integer array representation of a grid into a convertible grid.
    /// </summary>
    /// <param name="intArrays">The 3D integer array to convert.</param>
    /// <returns>A convertible grid.</returns>
    public static ConvertibleGrid Convert(int[][][] intArrays)
    {
        var unsolved = GridToIntArraysConverter.GridFromIntArrays(intArrays);
        return Convert(unsolved);
    }

    /// <summary>
    /// Returns a new <see cref="ConvertibleGrid" />
    /// for the specified grid builder.
    /// </summary>
    /// <param name="builder">The grid to convert.</param>
    /// <returns>
    /// A new convertible grid.
    /// </returns>
    public static ConvertibleGrid Convert(this GridBuilder builder)
    {
        return new ConvertibleGrid(builder);
    }

    /// <summary>
    /// Returns a new <see cref="ConvertibleGrid" />
    /// for the specified grid.
    /// </summary>
    /// <param name="solverFieldGrid">The grid to convert.</param>
    /// <returns>
    /// A new convertible grid.
    /// </returns>
    public static ConvertibleGrid Convert(this Grid<SolverField> solverFieldGrid)
    {
        return new ConvertibleGrid(solverFieldGrid);
    }

    /// <summary>
    /// Returns a new <see cref="ConvertibleGrid" />
    /// for the specified grid.
    /// </summary>
    /// <param name="solverGrid">The grid to convert.</param>
    /// <returns>
    /// A new convertible grid.
    /// </returns>
    public static ConvertibleGrid Convert(this SolverGrid solverGrid)
    {
        return new ConvertibleGrid(solverGrid);
    }

    /// <summary>
    /// Returns a new <see cref="ConvertibleGrid" />
    /// for the specified grid builder fields.
    /// </summary>
    /// <param name="builderFields">The grid to convert.</param>
    /// <returns>
    /// A new convertible grid.
    /// </returns>
    public static ConvertibleGrid Convert(this BuilderField?[][] builderFields)
    {
        int size = builderFields.Length;
        var builder = new GridBuilder(size);
        var allFields = from row in builderFields
                        from field in row
                        where field != null
                        select field;
        foreach (var field in allFields)
        {
            builder.Add(field);
        }

        return Convert(builder);
    }

    /// <summary>
    /// Loads a grid from a file and converts it into a convertible grid.
    /// </summary>
    /// <param name="file">The file to load the grid from.</param>
    /// <returns>A convertible grid.</returns>
    /// <exception cref="ArgumentException">Thrown if the file format is unsupported.</exception>
    public static ConvertibleGrid LoadFrom(IFileInfo file)
    {
        var fs = file.FileSystem;
        var extension = fs.Path.GetExtension(file.FullName).ToLowerInvariant();
        var text = fs.File.ReadAllText(file.FullName);

        ConvertibleGrid? result = extension switch
        {
            ".txt" => ParseBuilderText(text),
            ".json" => ParseJson(text),
            _ => null,
        };

        if (result == null)
        {
            throw new ArgumentException(
                paramName: nameof(file),
                message: "Unsupported input format: " + extension);
        }
        else
        {
            return result;
        }
    }

    /// <summary>
    /// Converts two grids (solved and unsolved) into a URL-safe parameter string
    /// (for use in Straights.Web).
    /// </summary>
    /// <param name="solved">The solved grid.</param>
    /// <param name="unsolved">The unsolved grid.</param>
    /// <param name="encodingVersion">The encoding version to use.</param>
    /// <returns>A URL-safe parameter string.</returns>
    public static string ToUrlParameter(
        Grid<SolverField> solved,
        Grid<SolverField> unsolved,
        byte encodingVersion = BinaryGameSerializer.EncodingVersion)
    {
        var binary = ToBinaryString(
            solved,
            unsolved,
            encodingVersion);
        return Base64UrlEncoder.EncodeBase64Url(binary);
    }

    /// <summary>
    /// Converts a URL-safe parameter string back into solved and unsolved grids.
    /// </summary>
    /// <param name="urlParameter">The URL parameter string to convert.</param>
    /// <param name="encodingVersion">The encoding version to use.</param>
    /// <returns>A tuple containing the solved and unsolved grids.</returns>
    public static
        (Grid<SolverField> Solved, Grid<SolverField> Unsolved)
            ConvertUrlParameter(
        string urlParameter,
        byte encodingVersion = BinaryGameSerializer.EncodingVersion)
    {
        var binary = Base64UrlEncoder.DecodeBase64Url(urlParameter);
        return ConvertBinaryString(binary, encodingVersion);
    }

    /// <summary>
    /// Converts two grids (solved and unsolved) into a binary string representation.
    /// </summary>
    /// <param name="solved">The solved grid.</param>
    /// <param name="unsolved">The unsolved grid.</param>
    /// <param name="encodingVersion">The encoding version to use.</param>
    /// <returns>A binary string representation of the grids.</returns>
    public static string ToBinaryString(
        Grid<SolverField> solved,
        Grid<SolverField> unsolved,
        byte encodingVersion = BinaryGameSerializer.EncodingVersion)
    {
        var writer = new BinaryStringWriter();
        ToBinary(new(solved, unsolved), encodingVersion, writer);

        return writer.ToString();
    }

    /// <summary>
    /// Converts a binary string representation back into solved and unsolved grids.
    /// </summary>
    /// <param name="binaryString">The binary string to convert.</param>
    /// <param name="encodingVersion">The encoding version to use.</param>
    /// <returns>A tuple containing the solved and unsolved grids.</returns>
    public static (Grid<SolverField> Solved, Grid<SolverField> Unsolved) ConvertBinaryString(
        string binaryString,
        byte encodingVersion = BinaryGameSerializer.EncodingVersion)
    {
        var reader = new BinaryStringReader(binaryString);
        return FromBinary(reader, encodingVersion);
    }

    internal static Game FromBinary(
        BinaryStringReader reader,
        byte encodingVersion)
    {
        return encodingVersion switch
        {
            BinaryGameSerializer.EncodingVersion => BinaryGameSerializer.FromBinary(reader),
            _ => throw new ArgumentException(
                    paramName: nameof(encodingVersion),
                    message: $"Unsupported encoding version: {encodingVersion}"),
        };
    }

    internal static void ToBinary(
        Game game,
        byte encodingVersion,
        BinaryStringWriter writer)
    {
        switch (encodingVersion)
        {
            case BinaryGameSerializer.EncodingVersion:
                BinaryGameSerializer.ToBinary(game, writer);
                break;
            case LuisWalterBinaryConverter.EncodingVersion:
                LuisWalterBinaryConverter.ToBinary(game, writer);
                break;
            default:
                throw new ArgumentException(
                    paramName: nameof(encodingVersion),
                    message: $"Unsupported encoding version: {encodingVersion}");
        }
    }
}
