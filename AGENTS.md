# Agent instructions

## Changes and validation

- Make self-contained logical commits and publish only `main`.
- Finish a batch of changes locally, then push its settled revision once.
- Local Release builds and focused test selections are allowed. Full test suites
  and all benchmarks run only on CI; do not approximate a full suite with filtered runs.

## Avoid duplicate CI runs

- A push to `main` already triggers `.github/workflows/ci.yml`. Reuse that run
  for validation; do not also dispatch the workflow manually for the same revision.
- Before any manual dispatch, inspect existing runs for the intended commit.
  Reuse its queued, running, or successful run. Prefer rerunning an existing failed
  run when a retry is needed.
- Dispatch manually only when no automatic run exists for the revision, or when
  the user explicitly requests an additional run.
- Collect timing and other diagnostics in the planned validation run. Do not
  launch a second concurrent workflow just to compare settings.
