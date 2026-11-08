# Publishing releases

[Documentation](README.md)

Releases use [NuGet trusted publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing): GitHub OIDC exchanges the workflow identity for a temporary NuGet API key inside the protected `nuget` environment, so no long-lived API key is stored.

## One-time account setup

1. In [GitHub environments](https://github.com/symbiogenesis/DotJpegli/settings/environments), create an environment named `nuget`:
   - Restrict deployment branches to `main` only (no tags).
   - Add a required reviewer (enable self-review for a sole maintainer) and disable administrator bypass.
   - Add environment variable `NUGET_USER` set to the nuget.org profile username (not email address).
2. In [nuget.org trusted publishing](https://www.nuget.org/account/trustedpublishing), create a policy for the package owner:

| Field | Value |
| --- | --- |
| Repository owner | `symbiogenesis` |
| Repository | `DotJpegli` |
| Workflow file | `publish-nuget.yml` (filename only) |
| Environment | `nuget` |
| Package scope | `DotJpegli` |
| Allowed operations | Publish new versions |

## Publish a version

The root `README.md` is the source for both GitHub and NuGet (packing strips the centering HTML wrapper automatically).

1. **Bump version**: Set `<Version>` in `src/DotJpegli/DotJpegli.csproj` to a stable three-part version (for example, `5.0.0`) and commit to `main`. The NuGet package, CLI executables, and installers share this version (major/minor `<= 255`, patch `<= 65535`, no prerelease labels).
2. **Wait for push CI**: Push `main` once and wait for the automatic [CI](https://github.com/symbiogenesis/DotJpegli/actions/workflows/ci.yml) run to succeed. It retains `nuget-<commit>` and the five `cli-<rid>-<commit>` artifacts for 30 days.
3. **Preview**: Run [Publish release](https://github.com/symbiogenesis/DotJpegli/actions/workflows/publish-nuget.yml) on `main` with the version number and **publish** unchecked. The workflow downloads the CI artifacts for that commit, verifies contents and provenance without rebuilding, generates `SHA256SUMS`, and uploads `verified-release-<commit>`.
4. **Publish**: Run [Publish release](https://github.com/symbiogenesis/DotJpegli/actions/workflows/publish-nuget.yml) on the same `main` commit with **publish** checked and approve the `nuget` environment deployment. The workflow pushes the verified NuGet package, tags `v<version>`, and publishes the GitHub release with all 24 downloads plus `SHA256SUMS`.
5. **Confirm**: Verify that the new version is indexed on [nuget.org](https://www.nuget.org/packages/DotJpegli).

## Retries and failed checks

- **Failed CI**: Fix the issue or rerun the existing CI run before publishing.
- **Expired artifacts**: If the 30-day artifact retention expires, rerun the CI workflow (if within 30 days) or push a new commit on `main`.
- **Partial publication**: NuGet versions are immutable. If NuGet upload succeeded but the GitHub release step failed, click **Re-run failed jobs** on that publication workflow run so it retries the GitHub release upload without re-pushing to NuGet.
- **OIDC errors**: Check that the workflow filename (`publish-nuget.yml`), environment (`nuget`), `NUGET_USER`, and package owner match the nuget.org trusted policy.

### Local package verification

To verify package structure and metadata locally without running tests or benchmarks:

```powershell
dotnet restore src/DotJpegli/DotJpegli.csproj --locked-mode
dotnet pack src/DotJpegli/DotJpegli.csproj -c Release --no-restore --output <ABSOLUTE-TEMP-DIRECTORY> -p:ContinuousIntegrationBuild=true -p:RepositoryBranch=refs/heads/main
./tools/verify-release.ps1 -PackagePath <ABSOLUTE-PACKAGE-PATH> -ExpectedVersion 5.0.0 -ExpectedRevision <FULL-COMMIT-SHA>
```
