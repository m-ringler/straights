// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.CommandLine;
using System.IO.Abstractions;
using System.Text;

public sealed class Program
{
    internal static RootCommand Build(IFileSystem fs)
    {
        var command = new RootCommand("Straights Puzzle Toolkit");

        var generate = new GenerateCommandBuilder(fs).Build();
        command.Add(generate);

        var play = new PlayCommandBuilder(fs).Build();
        command.Add(play);

        var solve = new SolveCommandBuilder(fs).Build();
        command.Add(solve);

        var edit = new EditCommandBuilder(fs).Build();
        command.Add(edit);

        var convert = new ConvertCommandBuilder(fs).Build();
        command.Add(convert);
        return command;
    }

    private static int Main(string[] args)
    {
        EnsureConsoleEncodingIsUnicode();
        RootCommand command = Build(new FileSystem());

        try
        {
            return command.Parse(args).Invoke();
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine(ex.Message);
            return 255;
        }
    }

    private static void EnsureConsoleEncodingIsUnicode()
    {
        if (System.Console.OutputEncoding != Encoding.Unicode)
        {
            System.Console.OutputEncoding = Encoding.UTF8;
        }
    }
}
