// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO.Abstractions;

internal sealed class PlayCommandBuilder(
        IFileSystem fs,
        Func<PlayCommand, int> execute)
    : ICommandBuilder
{
    public const int DefaultPort = 7138;

    private const string FileArgumentDescription =
        """
        An image file with a Straights grid, or a Straights grid saved as .txt or .json;
        when omitted a new grid is generated.

        """;

    private readonly Argument<FileInfo?> fileArgument = new(
        name: "imageOrTextFile",
        description: FileArgumentDescription,
        getDefaultValue: () => null);

    private readonly Option<bool> offlineOption = new(
        name: "--offline",
        getDefaultValue: () => false,
        description: "Serve the game on localhost, instead of using the default website.")
    {
        Arity = ArgumentArity.Zero,
    };

    private readonly Option<int> offlinePortOption = new(
        name: "--offline-port",
        getDefaultValue: () => DefaultPort,
        description: "Serve the game on localhost on the specified port (implies --offline).")
    {
        Arity = ArgumentArity.ExactlyOne,
    };

    public PlayCommandBuilder(
        IFileSystem fs)
        : this(fs, program => program.Run())
    {
    }

    public Command Build()
    {
        var command = new Command("play", "Plays a straights puzzle in the default browser");

        command.AddArgument(this.fileArgument);
        command.AddOption(this.offlineOption);
        command.AddOption(this.offlinePortOption);

        command.SetHandler(this.RunProgram);

        return command;
    }

    private void RunProgram(InvocationContext context)
    {
        var pr = context.ParseResult;

        FileInfo? file = pr.GetValueForArgument(this.fileArgument);
        bool offline = pr.GetValueForOption(this.offlineOption)
            || pr.FindResultFor(this.offlinePortOption)?.IsImplicit == false;
        int? port = offline
            ? pr.GetValueForOption(this.offlinePortOption)
            : null;

        var inputFile = fs.Wrap(file);

        var program = new PlayCommand(
            new BrowserLauncher(),
            fs)
        {
            PortOnLocalHost = port,
            InputFile = inputFile,
        };

        int returnCode = execute(program);
        context.ExitCode = returnCode;
    }
}
