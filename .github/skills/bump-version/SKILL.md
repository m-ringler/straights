---
name: bump-version
description: Bump the coordinated project, npm, web UI, PWA cache, and applicable local-tool versions following this repository's git-history convention.
---

# Bump version

Recent commits named `Bump version` update one release version consistently across the repository.

## Version locations

Update the same new release version in:

- `Directory.Build.props`: `<VersionPrefix>`.
- `package.json`: top-level `version`.
- `package-lock.json`: both the root `name` package entry and root package record.
- `webdev/base/shared/index.html`: the `Straights.Web` version displayed in the about dialog.
- `webdev/ts/shared/service-worker.ts`: `CACHE_NAME`, using the `v<version>` form.

If a release bump commit also updates `.config/dotnet-tools.json`, inspect the commit's intent before changing tool entries. Do not change a tool version merely because its package happens to match the project version.

## Workflow

1. Determine the target semantic version from the user or the next release branch.
2. Replace the old version in the canonical locations above; do not add build IDs or CI-only suffixes.
3. Leave `Directory.Packages.props` and dependency versions unchanged.
4. Run `npm ci` if dependencies need to be refreshed, then use the repository's web validation tasks.
5. Build and test the command-line solution as covered by `.vscode/tasks.json`.
6. Review the diff to ensure every occurrence belongs to the release bump and no unrelated files changed.
7. Use `Bump version` as the commit subject unless project conventions or the user specify otherwise.
