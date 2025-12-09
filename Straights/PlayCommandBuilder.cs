// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.CommandLine;
using System.CommandLine.Help;
using System.IO.Abstractions;
using Straights.Play;

internal sealed class PlayCommandBuilder(
    IFileSystem fs,
    Func<PlayCommand, int> execute
) : ICommandBuilder
{
    public const int DefaultPort = 7138;

    private const string FileArgumentDescription = """
        An image file with a Straights grid, or a Straights grid saved as .txt or .json;
        when omitted a new grid is generated.

        """;

    private readonly Argument<FileInfo?> fileArgument = new(
        name: "imageOrTextFile"
    )
    {
        Description = FileArgumentDescription,
        DefaultValueFactory = _ => null,
    };

    private readonly Option<bool> offlineOption = new(name: "--offline")
    {
        Description =
            "Serve the game on localhost, instead of using the default website.",
        Arity = ArgumentArity.Zero,
        DefaultValueFactory = _ => false,
    };

    private readonly Option<int> offlinePortOption = new(name: "--offline-port")
    {
        Description =
            "Serve the game on localhost on the specified port (implies --offline).",
        Arity = ArgumentArity.ExactlyOne,
        DefaultValueFactory = _ => DefaultPort,
        HelpName = "offline-port",
    };

    public PlayCommandBuilder(IFileSystem fs)
        : this(fs, program => program.Run()) { }

    public Command Build()
    {
        var command = new Command(
            "play",
            "Plays a straights puzzle in the default browser"
        )
        {
            this.fileArgument,
            this.offlineOption,
            this.offlinePortOption,
            new HelpOption(),
        };

        command.SetAction(this.RunProgram);

        return command;
    }

    private int RunProgram(ParseResult pr)
    {
        FileInfo? file = pr.GetValue(this.fileArgument);
        bool offline =
            pr.GetValue(this.offlineOption)
            || pr.GetResult(this.offlinePortOption)?.Implicit == false;
        int? port = offline ? pr.GetValue(this.offlinePortOption) : null;

        var inputFile = fs.Wrap(file);

        var program = new PlayCommand(new BrowserLauncher(), fs)
        {
            PortOnLocalHost = port,
            InputFile = inputFile,
        };

        int returnCode = execute(program);
        return returnCode;
    }
}
