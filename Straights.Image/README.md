# Straights.Image

[![NuGet Version](https://img.shields.io/nuget/v/Straights.Image)](https://www.nuget.org/packages/Straights.Image/) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Straights.Image is a dotnet library that reads single square Str8ts and Sudoku grids from screenshot images. The maximum supported grid size is 9x9.

This package depends on [OpenCvSharp5](https://github.com/shimat/opencvsharp) and includes platform-specific native runtime packages as dependencies.

| Platform | Runtime library |
| -------- | --------------- |
| Windows x64 | [OpenCvSharp5.runtime.win.slim](https://www.nuget.org/packages/OpenCvSharp5.runtime.win.slim) |
| Linux x64 | [OpenCvSharp5.official.runtime.linux-x64.slim](https://www.nuget.org/packages/OpenCvSharp5.official.runtime.linux-x64.slim) |

The Linux x64 runtime is built for manylinux 2.28 and supports Ubuntu 20.04+, Debian 10+, and RHEL/AlmaLinux 8+. The slim profile has no GUI dependencies. These runtime packages are transitive dependencies of Straights.Image; consumers do not need to add them explicitly. Linux ARM64 is not currently included.
See also

- <https://github.com/m-ringler/straights/blob/main/README.md#linux>

## Usage

<https://github.com/m-ringler/straights/blob/9e2cc25fb00b267d35a6ba8cc59163bd8f25d34a/Straights.Image.Tests/GridReader/BlackAndWhiteGridReaderTests.cs#L113-L115>
