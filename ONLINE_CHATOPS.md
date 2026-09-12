# UnityLaptop Online ChatOps

`karlokalinic/my-unity-project` is the only canonical gameplay/source repository.

The production path is:

`GitHub main -> Unity Build Automation UnityLaptop-WebGL -> Unity WebGL player -> Cloudflare Worker unitylaptop -> public revision verification`

Public target:

`https://unitylaptop.karlolegend.workers.dev`

The legacy private repository `karlokalinic/UNITYLAPTOP` contains an old browser-native smoke mirror. It is not canonical gameplay source and its browser fallback must use Worker `unitylaptop-smoke`; it must never deploy browser `web/` assets to the production Worker `unitylaptop`.

## Unity Build Automation configuration

Production target:

- repository: `karlokalinic/my-unity-project`
- configuration: `UnityLaptop-WebGL`
- branch: `main`
- project subdirectory: repository root
- Unity version: auto-detect from `ProjectSettings/ProjectVersion.txt` (`6000.4.0f1`)
- platform: WebGL
- auto-build: enabled
- automatic build sharing may remain enabled, but Unity sharing is not the canonical public URL
- builder must expose the standard Build Automation variables such as `IS_BUILDER`, `SCM_BRANCH`, `BUILD_REVISION`, `UCB_BUILD_NUMBER`, `PROJECT_DIRECTORY`, and the generated WebGL output
- production environment must provide `CLOUDFLARE_API_TOKEN` and `CLOUDFLARE_ACCOUNT_ID`

`Assets/Editor/CloudflareWebGLPostBuild.cs` is repository-owned deployment integration. On a Unity Build Automation WebGL build of `main`, it launches `scripts/uba-postbuild-cloudflare.sh`. Local/editor builds and non-main UBA branches do not deploy production.

The shell deployer:

1. accepts only an actual Unity-generated WebGL player (`createUnityInstance` / `.loader.js` fingerprint);
2. explicitly rejects the known legacy browser fallback fingerprint (`app.js?v=20260905-physics1`);
3. stages only the current Unity build output under `.uba-cloudflare-webgl`;
4. blocks deployment if any single staged file exceeds Cloudflare Workers Static Assets' 25 MiB file limit;
5. writes `unitylaptop-build.json` containing canonical repository, exact source revision, UBA build number and build timestamp;
6. deploys the staging directory to Cloudflare Worker `unitylaptop`;
7. repeatedly fetches the public Worker and succeeds only when both the Unity loader fingerprint and the exact revision marker are live.

A production `main` UBA build is not considered successfully deployed if Cloudflare credentials are absent, Wrangler fails, the legacy fallback is encountered, a static asset violates the hosting limit, or the public revision marker does not match the build revision.

## Build safety

`Assets/Editor/CloudBuildGuard.cs` blocks Unity builds with invalid enabled scenes, missing scripts, invalid player Humanoid import/runtime resource binding, or invalid Rake/Snowman imports.

`.github/workflows/validate-player-humanoid.yml`, `.github/workflows/validate-pitch-black-assets.yml`, and `.github/workflows/validate-production-deploy.yml` are source/contract gates. They do not substitute for an actual Unity compile, WebGL build or public deployment verification.

## Deployment truth

A release is complete only at:

`SOURCE UPDATED -> UNITY BUILD PASSED -> CLOUDFLARE DEPLOY PASSED -> unitylaptop-build.json MATCHES SOURCE REVISION -> PUBLIC UNITY ENTRY POINT VERIFIED`

If any stage cannot be directly observed, report only the highest verified stage. Never treat the legacy browser smoke mirror as a Unity WebGL build.
