---
name: run-megalinter
description: Run the repository's CI-equivalent MegaLinter locally using its pinned .NET flavor and configuration.
---

# Run MegaLinter locally

The CI reference is `.github/workflows/mega-linter.yml`. It runs the MegaLinter `dotnet` flavor at `v10.2.0` (check the workflow file for the exact version, update this SKILL.md on mismatch). Shared lint settings, including full-codebase validation and disabled fixes, are in `.mega-linter.yml`.

## Prerequisites

Install Node.js and Docker, and ensure the Docker daemon is running. The local `mega-linter-runner` uses Docker to run the linters.

## Run

From the repository root, run in a visible terminal:

```sh
npx --yes mega-linter-runner --flavor dotnet --release v10.2.0 --user-map
```

Keep the process in the foreground and leave stdout/stderr attached to the terminal so progress and linter output remain visible. Do not redirect output to a file, pipe it through another command, or launch it detached. Add `--debug` when more detailed progress is useful.
The `--user-map` option runs the container as the invoking user so generated reports in the mounted workspace are not root-owned.

When invoking through a shell tool that returns while the process is still running, retain its process/session ID and read output from that same session until it finishes. Do not start a second run to check progress.

## Docker bridge networking failure

If the runner fails before linting with a Docker bridge/veth error such as `failed to add the host ... pair interfaces: operation not supported`, Docker's bridge network may be unavailable. On systems where Docker supports host networking, run the same pinned image directly, mounting the repository at MegaLinter's default workspace:

```sh
docker run --platform linux/amd64 --network host --rm \
  --user "$(id -u):$(id -g)" \
  -v "$(pwd):/tmp/lint:rw" \
  ghcr.io/oxsecurity/megalinter-dotnet:v10.2.0
```

Run this from the repository root in the foreground to see progress. The container reads `.mega-linter.yml` from the mounted workspace. Passing the invoking user's UID/GID keeps generated reports from being root-owned. Host networking is platform-dependent; otherwise, restore Docker bridge networking and use the runner command above.

The runner reads `.mega-linter.yml` from the current repository. `VALIDATE_ALL_CODEBASE: true` means it checks the full codebase, matching CI rather than only changed files. `APPLY_FIXES: none` prevents automatic fixes; review reported issues and make changes deliberately.

Ignore warning-classified results; do not fix or report them as failures. In particular, a non-blocking finding shown inside a linter's warning result is still a warning. Treat error-classified results or a nonzero final exit status as failures, inspect the corresponding linter log under `megalinter-reports/`, and rerun to verify any fixes.

MegaLinter writes reports to `megalinter-reports/` and `mega-linter.log`; `megalinter-reports/` is git-ignored.

## Optional disk cleanup

After the run has finished, remove the downloaded MegaLinter image if it is not needed for another run:

```sh
docker image rm ghcr.io/oxsecurity/megalinter-dotnet:v10.2.0
```

For other versions, inspect `docker image ls ghcr.io/oxsecurity/megalinter-dotnet` and remove only the specific MegaLinter image tags you no longer need. Removing an image means Docker must download it again next time.

Reports are usually smaller than the image, but can also be removed after reviewing or saving anything needed:

```sh
rm -r megalinter-reports
```

Do not use broad Docker prune commands for this cleanup; they can remove unrelated images, caches, or containers.

For CI behavior or configuration changes, consult `.github/workflows/mega-linter.yml` and `.mega-linter.yml` rather than duplicating their settings here.
