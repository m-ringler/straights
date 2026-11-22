# Copilot instructions for straights

## Repository map

- `Straights/`: .NET command-line application.
- `Straights.Solver/`: puzzle solving, generation, and conversion library.
- `Straights.Image/`: screenshot/grid image processing library.
- `Straights.Web/`: browser-wasm web application.
- `webdev/ts/`: TypeScript and web test sources.
- `*.Tests/`: xUnit test projects.
- `test/`: puzzle and screenshot fixtures.

Treat `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `global.json`, and `NuGet.config` as shared build configuration. Package versions belong in `Directory.Packages.props`.

## Canonical tools

Use repository configuration instead of maintaining copied tool inventories:

- `.vscode/tasks.json` defines the VS Code build, test, packaging, publish, and web tasks.
- `package.json` defines npm scripts and Node tooling.
- `.config/dotnet-tools.json` defines local .NET tools.
- `.github/workflows/dotnet.yml` is the reference for the CI build and test sequence.
- `global.json` defines the required .NET SDK line.

Prefer an existing task or script over inventing a new command. Keep task label names and scripts in source files stable.

## Engineering rules

- Read only the relevant project and tests before changing code.
- Preserve existing public APIs unless the task explicitly requires an API change.
- Follow existing namespaces, project boundaries, naming, formatting, and license-header conventions.
- `net10.0`, nullable reference types, implicit usings, and warnings-as-errors apply repository-wide through `Directory.Build.props`.
- Use dependency injection and existing abstractions, especially `IFileSystem`, for testability.
- Keep solver, image, command-line, and web responsibilities in their existing projects.
- Do not edit generated, downloaded, build-output, coverage, or publish directories.
- Make package and SDK changes only in the central configuration files.

## Validation

- For focused changes, run the smallest relevant task or test project first.
- Use the task names in `.vscode/tasks.json`; do not repeat task arguments in documentation or instructions.
- Run both relevant web and .NET validation for cross-layer changes.
- Preserve Verify snapshot files; if an intentional snapshot change is required, run the repository's snapshot accept workflow and inspect the diff.
- Report validation commands and failures precisely.
