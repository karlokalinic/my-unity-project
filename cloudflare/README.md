# Cloudflare production target

`wrangler.uba.toml` is the only repository-owned configuration for the production Worker `unitylaptop`.

Its asset directory is temporary Build Automation staging (`.uba-cloudflare-webgl`), never a browser-authored fallback. The UBA post-build path writes an exact source-revision marker and verifies the public Worker before a production build can be treated as deployed.
