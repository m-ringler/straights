// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Play;

using System.Diagnostics;

public interface IBrowserLauncher
{
    Process? OpenBrowser(string url);
}
