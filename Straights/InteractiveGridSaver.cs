// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.IO.Abstractions;

using Straights.Console;
using Straights.Solver.Builder;
using Straights.Solver.Converter;

internal sealed class InteractiveGridSaver(IFileSystem fileSystem, ReadWriteConsole console)
{
    public void SaveGrid(GridBuilder builder, string? suggestedPath)
    {
        console.Terminal.Write("Do you want to save the grid? [y/n]:");
        string answer = string.Empty;
        while (answer is not "y" and not "n")
        {
            answer = console.ReadLine()?.Trim() ?? string.Empty;
        }

        if (answer == "n")
        {
            return;
        }

        if (suggestedPath == null)
        {
            console.Terminal.Write("Enter path: ");
        }
        else
        {
            console.Terminal.Write($"Enter path or press return to use \"{suggestedPath}\":" + Environment.NewLine);
        }

        answer = console.ReadLine()?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(answer))
        {
            answer = suggestedPath ?? string.Empty;
        }

        if (answer.Length > 0)
        {
            var file = fileSystem.FileInfo.New(
                this.Expand(answer));

            file.Directory?.Create();

            var grid = new ConvertibleGrid(builder);
            if (!grid.CanWriteTo(file))
            {
                file = fileSystem.FileInfo.New(
                    fileSystem.Path.ChangeExtension(answer, ".txt"));
            }

            grid.WriteTo(file);
            console.Terminal.WriteLine("Grid written to " + file.FullName);
        }
    }

    private string Expand(string answer)
    {
        if (answer.StartsWith("~/", StringComparison.Ordinal)
            && !fileSystem.Directory.Exists("./~"))
        {
            answer = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                + answer[1..];
        }

        return Environment.ExpandEnvironmentVariables(answer);
    }
}
