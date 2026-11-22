---
name: web-development
description: Work on the TypeScript, Vitest, browser-wasm, PWA, or web application areas of the straights repository.
---

# Web development

Use `package.json`, `tsconfig.json`, `vitest.config.mts`, and `.vscode/tasks.json` as the canonical configuration sources.

## Source areas

- Shared browser logic is under `webdev/ts/shared/`.
- Web service and WASM integration are under `webdev/ts/webservice/` and `webdev/ts/wasm/`.
- Tests are under `webdev/ts/test/`.
- Static web assets are under `webdev/base/`.
- The .NET browser-wasm host is `Straights.Web/`.

## Workflow

- Use existing npm scripts and VS Code task names rather than duplicating command lines.
- Compile TypeScript before building or publishing the web application.
- Run the focused Vitest suite for changed behavior, then broaden validation when cross-module behavior is involved.
- Keep strict TypeScript and ES2022 conventions from `tsconfig.json`.
- Preserve PWA, service-worker, offline, and WASM integration behavior when editing shared web code.
