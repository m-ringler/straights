// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image.Tests;

public static class TestData
{
    public static string GetPath(string name)
    {
        var assemblyPath = typeof(TestData).Assembly.Location;
        var dir = Path.GetDirectoryName(assemblyPath) ?? ".";
        var result = Path.Combine(dir, name);
        return !File.Exists(result)
            ? throw new FileNotFoundException($"File not found: {result}")
            : result;
    }
}