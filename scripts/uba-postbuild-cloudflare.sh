#!/usr/bin/env bash
set -euo pipefail

log() { printf '[UNITYLAPTOP][UBA->CF] %s\n' "$*"; }
fail() { printf '::error::[UNITYLAPTOP][UBA->CF] %s\n' "$*" >&2; exit 1; }

PROJECT_ROOT="${PROJECT_DIRECTORY:-${PROJECT_PATH:-${WORKSPACE:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}}}"
PLAYER_PATH="${UNITY_PLAYER_PATH:-${OUTPUT_DIRECTORY:-}}"
REVISION="${BUILD_REVISION:-${SCM_REVISION:-${GIT_COMMIT:-unknown}}}"
BUILD_NO="${UCB_BUILD_NUMBER:-${BUILD_NUMBER:-unknown}}"
PUBLIC_URL="${CF_PUBLIC_URL:-https://unitylaptop.karlolegend.workers.dev}"

[[ -n "${PLAYER_PATH}" ]] || fail "Unity WebGL artifact path is unavailable (UNITY_PLAYER_PATH/OUTPUT_DIRECTORY unset)."
[[ -d "${PLAYER_PATH}" ]] || fail "Unity WebGL artifact path is not a directory: ${PLAYER_PATH}"
[[ -f "${PLAYER_PATH}/index.html" ]] || fail "Unity WebGL index.html is missing under ${PLAYER_PATH}."
[[ -n "${CLOUDFLARE_API_TOKEN:-}" ]] || fail "Missing CLOUDFLARE_API_TOKEN in Build Automation environment."
[[ -n "${CLOUDFLARE_ACCOUNT_ID:-}" ]] || fail "Missing CLOUDFLARE_ACCOUNT_ID in Build Automation environment."
[[ -f "${PROJECT_ROOT}/cloudflare/wrangler.uba.toml" ]] || fail "Missing canonical Cloudflare config under ${PROJECT_ROOT}/cloudflare."

if grep -Fq 'app.js?v=20260905-physics1' "${PLAYER_PATH}/index.html" || grep -Fq '<div class="brand">UNITYLAPTOP <span>WEBGL</span></div>' "${PLAYER_PATH}/index.html"; then
  fail "Refusing deployment: artifact is the legacy browser smoke fallback, not a Unity-generated WebGL player."
fi
if ! grep -Eq 'createUnityInstance|\.loader\.js' "${PLAYER_PATH}/index.html"; then
  fail "Refusing deployment: index.html does not have a Unity WebGL loader fingerprint."
fi

STAGING_DIR="${PROJECT_ROOT}/.uba-cloudflare-webgl"
rm -rf "${STAGING_DIR}"
mkdir -p "${STAGING_DIR}"
cp -R "${PLAYER_PATH}/." "${STAGING_DIR}/"

# Cloudflare Workers Static Assets currently enforce a 25 MiB limit per file.
# Fail before Wrangler so a stale public version can never be mistaken for a successful deploy.
OVERSIZED="$(find "${STAGING_DIR}" -type f -size +26214400c -print -quit)"
if [[ -n "${OVERSIZED}" ]]; then
  SIZE="$(wc -c < "${OVERSIZED}" | tr -d ' ')"
  fail "Static asset exceeds Cloudflare's 25 MiB per-file limit: ${OVERSIZED} (${SIZE} bytes). Split/compress the Unity payload before production deployment."
fi

python3 - "${STAGING_DIR}/unitylaptop-build.json" "${REVISION}" "${BUILD_NO}" <<'PY'
import json
import pathlib
import sys
from datetime import datetime, timezone
path = pathlib.Path(sys.argv[1])
payload = {
    "sourceRepository": "karlokalinic/my-unity-project",
    "sourceRevision": sys.argv[2],
    "buildNumber": sys.argv[3],
    "builtAtUtc": datetime.now(timezone.utc).isoformat(),
    "artifact": "Unity WebGL",
}
path.write_text(json.dumps(payload, separators=(",", ":")), encoding="utf-8")
PY

FILE_COUNT="$(find "${STAGING_DIR}" -type f | wc -l | tr -d ' ')"
SIZE_BYTES="$(du -sk "${STAGING_DIR}" | awk '{print $1 * 1024}')"
log "Validated Unity WebGL staging: files=${FILE_COUNT} bytes=${SIZE_BYTES} revision=${REVISION} build=${BUILD_NO}."

if ! command -v node >/dev/null 2>&1 || ! command -v npm >/dev/null 2>&1; then
  [[ -s "${HOME}/.nvm/nvm.sh" ]] || fail "Node/npm unavailable and NVM was not found on the Build Automation agent."
  # shellcheck disable=SC1090
  source "${HOME}/.nvm/nvm.sh"
  nvm install 22
  nvm use 22
fi

cd "${PROJECT_ROOT}"
npm install --no-audit --no-fund
npx wrangler --version
log "Deploying canonical Unity WebGL revision ${REVISION} to Cloudflare Worker 'unitylaptop'."
npx wrangler deploy --config cloudflare/wrangler.uba.toml

EXPECTED_REVISION="${REVISION}"
for attempt in $(seq 1 18); do
  MARKER="$(curl --fail --silent --show-error --location --max-time 20 "${PUBLIC_URL}/unitylaptop-build.json" 2>/dev/null || true)"
  INDEX="$(curl --fail --silent --show-error --location --max-time 20 "${PUBLIC_URL}/" 2>/dev/null || true)"
  if [[ -n "${MARKER}" ]] && [[ -n "${INDEX}" ]] && \
     [[ "${MARKER}" == *'"sourceRepository":"karlokalinic/my-unity-project"'* ]] && \
     [[ "${MARKER}" == *"\"sourceRevision\":\"${EXPECTED_REVISION}\""* ]] && \
     [[ "${INDEX}" != *'app.js?v=20260905-physics1'* ]] && \
     { [[ "${INDEX}" == *'createUnityInstance'* ]] || [[ "${INDEX}" == *'.loader.js'* ]]; }; then
    log "CLOUDFLARE_DEPLOY_OK url=${PUBLIC_URL} build=${BUILD_NO} revision=${REVISION}"
    exit 0
  fi
  log "Public revision verification ${attempt}/18 not ready; sleeping 5s."
  sleep 5
done

fail "Cloudflare deploy returned but public Worker did not serve the exact Unity revision ${REVISION}."
