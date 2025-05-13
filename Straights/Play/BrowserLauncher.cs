// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Play;

using System.Diagnostics;
using System.Runtime.InteropServices;

internal class BrowserLauncher : IBrowserLauncher
{
    public Process? OpenBrowser(string url)
    {
        try
        {
            var si = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = true,
            };
            return Process.Start(si);
        }
        catch
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}
