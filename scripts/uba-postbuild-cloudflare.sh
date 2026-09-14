#!/usr/bin/env bash
set -euo pipefail

log() { printf '[UNITYLAPTOP][UBA->CF] %s\n' "$*"; }
fail() { printf '::error::[UNITYLAPTOP][UBA->CF] %s\n' "$*" >&2; exit 1; }

PROJECT_ROOT="${PROJECT_DIRECTORY:-${PROJECT_PATH:-${WORKSPACE:-$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)}}}"
PLAYER_PATH="${UNITY_PLAYER_PATH:-${OUTPUT_DIRECTORY:-}}"
REVISION="${BUILD_REVISION:-${SCM_REVISION:-${GIT_COMMIT:-unknown}}}"
BUILD_NO="${UCB_BUILD_NUMBER:-${BUILD_NUMBER:-unknown}}"
BRANCH="${SCM_BRANCH:-${GIT_BRANCH:-unknown}}"
PUBLIC_URL="${CF_PUBLIC_URL:-https://unitylaptop.karlolegend.workers.dev}"
CANONICAL_BRANCH="main"
UBA_MIRROR_BRANCH="tooling/unity-cloud-devops"
CANONICAL_GIT_URL="https://github.com/karlokalinic/my-unity-project.git"
SOURCE_REVISION="${REVISION}"

[[ -n "${PLAYER_PATH}" ]] || fail "Unity WebGL artifact path is unavailable (UNITY_PLAYER_PATH/OUTPUT_DIRECTORY unset)."
[[ -d "${PLAYER_PATH}" ]] || fail "Unity WebGL artifact path is not a directory: ${PLAYER_PATH}"
[[ -f "${PLAYER_PATH}/index.html" ]] || fail "Unity WebGL index.html is missing under ${PLAYER_PATH}."
[[ "${REVISION}" != "unknown" ]] || fail "Build Automation did not expose BUILD_REVISION/SCM_REVISION/GIT_COMMIT; revision-safe deployment is unavailable."
[[ -n "${CLOUDFLARE_API_TOKEN:-}" ]] || fail "Missing CLOUDFLARE_API_TOKEN in Build Automation environment."
[[ -n "${CLOUDFLARE_ACCOUNT_ID:-}" ]] || fail "Missing CLOUDFLARE_ACCOUNT_ID in Build Automation environment."
[[ -f "${PROJECT_ROOT}/cloudflare/wrangler.uba.toml" ]] || fail "Missing canonical Cloudflare config under ${PROJECT_ROOT}/cloudflare."

if [[ "${BRANCH}" == "${UBA_MIRROR_BRANCH}" ]]; then
  CANONICAL_REVISION="$(git ls-remote "${CANONICAL_GIT_URL}" "refs/heads/${CANONICAL_BRANCH}" | awk 'NR == 1 { print $1}')"
  [[ -n "${CANONICAL_REVISION}" ]] || fail "Unable to resolve canonical ${CANONICAL_BRANCH} revision before production deploy."

  HEAD_REVISION="$(git -C "${PROJECT_ROOT}" rev-parse HEAD)"
  [[ "${HEAD_REVISION}" == "${REVISION}" ]] || fail "UBA revision mismatch: environment=${REVISION} checkout=${HEAD_REVISION}."
  [[ -f "${PROJECT_ROOT}/.uba-build-request.json" ]] || fail "UBA material trigger file is missing from mirror build."

  git -C "${PROJECT_ROOT}" fetch --quiet --no-tags --depth=1 "${CANONICAL_GIT_URL}" "${CANONICAL_REVISION}"
  CHANGED_FILES="$(git -C "${PROJECT_ROOT}" diff --name-only FETCH_HEAD HEAD)"
  [[ "${CHANGED_FILES}" == ".uba-build-request.json" ]] || fail "UBA mirror differs from canonical main outside the approved trigger file: ${CHANGED_FILES}"

  TRIGGER_CANONICAL="$(python3 - "${PROJECT_ROOT}/.uba-build-request.json" <<'PY'
