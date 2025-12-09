// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights.Play;

using System.IO.Abstractions;
using System.Threading.Tasks;

internal interface IWebApp
{
    Task Run(string url, IDirectoryInfo folder);
}
