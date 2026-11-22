---
name: repository-validation
description: Select and run the smallest repository-native validation for a change, then expand to CI-equivalent checks when needed.
---

# Repository validation

Start by using the relevant labels in `.vscode/tasks.json`. Consult `package.json` for web scripts and `.github/workflows/dotnet.yml` for CI coverage.

## Selection

- C# code: run the narrowest relevant test project or task, then build the appropriate solution filter.
- TypeScript code: run the relevant Vitest test and TypeScript compilation.
- Cross-project or public API changes: validate the full command-line solution and relevant web checks.
- Package or publish changes: use the corresponding NuGet or publish task and inspect produced artifact paths.

## Evidence

- Read failures and warning output before changing code.
- Do not treat a build that has not restored dependencies as proof of success.
- Preserve unrelated generated or snapshot differences.
- Use CI workflow configuration only as a verification reference; do not copy it into task documentation.
