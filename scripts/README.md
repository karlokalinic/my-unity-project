# Production WebGL deployment

The canonical public build is produced only from `karlokalinic/my-unity-project`.

Unity Build Automation builds `main` as WebGL. `Assets/Editor/CloudflareWebGLPostBuild.cs` then runs `scripts/uba-postbuild-cloudflare.sh`, which stages the generated Unity player, rejects the legacy browser smoke-test fingerprint, enforces the Cloudflare static-asset file limit, deploys Worker `unitylaptop`, and verifies `unitylaptop-build.json` against the exact source revision.

The public URL is `https://unitylaptop.karlolegend.workers.dev`.

The legacy repository `karlokalinic/UNITYLAPTOP` may host its browser smoke-test only under a different Worker name and must never overwrite `unitylaptop`.
