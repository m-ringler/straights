// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Play;

using System.IO.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Primitives;

using PlayCore = Straights.Solver.Play;

internal class WebApp
{
    public static Task Run(string url, IDirectoryInfo folder)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            WebRootPath = folder.FullName,
        });
        _ = builder.WebHost.UseUrls(url);
        _ = builder.Services.AddControllers();

        static int GetValue(StringValues values, int defaultValue)
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

        var app = builder.Build();
        _ = app.UseDefaultFiles();
        _ = app.UseStaticFiles();
        _ = app.UseRouting();
        _ = app.UseEndpoints(endpoints =>
        {
            _ = endpoints.MapControllers();
            _ = endpoints.MapGet("/generate", async context =>
            {
                var query = context.Request.Query;
                int gridSize = GetValue(query["gridSize"], 9);
                var difficulty = GetValue(query["difficulty"], 3);
                var generatedGameCode = PlayCore.GenerateGameCode(
                    gridSize,
                    difficulty);
                await context.Response.WriteAsJsonAsync(new
                {
                    status = 0,
                    message = generatedGameCode,
                });
            });
        });

        return app.RunAsync();
    }
}