# straights

Solves and generates Straights (or Str8ts) puzzles.

straights accepts both text files and screenshots as input.

## License

The Straights command line and web apps are licensed under GPL 3.0 or any later version.
The Straights.Solver and Straights.Image libraries are licensed under the MIT license.

## Straights Command Line Application

~~~txt
Description:
  Straights Puzzle Toolkit

Usage:
  straights [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  generate                   Generates a straights puzzle
  play <imageOrTextFile>     Plays a straights puzzle in the default browser
  solve <imageOrTextFile>    Solves a straights puzzle []
  edit <imageOrTextFile>     Edits a straights puzzle []
  convert <imageOrTextFile>  Converts a straights puzzle

~~~

## Straights.Solver

Straights.Solver is the core library behind the straights command line app and the straights webapp. It solves, generates, and converts
straights puzzles.

Straights.Solver theoretically supports grid sizes up to 64 x 64, but
I have not generated a grid larger than 12 x 12 yet.

## Straights.Image

Straights.Image is a library that reads screenshots of Straights and Sudoku grids.
It has no dependency on Straights.Solver or the Straights command line application (it is used by the latter).

It uses [OpenCvSharp](https://github.com/shimat/opencvsharp) to detect the grids, and - on Linux - you may have to build
an appropriate OpenCvSharpExtern.so runtime. See 'Installation' below.

Straights.Image supports grid sizes up to 9 x 9.

## Straights.Web

[Straights.Web](./Straights.Web/Readme.md) is a Straights webapp that uses Straights.Solver to generate new grids.
Try it online at <http://mospace.de/str8ts/>.

## Status

The code works but is still pre-1.0,
which means that breaking changes in the APIs can happen at every release.
In particular:

* Different releases of the command line app may produce different grids for the same arguments even if you pass an explicit random seed.
* The library APIs are not stable, and may change in every release.
* The data formats uses by the webapp may still change.

## Installation

### Windows

Everything should work out of the box.
To use the command line app put it somewhere on your $env:Path,
to use any of the libraries just add a package reference to the nuget package.

### Linux

Essentially the same as for windows.

But the Linux native runtimes of OpenCvSharp used by Straights.Image depend on libraries that need to be installed on the system. These libraries are different for different distributions, e. g. for ubuntu-22.04 and ubuntu-24-04.

If Straights.Image fails with an "Unable to load shared library 'OpenCvSharpExtern' or one of its dependencies." error, you can [check which dependencies are missing](https://github.com/shimat/opencvsharp/issues/1618#issuecomment-1846537140), and install them. Alternatively, you can simply install all the libraries used to build it:

<https://github.com/m-ringler/opencvsharp/blob/40c5fd3f8e56382eddd77cf9ada780ffd2932cef/.github/workflows/ubuntu24.yml#L28C1-L51C30>

If you are neither on Ubuntu-22 nor on Ubuntu 24, you may have to build your own OpenCvSharpExtern.so.

As there is no official OpenCvSharp nuget package for Ubuntu-24, we're using [our own nuget](https://www.nuget.org/packages/m-ringler.OpenCvSharp4.ubuntu24.runtime.linux-x64/) built from [a fork of the OpenCvSharp repo](https://github.com/m-ringler/opencvsharp/) for the Ubuntu-24 build of the straights command line app.

## Building

You'll need a dotnet SDK that matches the SDK version in [global.json](./global.json).

For Straights.Web, you need to download and activate the
[Emscripten SDK](https://emscripten.org/docs/getting_started/downloads.html#sdk-download-and-install).

## Backlog

* Increase test coverage
* Nugetize the libs
* Add more XML doc(use Copilot?)
* Microsoft.CodeAnalysis.PublicApiAnalyzers
* Migrate to slnx format

* Issues/Bugs
  * Digit recognition sometimes fails on 3/5

## Credits

* [Luis Walter](https://github.com/daandtu/Str8ts?search=1): the straights webapp is derived from his.
* [Kshitij Dhama](https://www.kaggle.com/datasets/kshitijdhama/printed-digits-dataset/data): the digit-recognition model in Straights.Image is trained on his printed-digits dataset.
* [Bekhzod Olimov](https://www.kaggle.com/code/killa92/100-accurate-digits-classifier-using-pytorch-timm): the pytorch code used to train the digit-recognition model in Straights.Image is largely his.
