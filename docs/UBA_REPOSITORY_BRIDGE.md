# Unity Build Automation repository bridge

Canonical gameplay/source remains `karlokalinic/my-unity-project` on `main`.

The Unity Build Automation SCM integration that produced the September build history is attached to the private repository `karlokalinic/UNITYLAPTOP`, branch `tooling/unity-cloud-devops`. That private branch is therefore treated as a machine-owned Build Automation transport, not as an independently authored gameplay branch.

`karlokalinic/UNITYLAPTOP/.github/workflows/canonical-production-bridge.yml` periodically reads canonical `my-unity-project/main` and publishes a parentless snapshot commit to the private UBA branch. The snapshot contains the canonical project plus `.uba-build-request.json`; `.github/workflows/*` is intentionally omitted because GitHub Apps cannot rewrite workflow files cross-repository without the workflows permission and Unity Build Automation does not consume GitHub workflow definitions.

The marker records the canonical source SHA. Production post-build validation accepts only the marker plus the intentional workflow omission as bridge-only differences. All Unity/runtime/build/deployment source must otherwise match canonical `my-unity-project/main`.

The public repository must not maintain a second same-named UBA mirror branch as a trigger mechanism. A push to `my-unity-project/tooling/unity-cloud-devops` is not a Build Automation request when the Unity project is connected to `karlokalinic/UNITYLAPTOP`.

Build truth remains external: a successful bridge push proves only that the exact canonical Unity project reached the SCM branch consumed by Build Automation. A Unity build is not claimed until Build Automation creates and completes a real build.