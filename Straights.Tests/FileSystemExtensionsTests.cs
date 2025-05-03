// SPDX-FileCopyrightText: 2025 Moritz Ringler
//
// SPDX-License-Identifier: MIT

namespace Straights.Tests;

using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

#pragma warning disable IO0004
#pragma warning disable IO0007

/// <summary>
///  Tests for the <see cref="FileSystemExtensions"/> class.
/// </summary>
public class FileSystemExtensionsTests
{
    [Fact]
    public void Wrap_FileInfo_ReturnsExpected()
    {
        var fileInfo = new FileInfo("test.txt");
        var fileSystem = new MockFileSystem();

        var actual = fileSystem.Wrap(fileInfo);

        var wrapper = actual.Should().BeOfType<FileInfoWrapper>().Subject;
        wrapper.FileSystem.Should().BeSameAs(fileSystem);
        wrapper.FullName.Should().Be(fileInfo.FullName);
    }

    [Fact]
    public void Wrap_NullFileInfo_ReturnsNull()
    {
        FileInfo? fileInfo = null;
        var fileSystem = new MockFileSystem();

        var actual = fileSystem.Wrap(fileInfo);

        actual.Should().BeNull();
    }

    [Fact]
    public void Wrap_DirectoryInfo_ReturnsExpected()
    {
        var directoryInfo = new DirectoryInfo("testDir");
        var fileSystem = new MockFileSystem();

        var actual = fileSystem.Wrap(directoryInfo);

        var wrapper = actual.Should().BeOfType<DirectoryInfoWrapper>().Subject;
        wrapper.FileSystem.Should().BeSameAs(fileSystem);
        wrapper.FullName.Should().Be(directoryInfo.FullName);
    }

    [Fact]
    public void Wrap_NullDirectoryInfo_ReturnsNull()
    {
        DirectoryInfo? directoryInfo = null;
        var fileSystem = new MockFileSystem();

        var actual = fileSystem.Wrap(directoryInfo);

        actual.Should().BeNull();
    }
}
