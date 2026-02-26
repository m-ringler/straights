# Straights.Image

 [![NuGet Version](https://img.shields.io/nuget/v/Straights.Image)](https://www.nuget.org/packages/Straights.Image/) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Straights.Image is a dotnet library that reads single square Str8ts and Sudoku grids from screenshot images. The maximum supported grid size is 9x9.

This package depends on [OpenCvSharp](https://github.com/shimat/opencvsharp) and a working native OpenCvSharpExtern runtime library.

The [windows runtime library](https://www.nuget.org/packages/OpenCvSharp4.runtime.win) is a dependency of this package, so you don't need to add an explicit package reference.

For Linux, there is no single runtime library because the runtime library depends on other DLLs that are provided by the linux distribution. You will need to pick a suitable runtime library for the particular distribution that you build for. This runtime library must be added as a package reference to the application or test project that uses Straights.Image.

| Distribution | Runtime library |
|-|-|
| Ubuntu 22.04 | [OpenCvSharp4.official.runtime.ubuntu.22.04-x64.slim](https://www.nuget.org/packages/OpenCvSharp4.official.runtime.ubuntu.22.04-x64.slim) |
| Ubuntu 24.04 | [OpenCvSharp4.official.runtime.ubuntu.24.04-x64.slim](https://www.nuget.org/packages/OpenCvSharp4.official.runtime.ubuntu.24.04-x64.slim) |
| Other distros | [Search nuget.org](https://www.nuget.org/packages?page=2&q=OpenCvSharp4) or build the runtime yourself |

Users may have to install distribution packages that the runtime library depends on.
See also

* <https://github.com/m-ringler/straights/blob/main/README.md#linux>

## Usage

<https://github.com/m-ringler/straights/blob/17de5c3811c085044720280c893b5c0ccb642e7d/Straights.Image.Tests/GridReader/BlackAndWhiteGridReaderTests.cs#L111-L112>
