# UnityLaptop Online ChatOps

`karlokalinic/my-unity-project` is the only canonical gameplay/source repository.

The intended production path is:

`GitHub main -> Unity Build Automation UnityLaptop-WebGL -> Unity WebGL player -> Cloudflare Worker unitylaptop -> public revision verification`

Public target:

`https://unitylaptop.karlolegend.workers.dev`

The legacy private repository `karlokalinic/UNITYLAPTOP` contains an old browser-native smoke mirror. It is not canonical gameplay source and its browser fallback must use Worker `unitylaptop-smoke`; it must never deploy browser `web/` assets to the production Worker `unitylaptop`.

Do not duplicate the production Unity compile with a GitHub Actions/GameCI workflow unless that runner has a complete supported Unity activation boundary. `UNITY_EMAIL` + `UNITY_PASSWORD` alone are not a valid Unity Personal CI activation path; GameCI Personal builds require a usable `UNITY_LICENSE` (`.ulf`), while serial licensing requires `UNITY_SERIAL` plus account credentials. Unity Build Automation is the preferred license-bearing production builder once its account-bound configuration is actually present.

## Unity Build Automation configuration

The values below are the required production contract. Repository documentation is not proof that the external Unity Dashboard target exists, is authenticated, or has auto-build enabled. Those facts must be verified from an actual Build Automation record or from the exact public revision marker.

Required production target:

- repository: `karlokalinic/my-unity-project`
- configuration: `UnityLaptop-WebGL`
- branch: `main`
- project subdirectory: repository root
- Unity version: auto-detect from `ProjectSettings/ProjectVersion.txt` (`6000.4.0f1`)
- platform: WebGL
- auto-build: required
- automatic build sharing may remain enabled, but Unity sharing is not the canonical public URL
- builder must expose the standard Build Automation variables such as `IS_BUILDER`, `SCM_BRANCH`, `BUILD_REVISION`, `UCB_BUILD_NUMBER`, `PROJECT_DIRECTORY`, and the generated WebGL output
- production environment must provide `CLOUDFLARE_API_TOKEN` and `CLOUDFLARE_ACCOUNT_ID`

### Production trigger boundary

The normal production trigger is a commit to canonical `main`; an enabled `UnityLaptop-WebGL` configuration with auto-build enabled must detect that commit and build it without a local Unity installation.

An optional explicit API trigger may be added through a Unity Build Automation service account. When GitHub is used for that purpose, standardize the repository secrets as `UNITY_SERVICE_ACCOUNT_KEY_ID` and `UNITY_SERVICE_ACCOUNT_SECRET_KEY`. The service account must have permission to read/trigger Build Automation for Unity project `c0f22441-2e8f-4181-b06c-e28a92df0ce6`. Never commit those credentials or a Unity bearer token.

A GitHub push, successful source validation, or the existence of this documentation is not evidence that UBA ran. The production truth remains the actual UBA build record followed by the exact public revision marker.

`Assets/Editor/CloudflareWebGLPostBuild.cs` is repository-owned deployment integration. On an authenticated Unity Build Automation WebGL build of `main`, it launches `scripts/uba-postbuild-cloudflare.sh`. Local/editor builds and non-main UBA branches do not deploy production.

The shell deployer:

1. accepts only an actual Unity-generated WebGL player (`createUnityInstance` / `.loader.js` fingerprint);
2. explicitly rejects the known legacy browser fallback fingerprint (`app.js?v=20260905-physics1`);
3. stages only the current Unity build output under `.uba-cloudflare-webgl`;
4. blocks deployment if any single staged file exceeds Cloudflare Workers Static Assets' 25 MiB file limit;
5. writes `unitylaptop-build.json` containing canonical repository, exact source revision, UBA build number and build timestamp;
6. deploys the staging directory to Cloudflare Worker `unitylaptop`;
7. repeatedly fetches the public Worker and succeeds only when both the Unity loader fingerprint and the exact revision marker are live.

A production `main` UBA build is not considered successfully deployed if the Build Automation target is absent/unlinked, Cloudflare credentials are absent, Wrangler fails, the legacy fallback is encountered, a static asset violates the hosting limit, or the public revision marker does not match the build revision.

`.github/workflows/verify-public-webgl.yml` is the independent production truth probe. It may only pass when the public Worker serves a genuine Unity WebGL entry point and `unitylaptop-build.json` identifies the exact current canonical commit.

## Build safety

`Assets/Editor/CloudBuildGuard.cs` blocks Unity builds with invalid enabled scenes, missing scripts, invalid player Humanoid import/runtime resource binding, or invalid Rake/Snowman imports.

`.github/workflows/validate-player-humanoid.yml`, `.github/workflows/validate-pitch-black-assets.yml`, and `.github/workflows/validate-production-deploy.yml` are source/contract gates. They do not substitute for an actual Unity compile, WebGL build or public deployment verification.

## Deployment truth

A release is complete only at:

`SOURCE UPDATED -> UNITY BUILD PASSED -> CLOUDFLARE DEPLOY PASSED -> unitylaptop-build.json MATCHES SOURCE REVISION -> PUBLIC UNITY ENTRY POINT VERIFIED`

If any stage cannot be directly observed, report only the highest verified stage. Never treat the legacy browser smoke mirror as a Unity WebGL build.
