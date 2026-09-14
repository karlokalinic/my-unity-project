# UnityLaptop Online ChatOps

`karlokalinic/my-unity-project` is the only canonical gameplay/source repository. `main` is the only branch humans and engineering agents should treat as source of truth.

The current production path is:

`GitHub main -> machine build-request commit on tooling/unity-cloud-devops (same tree as main) -> Unity Build Automation UNITYLAPTOP-WebGL-DEV -> Unity WebGL player -> optional Cloudflare production deploy -> public revision verification`

Public target:

`https://unitylaptop.karlolegend.workers.dev`

The legacy repository `karlokalinic/UNITYLAPTOP` contains the old browser-native smoke implementation. It is not canonical gameplay source and its browser fallback must never overwrite the production Unity Worker.

## Actual Unity Build Automation configuration

The Unity Dashboard configuration currently used for this repository is:

- Unity organization ID: `6872486406979`
- Unity project ID: `c5f8ee3c-ba65-4e4a-8174-2fc09f6e9b52`
- build target/configuration: `UNITYLAPTOP-WebGL-DEV`
- platform: WebGL
- configured branch: `tooling/unity-cloud-devops`
- project subfolder: repository root
- Unity version: auto-detect from `ProjectSettings/ProjectVersion.txt` (`6000.4.0f1`)
- builder: macOS Sequoia, Apple Silicon, STANDARD 4 vCPU / 16 GB
- auto-build: enabled
- auto-cancel: enabled

The Build Automation branch is intentionally machine-owned. `.github/workflows/sync-unity-build-automation.yml` checks out canonical `main`, creates a unique empty build-request commit whose Git tree is byte-for-byte the canonical `main` tree, and force-publishes that commit to `tooling/unity-cloud-devops`. No gameplay changes are authored directly on the mirror branch.

A unique build-request commit is deliberate: Unity Build Automation must see a genuinely new commit on its configured branch for every canonical push or explicit workflow dispatch. Repointing the branch to an already-existing `main` commit is not treated as sufficient evidence that a new UBA build was requested.

`main` remains source of truth. The mirror commit is only a trigger envelope; its commit tree must equal canonical `main`. The production deployer independently fetches current `main`, compares Git tree IDs, and records the canonical `main` SHA as `sourceRevision`.

## Trigger boundary

A canonical `main` push triggers the GitHub mirror workflow. The workflow publishes a new build-request commit on `tooling/unity-cloud-devops` and logs `UBA_BUILD_REQUESTED` with both the canonical SHA and mirror SHA. Unity Build Automation has auto-build enabled on that branch.

Manual build requests can re-run the mirror workflow without changing gameplay source. Each run emits another unique empty build-request commit with the same canonical tree, giving UBA a new commit to build.

The Unity Build Automation API key is not required for this Git-trigger path and must never be committed. If an API trigger is introduced later, its credential belongs in an encrypted secret store, not repository content.

A GitHub push, successful source validation, or mirror build request is not evidence that Unity compiled successfully. Build truth requires a UBA build attempt/result.

## Production deployment

`Assets/Editor/CloudflareWebGLPostBuild.cs` runs only on Build Automation WebGL builders and accepts canonical `main` or the machine-owned `tooling/unity-cloud-devops` branch.

When UBA builds the mirror, `scripts/uba-postbuild-cloudflare.sh` proves that the build-request commit tree exactly matches current canonical `main`. The public marker records canonical `main` as `sourceRevision` and the UBA trigger commit separately as `buildRevision`.

The Build Automation environment may provide these production credentials as protected environment variables:

- `CLOUDFLARE_API_TOKEN`
- `CLOUDFLARE_ACCOUNT_ID`

Do not commit either value. If they are absent, the Unity WebGL build is allowed to complete and is explicitly reported as a build-only result; production Cloudflare deployment is skipped. Deployment and public verification remain incomplete until those credentials exist.

When credentials are available, the deployer `scripts/uba-postbuild-cloudflare.sh`:

1. accepts only an actual Unity-generated WebGL player (`createUnityInstance` / `.loader.js` fingerprint);
2. rejects the known legacy browser fallback (`app.js?v=20260905-physics1`);
3. proves the UBA build-request commit tree is identical to remote canonical `main`;
4. stages only the current Unity build output under `.uba-cloudflare-webgl`;
5. blocks deployment if a staged file exceeds Cloudflare Workers Static Assets' 25 MiB single-file limit;
6. writes `unitylaptop-build.json` containing repository, canonical source revision, UBA build revision, UBA build number and timestamp;
7. deploys only that staging directory to Cloudflare Worker `unitylaptop`;
8. repeatedly fetches the public Worker and succeeds only when both the Unity loader and canonical revision marker are live.

`.github/workflows/verify-public-webgl.yml` independently waits for the current canonical `main` SHA. It passes only when the public Worker serves a genuine Unity WebGL entry point and `unitylaptop-build.json` identifies that exact canonical commit.

## Build safety

`Assets/Editor/CloudBuildGuard.cs` blocks Unity builds with invalid enabled scenes, missing scripts, invalid player Humanoid import/runtime resource binding, or invalid Rake/Snowman imports.

`.github/workflows/validate-unity-build-prereqs.yml` permanently guards the two conditions that invalidated the stale September 5 build: the project must target Unity `6000.4.0f1`, and `com.unity.modules.physics` must remain enabled.

`.github/workflows/validate-player-humanoid.yml`, `.github/workflows/validate-pitch-black-assets.yml`, `.github/workflows/validate-unity-build-prereqs.yml`, and `.github/workflows/validate-production-deploy.yml` are source/contract gates. They do not substitute for an actual Unity compile, WebGL build or public deployment verification.

## Deployment truth

A release is complete only at:

`SOURCE UPDATED -> UBA BUILD-REQUEST COMMIT PUBLISHED -> UNITY BUILD PASSED -> CLOUDFLARE DEPLOY PASSED -> unitylaptop-build.json MATCHES MAIN -> PUBLIC UNITY ENTRY POINT VERIFIED`

If Cloudflare credentials are absent, the highest possible result is `UNITY BUILD PASSED (BUILD-ONLY)`.

If any stage cannot be directly observed, report only the highest verified stage.
