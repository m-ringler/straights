// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.IO.Abstractions;

using Straights.Console;
using Straights.Play;
using Straights.Solver;

public sealed class PlayCommand(
    IBrowserLauncher browserLauncher, IFileSystem fs)
{
    public IWriteOnlyConsole Terminal { get; init; } = new Terminal();

    public IFileInfo? InputFile { get; init; }

    public int? PortOnLocalHost { get; init; }

    public int Run()
    {
        Uri baseUri = PlayUrl.DefaultBaseUri;
        Task? serverTask = null;
        if (this.PortOnLocalHost.HasValue)
        {
            var url = $"http://localhost:{this.PortOnLocalHost}/";
            var folder = this.GetWebRoot();
            this.Terminal.WriteLine(folder.ToString() ?? string.Empty);

            serverTask = WebApp.Run(url, folder);

            baseUri = new Uri($"{url}?code=");
        }

        Uri playUrl = this.GetPlayUri(baseUri);

        this.Terminal.WriteLine(
            "Launching " + playUrl);

        _ = browserLauncher.OpenBrowser(playUrl.ToString());
        serverTask?.Wait();
        return 0;
    }

    private Uri GetPlayUri(Uri baseUri)
    {
        var inputFile = this.InputFile;
        if (inputFile is null)
        {
            return baseUri;
        }

        var (builder, _, _) = new GridLoader().LoadGrid(inputFile);

        var grid = builder.Convert();
        Uri playUrl = new PlayUrl
        {
            BaseUri = baseUri,
        }
        .GetPlayUri(grid.SolverGrid);
        return playUrl;
    }

    private IDirectoryInfo GetWebRoot()
    {
        var baseDir = AppContext.BaseDirectory;
        if (baseDir == null)
        {
            throw new InvalidOperationException("Failed to get the assembly directory.");
        }

        var webRoot = fs.Path.Combine(baseDir, "Play", "html");
        return fs.DirectoryInfo.New(webRoot);
    }
}
