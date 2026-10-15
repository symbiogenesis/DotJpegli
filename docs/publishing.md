# Publishing to NuGet

[Documentation](README.md)

Publishing uses [NuGet trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing):
GitHub OIDC exchanges the workflow identity for a temporary NuGet API key. There
is no long-lived API key to store or rotate. Only the publishing job can request
an OIDC token, and it runs in the protected `nuget` environment.

## One-time account setup

In [GitHub environments](https://github.com/symbiogenesis/DotJpegli/settings/environments),
create `nuget` before publishing:

1. Restrict deployment branches to the selected **branch** `main`; allow no tags.
2. Add a required reviewer. For a sole maintainer, allow self-review so that the
   workflow initiator can approve publication. Disable administrator bypass.
3. Add environment **variable** `NUGET_USER` with the nuget.org profile name of
   the account creating the trusted policy, not its email address. This value
   is an account identifier, not a credential.

Sign in to [nuget.org trusted publishing](https://www.nuget.org/account/trustedpublishing)
with the account that owns or will own the package. Create this policy:

| Field | Value |
| --- | --- |
| Repository owner | `symbiogenesis` |
| Repository | `DotJpegli` |
| Workflow file | `publish-nuget.yml` (filename only) |
| Environment | `nuget` |
| Package scope | `DotJpegli` (exact package ID) |
| Allowed operations | Publish new versions; also allow a new package for the initial publication |

Use the intended package owner if publishing through a NuGet organization.
NuGet requires its own account authorization; GitHub's `GITHUB_TOKEN` cannot
publish to nuget.org. Do not add a `NUGET_API_KEY` repository secret.

## Publish a version

1. Set `<Version>` in `src/DotJpegli/DotJpegli.csproj` to the intended version,
   such as `1.0.0` or `1.1.0-rc.1`, and commit it with the release changes to
   `main`. Publishing uses that version without rewriting it.
2. Push the settled batch to `main` once. Reuse its automatic
   [Managed encoder CI](https://github.com/symbiogenesis/DotJpegli/actions/workflows/ci.yml)
   run; do not dispatch duplicate CI. The **Release NuGet package** job builds
   all three targets after tests and deployment checks, verifies package
   contents and repository provenance, and retains `nuget-<commit>` for 30 days.
3. After the entire CI run succeeds, run
   [Publish NuGet](https://github.com/symbiogenesis/DotJpegli/actions/workflows/publish-nuget.yml)
   on **main**, entering the exact version. Leave **publish** unchecked for a
   preview. The run checks the latest push CI for the same commit, downloads
   its package, verifies version/provenance/content, and reports its SHA-256.
4. To publish, run it again on the same `main` revision with **publish** checked,
   then review and approve the `nuget` environment deployment. The publishing
   job downloads the same immutable CI artifact and pushes it without rebuilding.
5. Confirm the version is listed at [nuget.org](https://www.nuget.org/packages/DotJpegli).
   NuGet validation and indexing can take time after upload succeeds.

This flow publishes only from `main`, with no publication tags or work-branch
pushes. Preview runs need no NuGet account configuration. Existing embedded PDBs
and SDK Source Link support are retained, so a separate `.snupkg` is unnecessary.
Actions are pinned to immutable commits, publication jobs run serially, and
duplicate package versions fail instead of being silently skipped.

## Retries and failed checks

- A failed or incomplete CI run blocks publication. Resolve the failure or rerun
  the existing CI run, then retry publishing. A targeted performance failure is
  still a CI failure; this workflow does not bypass it.
- If `main` advances, the next publishing run uses the new commit and requires
  that commit's successful CI artifact. It cannot publish an older branch or tag.
- If the artifact expires, rerun the existing CI run if it is still eligible
  (GitHub allows reruns for 30 days). Otherwise, make a new commit on `main` and
  use its automatic CI run. Keep the version if it has not been published.
  No local rebuild is uploaded for publication.
- NuGet versions are immutable. If an upload completed but a later step failed,
  check nuget.org before retrying. Change the version for a different package.
- OIDC failures usually indicate a mismatch in workflow filename, environment,
  `NUGET_USER`, policy owner or allowed package scope. Check the trusted policy
  instead of creating a permanent API key.

Local package verification (build check, no tests or benchmarks):

```powershell
dotnet restore src/DotJpegli/DotJpegli.csproj --locked-mode
dotnet pack src/DotJpegli/DotJpegli.csproj -c Release --no-restore --output <ABSOLUTE-TEMP-DIRECTORY> -p:ContinuousIntegrationBuild=true -p:RepositoryBranch=refs/heads/main
./tools/verify-nuget-package.ps1 -PackagePath <ABSOLUTE-PACKAGE-PATH> -ExpectedVersion 1.0.0 -ExpectedRevision <FULL-COMMIT-SHA>
```
