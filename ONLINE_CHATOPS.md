# UnityLaptop Online ChatOps

`karlokalinic/my-unity-project` is the only canonical gameplay/source repository. `main` is the only branch humans and engineering agents should treat as source of truth.

The current production path is:

`GitHub main -> exact mirror tooling/unity-cloud-devops -> Unity Build Automation UNITYLAPTOP-WebGL-DEV -> Unity WebGL player -> Cloudflare Worker unitylaptop -> public revision verification`

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

The Build Automation branch is intentionally a machine-owned mirror. `.github/workflows/sync-unity-build-automation.yml` force-updates `tooling/unity-cloud-devops` to the exact `main` commit on every canonical push. No gameplay changes should be authored directly on the mirror branch.

This keeps GitHub `main` canonical while matching the already-configured Unity Dashboard target without requiring a second source-of-truth branch or a local Unity installation.

## Trigger boundary

A canonical `main` push triggers the GitHub mirror workflow. The mirror workflow verifies that `tooling/unity-cloud-devops` resolves to exactly `${GITHUB_SHA}`. Unity Build Automation has auto-build enabled on that branch, so the mirror push is the normal cloud-build trigger.

The legacy Unity Build Automation API key is optional for normal operation and must never be committed. The repository does not require it to trigger routine builds because auto-build is driven by the mirrored Git ref. If an explicit API trigger is added later, store the credential only as an encrypted secret.

A GitHub push, successful source validation, or mirror update is not evidence that Unity compiled successfully. Production truth still requires a successful UBA build followed by exact public revision verification.

## Production deployment

`Assets/Editor/CloudflareWebGLPostBuild.cs` runs only on Build Automation WebGL builders and accepts either canonical `main` or the machine-owned `tooling/unity-cloud-devops` mirror. The shell deployer refuses a mirror build unless its `BUILD_REVISION` is identical to the current remote `main` revision.

The Build Automation environment must provide these production credentials as protected environment variables:

- `CLOUDFLARE_API_TOKEN`
- `CLOUDFLARE_ACCOUNT_ID`

Do not commit either value. If the Unity Dashboard shows no environment variables for `UNITYLAPTOP-WebGL-DEV`, the Unity player can still compile, but the production Cloudflare deploy is intentionally failed rather than falsely reporting success.

The deployer `scripts/uba-postbuild-cloudflare.sh`:

1. accepts only an actual Unity-generated WebGL player (`createUnityInstance` / `.loader.js` fingerprint);
2. rejects the known legacy browser fallback (`app.js?v=20260905-physics1`);
3. proves the UBA mirror revision is identical to remote canonical `main` before production deployment;
4. stages only the current Unity build output under `.uba-cloudflare-webgl`;
5. blocks deployment if a staged file exceeds Cloudflare Workers Static Assets' 25 MiB single-file limit;
6. writes `unitylaptop-build.json` containing repository, exact source revision, UBA build number and timestamp;
7. deploys only that staging directory to Cloudflare Worker `unitylaptop`;
8. repeatedly fetches the public Worker and succeeds only when both the Unity loader and exact revision marker are live.

`.github/workflows/verify-public-webgl.yml` independently waits for the current canonical `main` SHA. It passes only when the public Worker serves a genuine Unity WebGL entry point and `unitylaptop-build.json` identifies that exact commit.

## Build safety

`Assets/Editor/CloudBuildGuard.cs` blocks Unity builds with invalid enabled scenes, missing scripts, invalid player Humanoid import/runtime resource binding, or invalid Rake/Snowman imports.

`.github/workflows/validate-player-humanoid.yml`, `.github/workflows/validate-pitch-black-assets.yml`, and `.github/workflows/validate-production-deploy.yml` are source/contract gates. They do not substitute for an actual Unity compile, WebGL build or public deployment verification.

## Deployment truth

A release is complete only at:

`SOURCE UPDATED -> UBA MIRROR MATCHES MAIN -> UNITY BUILD PASSED -> CLOUDFLARE DEPLOY PASSED -> unitylaptop-build.json MATCHES MAIN -> PUBLIC UNITY ENTRY POINT VERIFIED`

If any stage cannot be directly observed, report only the highest verified stage.
