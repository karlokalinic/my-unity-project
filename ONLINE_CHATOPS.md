# UnityLaptop Online ChatOps

`karlokalinic/my-unity-project` is the canonical gameplay/source repository. `main` is the only source-of-truth branch.

Production path:

`GitHub main -> material trigger commit on tooling/unity-cloud-devops -> Unity Build Automation UNITYLAPTOP-WebGL-DEV -> Unity WebGL player -> optional Cloudflare production deploy -> public revision verification`

Public target: `https://unitylaptop.karlolegend.workers.dev`

The legacy repository `karlokalinic/UNITYLAPTOP` contains the old browser-native smoke implementation and must never overwrite production Unity output.

## Unity Build Automation configuration

Known configuration:

- Unity organization ID: `6872486406979`
- Unity project ID: `c5f8ee3c-ba65-4e4a-8174-2fc09f6e9b52`
- build target/configuration: `UNITYLAPTOP-WebGL-DEV`
- platform: WebGL
- configured branch: `tooling/unity-cloud-devops`
- project subfolder: repository root
- Unity version: auto-detect from `ProjectSettings/ProjectVersion.txt` (`6000.4.0f1`)
- builder: macOS Sequoia, Apple Silicon, STANDARD 4 vCPU / 16 GB

Do not infer that Auto-build is functioning from GitHub activity alone. On 2026-09-14 the Unity Dashboard still showed only builds from 9 days earlier after a new empty mirror commit had been pushed. That proved the previous empty-commit trigger was not sufficient evidence of an actual UBA build attempt.

## Material trigger contract

`.github/workflows/sync-unity-build-automation.yml` now checks out canonical `main`, writes `.uba-build-request.json`, commits that real file change, and force-publishes the result to `tooling/unity-cloud-devops`.

The trigger file contains the canonical `main` SHA, request timestamp, and GitHub run ID. It exists only on the machine-owned UBA branch. No gameplay/content change is authored there.

Every requested build therefore changes repository content on the exact branch Unity watches. Empty commits and branch repoints are not treated as build requests anymore.

The mirror commit may differ from canonical `main` only by `.uba-build-request.json`. `scripts/uba-postbuild-cloudflare.sh` fetches current `main`, verifies that the only diff is this trigger file, verifies its `canonicalMain` value, and records canonical `main` as `sourceRevision` while keeping the UBA trigger commit as `buildRevision`.

## Trigger truth

A GitHub mirror workflow success means only that a material SCM change was published to the configured UBA branch. It is not a Unity build success.

A Unity build is considered started only when Unity Build Automation creates a new build-history attempt. A build is considered passed only when that attempt finishes successfully.

If the material trigger still does not create a UBA build-history entry, the remaining fault is Unity-side source-control/Auto-build configuration or the Build Automation trigger service. At that point the Build Automation REST API must be used directly. The official API endpoint family is `https://build-api.cloud.unity3d.com/api/v1/` and the API key must be kept in a protected secret store, never committed to this public repository.

## Production deployment

`Assets/Editor/CloudflareWebGLPostBuild.cs` runs only on Build Automation WebGL builders and accepts canonical `main` or `tooling/unity-cloud-devops`.

The Build Automation environment may provide:

- `CLOUDFLARE_API_TOKEN`
- `CLOUDFLARE_ACCOUNT_ID`

If either is absent, the Unity WebGL build is allowed to complete as a build-only result and Cloudflare deployment is skipped.

When credentials are present, `scripts/uba-postbuild-cloudflare.sh`:

1. accepts only a Unity-generated WebGL player;
2. rejects the known legacy browser fallback;
3. proves the UBA mirror differs from current canonical `main` only by `.uba-build-request.json`;
4. validates the trigger file's canonical SHA;
5. stages only the current Unity output;
6. enforces Cloudflare's 25 MiB per-file limit;
7. writes `unitylaptop-build.json` with canonical source revision, UBA build revision, build number and timestamp;
8. deploys to Worker `unitylaptop` and verifies the exact public canonical revision.

## Build safety

`Assets/Editor/CloudBuildGuard.cs` blocks invalid enabled scenes, missing scripts, invalid player Humanoid binding, and invalid Rake/Snowman imports.

`.github/workflows/validate-unity-build-prereqs.yml` guards the conditions that invalidated the stale September 5 build: Unity must be `6000.4.0f1` and `com.unity.modules.physics` must remain enabled.

GitHub validation is not a substitute for Unity compilation.

## Deployment truth

A release is complete only at:

`SOURCE UPDATED -> MATERIAL UBA TRIGGER PUBLISHED -> UNITY BUILD ATTEMPT CREATED -> UNITY BUILD PASSED -> CLOUDFLARE DEPLOY PASSED -> PUBLIC REVISION VERIFIED`

If any stage cannot be observed directly, report only the highest verified stage.