import json
import pathlib
import sys
path = pathlib.Path(sys.argv[1])
data = json.loads(path.read_text(encoding='utf-8'))
print(data.get('canonicalMain', ''))
PY
)"
  [[ "${TRIGGER_CANONICAL}" == "${CANONICAL_REVISION}" ]] || fail "UBA trigger file canonical revision mismatch: trigger=${TRIGGER_CANONICAL} main=${CANONICAL_REVISION}."

  SOURCE_REVISION="${CANONICAL_REVISION}"
  log "Verified UBA build-request commit ${REVISION}: canonical source ${SOURCE_REVISION}, only material delta=.uba-build-request.json."
elif [[ "${BRANCH}" != "${CANONICAL_BRANCH}" ]]; then
  fail "Refusing production deployment from unapproved branch '${BRANCH}'."
fi

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

OVERSIZED="$(find "${STAGING_DIR}" -type f -size +26214400c -print -quit)"
if [[ -n "${OVERSIZED}" ]]; then
  SIZE="$(wc -c < "${OVERSIZED}" | tr -d ' ')"
  fail "Static asset exceeds Cloudflare's 25 MiB per-file limit: ${OVERSIZED} (${SIZE} bytes). Split/compress the Unity payload before production deployment."
fi

python3 - "${STAGING_DIR}/unitylaptop-build.json" "${SOURCE_REVISION}" "${REVISION}" "${BUILD_NO}" <<'PY'
import json
import pathlib
import sys
from datetime import datetime, timezone
path = pathlib.Path(sys.argv[1])
payload = {
    "sourceRepository": "karlokalinic/my-unity-project",
    "sourceRevision": sys.argv[2],
    "buildRevision": sys.argv[3],
    "buildNumber": sys.argv[4],
    "builtAtUtc": datetime.now(timezone.utc).isoformat(),
    "artifact": "Unity WebGL",
}
path.write_text(json.dumps(payload, separators=(",", ":")), encoding="utf-8")
PY

FILE_COUNT="$(find "${STAGING_DIR}" -type f | wc -l | tr -d ' ')"
SIZE_BYTES="$(du -sk "${STAGING_DIR}" | awk '{print $1 * 1024}')"
log "Validated Unity WebGL staging: files=${FILE_COUNT} bytes=${SIZE_BYTES} sourceRevision=${SOURCE_REVISION} buildRevision=${REVISION} build=${BUILD_NO}."

if ! command -v node >/dev/null 2>&1 || ! command -v npm >/dev/null 2>&1; then
  [[ -s "${HOME}/.nvm/nvm.sh" ]] || fail "Node/npm unavailable and NVM was not found on the Build Automation agent."
  # shellcheck disable=SC1090
  source "${HOME}/.nvm/nvm.sh"
  nvm install 22
  nvm use 22
fi

cd "${PROJECT_ROOT}"
npm install --no-package-lock --no-audit --no-fund
npx wrangler --version
log "Deploying canonical Unity WebGL source revision ${SOURCE_REVISION} (UBA build revision ${REVISION}) to Cloudflare Worker 'unitylaptop'."
npx wrangler deploy --config cloudflare/wrangler.uba.toml

EXPECTED_REVISION="${SOURCE_REVISION}"
for attempt in $(seq 1 18); do
  MARKER="$(curl --fail --silent --show-error --location --max-time 20 "${PUBLIC_URL}/unitylaptop-build.json" 2>/dev/null || true)"
  INDEX="$(curl --fail --silent --show-error --location --max-time 20 "${PUBLIC_URL}/" 2>/dev/null || true)"
  if [[ -n "${MARKER}" ]] && [[ -n "${INDEX}" ]] && \
     [[ "${MARKER}" == *'"sourceRepository":"karlokalinic/my-unity-project"'* ]] && \
     [[ "${MARKER}" == *"\"sourceRevision\":\"${EXPECTED_REVISION}\""* ]] && \
     [[ "${INDEX}" != *'app.js?v=20260905-physics1'* ]] && \
     { [[ "${INDEX}" == *'createUnityInstance'* ]] || [[ "${INDEX}" == *'.loader.js'* ]]; }; then
    log "CLOUDFLARE_DEPLOY_OK url=${PUBLIC_URL} build=${BUILD_NO} sourceRevision=${SOURCE_REVISION} buildRevision=${REVISION}"
    exit 0
  fi
  log "Public revision verification ${attempt}/18 not ready; sleeping 5s."
  sleep 5
done

fail "Cloudflare deploy returned but public Worker did not serve canonical Unity source revision ${SOURCE_REVISION}."
