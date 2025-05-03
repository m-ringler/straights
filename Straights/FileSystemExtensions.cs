// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Straights;

using System.IO.Abstractions;

public static class FileSystemExtensions
{
    public static IFileInfo? Wrap(this IFileSystem fs, FileInfo? f)
    {
        return f?.Wrap(fs);
    }

    public static IFileInfo Wrap(this FileInfo f, IFileSystem fs)
    {
        return new FileInfoWrapper(fs, f);
    }

    public static IDirectoryInfo? Wrap(this IFileSystem fs, DirectoryInfo? d)
    {
        return d?.Wrap(fs);
    }

    public static IDirectoryInfo Wrap(this DirectoryInfo d, IFileSystem fs)
    {
        return new DirectoryInfoWrapper(fs, d);
    }
}
