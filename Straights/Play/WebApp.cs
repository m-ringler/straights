// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Play;

using System.IO.Abstractions;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using PlayCore = Straights.Solver.Play;

/// <summary>
/// This class starts a web application for the Straights game.
/// </summary>
/// <remarks>
/// This web app is started when the user calls `straights play`
/// with the `--offline` flag.
/// <para/>
/// The web app consists of static HTML and Javascript
/// plus a) the endpoint to generate a new game code which is
/// mapped to the <see cref="PlayCore.GenerateGameCode(int, int)"/>
/// method and b) the endpoint to generate a hint which is
/// mapped to the <see cref="PlayCore.GenerateHint(string)"/>
/// method.
/// </remarks>
internal class WebApp : IWebApp
{
    /// <summary>
    /// Runs the web application.
    /// </summary>
    /// <param name="url">The URL to host the application.</param>
    /// <param name="folder">The directory containing the web root.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task Run(string url, IDirectoryInfo folder)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            WebRootPath = folder.FullName,
        });
        _ = builder.WebHost.UseUrls(url);
#if DEBUG
        _ = builder.WebHost.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
#endif
        _ = builder.Services.AddControllers();

        var app = builder.Build();
        _ = app.UseDefaultFiles();
        _ = app.UseStaticFiles();
        _ = app.UseRouting();
        _ = app.UseEndpoints(endpoints =>
        {
            _ = endpoints.MapControllers();

            _ = endpoints.MapGet(
                "/generate",
                GetRequestDelegate(GenerateGame));

            _ = endpoints.MapPost(
                "/hint",
                GetRequestDelegate(GenerateHint));
        });

        return app.RunAsync();
    }

    private static RequestDelegate GetRequestDelegate(
        Func<HttpRequest, Task<string>> func)
    {
        return context => HandleEndpointAsync(context, func);
    }

    private static async Task HandleEndpointAsync(
        HttpContext context,
        Func<HttpRequest, Task<string>> operation)
    {
        context.Response.ContentType = "application/json";

        try
        {
            var result = await operation(context.Request);
            await context.Response.WriteAsJsonAsync(
                new ApiResponse(0, result));
        }
        catch (Exception ex)
        {
            await context.Response.WriteAsJsonAsync(
                new ApiResponse(1, ex.Message));
        }
    }

    private static Task<string> GenerateGame(HttpRequest request)
    {
        var result = PlayCore.GenerateGameCode(
            GetValue(request.Query["gridSize"], 9),
            GetValue(request.Query["difficulty"], 3));
        return Task.FromResult(result);
    }

    private static async Task<string> GenerateHint(HttpRequest request)
    {
        if (request.Body == null)
        {
            throw new ArgumentException("Request body cannot be null");
        }

        using var utf8Reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);
        var gameAsJson = await utf8Reader.ReadToEndAsync();
        return PlayCore.GenerateHint(gameAsJson);
    }

    private static int GetValue(StringValues values, int defaultValue)
    {
        if (values.Count == 0)
        {
            return defaultValue;
        }

        if (int.TryParse(values[0], out var value))
        {
            return value;
        }

        return defaultValue;
    }

    /// <summary>
    /// Represents the response from the API endpoints.
    /// </summary>
    /// <param name="Status">The status code. 0 for success, 1 for error.</param>
    /// <param name="Message">The response message or error message.</param>
    private record class ApiResponse(int Status, string Message);
}
