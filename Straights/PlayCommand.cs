// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.IO.Abstractions;
using Straights.Console;
using Straights.Play;

public sealed class PlayCommand(
    IBrowserLauncher browserLauncher,
    IFileSystem fs
)
{
    public IWriteOnlyConsole Terminal { get; init; } = new Terminal();

    public IFileInfo? InputFile { get; init; }

    public int? PortOnLocalHost { get; init; }

    internal IWebApp WebApp { get; init; } = new WebApp();

    public int Run()
    {
        Uri baseUri = PlayUrl.DefaultBaseUri;
        Task? serverTask = null;
        if (this.PortOnLocalHost.HasValue)
        {
            var url = $"http://localhost:{this.PortOnLocalHost}/";
            var folder = this.GetWebRoot();

            serverTask = this.WebApp.Run(url, folder);

            baseUri = new Uri($"{url}?code=");
        }

        Uri playUrl = this.GetPlayUri(baseUri);

        this.Terminal.WriteLine("Launching " + playUrl);

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

        var (grid, _, _) = new GridLoader().LoadGrid(inputFile);

        Uri playUrl = new PlayUrl { BaseUri = baseUri }.GetPlayUri(
            grid.SolverGrid
        );
        return playUrl;
    }

    private IDirectoryInfo GetWebRoot()
    {
        string baseDir = AppContext.BaseDirectory;
        var webRoot = fs.Path.Combine(baseDir, "Play", "html");
        return fs.DirectoryInfo.New(webRoot);
    }
}
