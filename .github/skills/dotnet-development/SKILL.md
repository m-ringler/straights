---
name: dotnet-development
description: Develop, test, format, package, or publish the repository's .NET projects using its existing solution filters and VS Code tasks.
---

# .NET development

Use the repository tasks in `.vscode/tasks.json` as the canonical workflow.

## Before editing

- Identify whether the change belongs in `Straights`, `Straights.Solver`, `Straights.Image`, or their test projects.
- Read the project file, shared build properties, and nearby tests.
- Use central package management from `Directory.Packages.props`.
- Preserve license headers and existing documentation conventions.
- Prefer snapshot-based tests over long assertion sequences; the project uses VerifyTests as its C# snapshot infrastructure.

## Build and test

- Prefer the relevant named task in `.vscode/tasks.json`.
- Use `straights-commandline.slnf` for command-line and library work in that solution filter.
- Use `straights-nuget.slnf` for NuGet packaging work.
- Add focused xUnit tests when behavior changes.
- Inspect Verify `.verified.txt` files rather than rewriting snapshots without understanding failures.

## Tooling

Local .NET tools are declared in `.config/dotnet-tools.json`.
CI and package/build conventions are documented by `.github/workflows/dotnet.yml`.
