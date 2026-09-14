# UnityLaptop Online ChatOps

`karlokalinic/my-unity-project` on `main` is the canonical gameplay/source repository. The private repository `karlokalinic/UNITYLAPTOP` is not a second gameplay source of truth; it is the SCM compatibility bridge historically connected to Unity Build Automation plus legacy smoke/deployment tooling.

Current remote production path:

`my-unity-project/main -> private UNITYLAPTOP/tooling/unity-cloud-devops snapshot -> Unity Build Automation -> Unity WebGL -> Cloudflare Worker unitylaptop -> exact public revision verification`

Public target: `https://unitylaptop.karlolegend.workers.dev`

## Why the private repository exists in the build path

The September Build Automation configuration/history was created against `karlokalinic/UNITYLAPTOP`, branch `tooling/unity-cloud-devops`. Pushing a same-named branch inside `karlokalinic/my-unity-project` therefore does not prove that Unity received a new SCM change.

The private bridge is now explicit. `karlokalinic/UNITYLAPTOP/.github/workflows/canonical-production-bridge.yml` runs every five minutes and on explicit dispatch. It reads the current canonical `my-unity-project/main` revision and publishes it to `UNITYLAPTOP/tooling/unity-cloud-devops` as a machine-owned snapshot.

The snapshot is parentless and contains the current canonical project plus `.uba-build-request.json`. `.github/workflows/*` is intentionally omitted because the repository-scoped GitHub App token cannot rewrite workflow definitions cross-repository and Unity Build Automation does not consume GitHub workflow files. Every other canonical path must match.

The marker records:

- canonical repository;
- canonical `main` SHA;
- request timestamp;
- bridge workflow run ID;
- private bridge repository;
- UBA source branch.

A successful bridge run proves only that the canonical Unity project reached the private SCM branch used by Build Automation. It is not evidence that Unity created or passed a build.

## Unity Build Automation configuration

Known project identifiers from the established configuration:

- Unity organization ID: `6872486406979`
- Unity project ID: `c5f8ee3c-ba65-4e4a-8174-2fc09f6e9b52`
- historical build configuration name: `UNITYLAPTOP-WebGL-DEV`
- platform: WebGL
- SCM repository: `karlokalinic/UNITYLAPTOP`
- SCM branch: `tooling/unity-cloud-devops`
- Unity project subfolder: repository root
- canonical Unity version carried by the bridge: `6000.4.0f1`

Do not infer Auto-build state from GitHub activity. A real Unity build is considered started only when Build Automation creates a new build-history attempt.

The previously added public-repository mirror/API workflows were removed after the repository mismatch was proven. The public repository no longer creates a misleading same-named UBA branch or a guaranteed-red direct API workflow when no Build Automation API credential exists there.

## Production post-build contract

`Assets/Editor/CloudflareWebGLPostBuild.cs` runs only on Build Automation WebGL builders and accepts `main` or `tooling/unity-cloud-devops` as approved build branches.

For private bridge builds, `scripts/uba-postbuild-cloudflare.sh` fetches current canonical `my-unity-project/main`, reads `.uba-build-request.json`, and verifies that all differences are limited to the bridge-owned marker plus the intentionally omitted `.github/workflows/*` paths. Any gameplay, asset, package, project-setting, deployment-script, scene, or other unexpected difference blocks production deployment.

When Build Automation exposes `CLOUDFLARE_API_TOKEN` and `CLOUDFLARE_ACCOUNT_ID`, the post-build path:

1. accepts only a Unity-generated WebGL player;
2. rejects the legacy browser fallback;
3. proves the bridge marker names the exact current canonical SHA;
4. rejects unexpected source differences;
5. stages only the current Unity output;
6. enforces Cloudflare's 25 MiB per-file static-asset limit;
7. writes `unitylaptop-build.json` with canonical source revision, UBA build revision, build number, and timestamp;
8. deploys to Worker `unitylaptop`;
9. succeeds only when the public Worker serves that exact canonical revision.

If Cloudflare credentials are absent, deployment cannot be claimed. GitHub source validation is never substituted for a Unity compile or a public deploy.

## Build safety

`Assets/Editor/CloudBuildGuard.cs` blocks invalid enabled scenes, missing scripts, invalid player Humanoid binding, and invalid Rake/Snowman imports.

`.github/workflows/validate-unity-build-prereqs.yml` guards Unity `6000.4.0f1` and `com.unity.modules.physics` in canonical source. Other source/asset validators remain GitHub-side preflight gates only.

## Deployment truth

A release is complete only at:

`SOURCE UPDATED -> PRIVATE UBA SCM SNAPSHOT PUBLISHED -> UNITY BUILD ATTEMPT CREATED -> UNITY BUILD PASSED -> CLOUDFLARE DEPLOY PASSED -> PUBLIC REVISION VERIFIED`

If any stage cannot be directly observed, report only the highest verified stage.
