// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Image;

using System.Reflection;
using System.Runtime.InteropServices;

/// <summary>
/// Tells OpenCvSharp where to find the native OpenCvSharpExtern library on Linux,
/// since the default loading mechanism cannot handle different files for
/// different Ubuntu versions. On Windows, the default loading mechanism works fine
/// and we don't need to do anything special.
/// </summary>
public static class NativeLibraryLoader
{
    private static readonly Lazy<bool> LibraryLoaded = new(() =>
    {
        Initialize();
        return true;
    });

    /// <summary>
    /// Ensures that the native library loader is initialized.
    /// </summary>
    public static void EnsureInitialized()
    {
        // Accessing the Value will trigger initialization if it hasn't already been triggered.
        _ = LibraryLoaded.Value;
    }

    private static void Initialize()
    {
        // Only set up the resolver for Linux
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return;
        }

        var libPath = FindLib();
        if (libPath == null)
        {
            // Let the default resolver handle it,
            // User may have supplied their own OpenCvSharpExtern.so
            // or be on a different Linux distro.
            return;
        }

        NativeLibrary.SetDllImportResolver(
            typeof(Cv2).Assembly,
            new DllResolver(libPath).Resolve
        );
    }

    private static string? FindLib()
    {
        // Detect Ubuntu version
        string? ubuntuVersion = GetUbuntuVersion();

        if (ubuntuVersion is null || ubuntuVersion.Length < 3)
        {
            return null;
        }

        string shortVersion = ubuntuVersion[0..3];
        string? soFileName = shortVersion switch
        {
            "22." or "24." => $"libOpenCvSharpExtern-u{shortVersion}so",
            _ => null,
        };

        if (soFileName == null)
        {
            return null;
            // Let the default resolver handle it,
            // User may have supplied their own OpenCvSharpExtern.so
            // or be on a different Linux distro.
        }

        // Local build (e.g. dotnet run)
        string nativeLibPath = Path.Combine(
            AppContext.BaseDirectory,
            "runtimes",
            "linux-x64",
            "native",
            soFileName
        );

        if (File.Exists(nativeLibPath))
        {
            return nativeLibPath;
        }

        // Published
        nativeLibPath = Path.Combine(AppContext.BaseDirectory, soFileName);

        if (File.Exists(nativeLibPath))
        {
            return nativeLibPath;
        }

        // Not found, let the default resolver look for it.
        return soFileName;
    }

    private static string? GetUbuntuVersion()
    {
        // Read /etc/os-release to detect Ubuntu version
        if (!File.Exists("/etc/os-release"))
        {
            return null;
        }

        string[] lines;
        try
        {
            lines = File.ReadAllLines("/etc/os-release");
        }
        catch (IOException)
        {
            return null;
        }

        var info =
            from line in lines
            let parts = line.Split('=', 2)
            where parts.Length == 2
            select KeyValuePair.Create(parts[0], parts[1].Trim('"'));
        var infoDict = new Dictionary<string, string>(
            info,
            StringComparer.OrdinalIgnoreCase
        );

        if (
            infoDict.TryGetValue("NAME", out var name)
            && "Ubuntu".Equals(name, StringComparison.OrdinalIgnoreCase)
            && infoDict.TryGetValue("VERSION_ID", out var version)
        )
        {
            return version;
        }

        return null;
    }

    private sealed class DllResolver(string nativeLibPath)
    {
        public IntPtr Resolve(
            string libraryName,
            Assembly assembly,
            DllImportSearchPath? searchPath
        )
        {
            return libraryName == "OpenCvSharpExtern"
                ? NativeLibrary.Load(nativeLibPath, assembly, searchPath)
                : IntPtr.Zero;
        }
    }
}
