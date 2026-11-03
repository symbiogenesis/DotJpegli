# Publishing releases

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

The root `README.md` is the source for both GitHub and the package page. Packing
generates the package's `README.md` under `obj`, removing the HTML wrapper used
to center the header on GitHub. Edit the root README rather than the generated copy.

1. Set `<Version>` in `src/DotJpegli/DotJpegli.csproj` to the intended stable
   three-part version, such as `1.0.0` or `1.1.0`, and commit it with the release
   changes to `main`. The CLI and MSI use the same version. Windows Installer
   permits major/minor values through 255 and a patch value through 65535;
   prerelease labels are not supported by this combined release process.
2. Push the settled batch to `main` once. Reuse its automatic
   [CI](https://github.com/symbiogenesis/DotJpegli/actions/workflows/ci.yml)
   run; do not dispatch duplicate CI. The **Release NuGet package** job builds
   all three targets after tests and deployment checks, verifies package
   contents and repository provenance, and retains `nuget-<commit>` for 30 days.
   The **NativeAOT CLI** jobs build and smoke-test Windows x64 and ARM64
   executables, portable ZIPs, symbol ZIPs and MSI installers; they retain
   `cli-win-x64-<commit>` and `cli-win-arm64-<commit>` for the same period.
3. After the entire CI run succeeds, run
   [Publish release](https://github.com/symbiogenesis/DotJpegli/actions/workflows/publish-nuget.yml)
   on **main**, entering the exact version. Leave **publish** unchecked for a
   preview. The run checks the latest push CI for the same commit, downloads
   its artifacts, verifies version/provenance/content and the complete download
   set, and creates `SHA256SUMS`. Preview artifacts are available as
   `verified-release-<commit>` on that workflow run. No files are rebuilt.
4. To publish, run it again on the same `main` revision with **publish** checked,
   then review and approve the `nuget` environment deployment. The publishing
   job downloads the same verified artifact and pushes it without rebuilding.
   After NuGet succeeds, the workflow creates or reuses a draft GitHub release
   tagged `v<version>` at the tested commit, uploads all downloads and the checksum
   manifest, then publishes it. Existing tags must point to that exact commit;
   an already published GitHub release blocks publication.
5. Confirm the version is listed at [nuget.org](https://www.nuget.org/packages/DotJpegli).
   NuGet validation and indexing can take time after upload succeeds.

This flow runs only from `main`; the workflow creates a release tag without a
second CI dispatch or a work-branch push. Preview runs need no NuGet account
configuration. The workflow filename remains `publish-nuget.yml` so existing
NuGet trusted-publishing policies continue to match. Existing embedded PDBs
and SDK Source Link support are retained, so a separate `.snupkg` is unnecessary.
Actions are pinned to immutable commits, publication jobs run serially, and
duplicate package versions fail instead of being silently skipped.

Each GitHub release contains the NuGet package, two standalone CLI EXEs, two
portable CLI ZIPs, two MSI installers, two symbol ZIPs and `SHA256SUMS`.
The EXE names include the release version and architecture; their bytes match
the executables in the portable ZIPs and installers. See the [CLI guide](cli.md) for
installation and signing status.

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
- If only the GitHub release job failed after NuGet succeeded, use **Re-run
  failed jobs** on that publication run. This retries uploading the same verified
  files without republishing NuGet. Do not start another publication run for the
  same version. A draft is completed only after every asset has been uploaded.
- OIDC failures usually indicate a mismatch in workflow filename, environment,
  `NUGET_USER`, policy owner or allowed package scope. Check the trusted policy
  instead of creating a permanent API key.

Local package verification (build check, no tests or benchmarks):

```powershell
dotnet restore src/DotJpegli/DotJpegli.csproj --locked-mode
dotnet pack src/DotJpegli/DotJpegli.csproj -c Release --no-restore --output <ABSOLUTE-TEMP-DIRECTORY> -p:ContinuousIntegrationBuild=true -p:RepositoryBranch=refs/heads/main
./tools/verify-release.ps1 -PackagePath <ABSOLUTE-PACKAGE-PATH> -ExpectedVersion 1.0.0 -ExpectedRevision <FULL-COMMIT-SHA>
```
