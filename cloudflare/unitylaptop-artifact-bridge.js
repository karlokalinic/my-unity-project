const EXPECTED_ISSUER = 'https://token.actions.githubusercontent.com';
const EXPECTED_AUDIENCE = 'unitylaptop-artifact-bridge';
const EXPECTED_REPOSITORY = 'karlokalinic/UNITYLAPTOP';
const EXPECTED_REF = 'refs/heads/main';
const CANONICAL_REPOSITORY = 'karlokalinic/my-unity-project';
const RELEASE_ROOT = 'unitylaptop/releases';
const CURRENT_KEY = 'unitylaptop/current.json';

let cachedJwks = null;
let cachedJwksAt = 0;

function json(value, status = 200) {
  return new Response(JSON.stringify(value), {
    status,
    headers: {
      'content-type': 'application/json; charset=utf-8',
      'cache-control': 'no-store',
      'x-content-type-options': 'nosniff',
    },
  });
}

function decodeBase64Url(value) {
  const normalized = value.replace(/-/g, '+').replace(/_/g, '/');
  const padded = normalized + '='.repeat((4 - normalized.length % 4) % 4);
  const binary = atob(padded);
  return Uint8Array.from(binary, c => c.charCodeAt(0));
}

function decodeJsonPart(value) {
  return JSON.parse(new TextDecoder().decode(decodeBase64Url(value)));
}

async function loadJwks() {
  const now = Date.now();
  if (cachedJwks && now - cachedJwksAt < 10 * 60 * 1000) return cachedJwks;

  const response = await fetch(`${EXPECTED_ISSUER}/.well-known/jwks`, {
    cf: { cacheTtl: 600, cacheEverything: true },
  });
  if (!response.ok) throw new Error(`GitHub OIDC JWKS lookup failed (${response.status}).`);
  const payload = await response.json();
  if (!Array.isArray(payload.keys)) throw new Error('GitHub OIDC JWKS response is invalid.');
  cachedJwks = payload.keys;
  cachedJwksAt = now;
  return cachedJwks;
}

async function verifyGitHubOidc(token) {
  const parts = token.split('.');
  if (parts.length !== 3) throw new Error('Malformed OIDC token.');

  const header = decodeJsonPart(parts[0]);
  const claims = decodeJsonPart(parts[1]);
  if (header.alg !== 'RS256' || !header.kid) throw new Error('Unsupported OIDC signing header.');

  const jwks = await loadJwks();
  const jwk = jwks.find(key => key.kid === header.kid);
  if (!jwk) throw new Error('GitHub OIDC signing key was not found.');

  const key = await crypto.subtle.importKey(
    'jwk',
    jwk,
    { name: 'RSASSA-PKCS1-v1_5', hash: 'SHA-256' },
    false,
    ['verify'],
  );

  const signed = new TextEncoder().encode(`${parts[0]}.${parts[1]}`);
  const signature = decodeBase64Url(parts[2]);
  const valid = await crypto.subtle.verify('RSASSA-PKCS1-v1_5', key, signature, signed);
  if (!valid) throw new Error('GitHub OIDC signature verification failed.');

  const now = Math.floor(Date.now() / 1000);
  const audiences = Array.isArray(claims.aud) ? claims.aud : [claims.aud];
  if (claims.iss !== EXPECTED_ISSUER) throw new Error('Unexpected OIDC issuer.');
  if (!audiences.includes(EXPECTED_AUDIENCE)) throw new Error('Unexpected OIDC audience.');
  if (!claims.exp || claims.exp < now - 30) throw new Error('Expired OIDC token.');
  if (claims.nbf && claims.nbf > now + 30) throw new Error('OIDC token is not active yet.');
  if (claims.repository !== EXPECTED_REPOSITORY) throw new Error('Unexpected GitHub repository identity.');
  if (claims.ref !== EXPECTED_REF) throw new Error('Uploads are accepted only from UNITYLAPTOP main.');
  if (!['push', 'workflow_dispatch'].includes(claims.event_name)) throw new Error('Unexpected GitHub Actions event.');
  return claims;
}

async function authorize(request) {
  const value = request.headers.get('authorization') || '';
  const match = /^Bearer\s+(.+)$/i.exec(value);
  if (!match) throw new Error('Missing GitHub OIDC bearer token.');
  return verifyGitHubOidc(match[1]);
}

function parseReleasePath(pathname) {
  const match = /^\/v1\/artifacts\/([0-9a-f]{40})\/(.+)$/i.exec(pathname);
  if (!match) return null;
  const sha = match[1].toLowerCase();
  const relative = decodeURIComponent(match[2]);
  if (!relative || relative.startsWith('/') || relative.includes('..') || relative.includes('\\') || relative.includes('\0')) {
    throw new Error('Unsafe artifact path.');
  }
  return { sha, relative };
}

function releaseKey(sha, relative) {
  return `${RELEASE_ROOT}/${sha}/${relative}`;
}

async function putArtifact(request, env, parsed) {
  await authorize(request);
  const sha256 = (request.headers.get('x-artifact-sha256') || '').toLowerCase();
  if (!/^[0-9a-f]{64}$/.test(sha256)) return json({ error: 'x-artifact-sha256 is required.' }, 400);

  const sizeHeader = request.headers.get('content-length');
  const contentType = request.headers.get('content-type') || 'application/octet-stream';
  const contentEncoding = request.headers.get('content-encoding') || undefined;
  const httpMetadata = { contentType };
  if (contentEncoding) httpMetadata.contentEncoding = contentEncoding;

  await env.BUILDS.put(releaseKey(parsed.sha, parsed.relative), request.body, {
    httpMetadata,
    customMetadata: {
      sourceRevision: parsed.sha,
      sha256,
      declaredSize: sizeHeader || '',
    },
  });

  return json({ ok: true, sourceRevision: parsed.sha, path: parsed.relative });
}

async function commitRelease(request, env, sha) {
  const claims = await authorize(request);
  let payload;
  try {
    payload = await request.json();
  } catch {
    return json({ error: 'Release manifest must be JSON.' }, 400);
  }

  if (payload.sourceRepository !== CANONICAL_REPOSITORY || payload.sourceRevision !== sha) {
    return json({ error: 'Canonical source provenance does not match release path.' }, 400);
  }
  if (!Array.isArray(payload.files) || payload.files.length < 4 || payload.files.length > 1000) {
    return json({ error: 'Release file list is missing or unreasonable.' }, 400);
  }

  const seen = new Set();
  let hasIndex = false;
  let hasLoader = false;
  let hasWasm = false;
  let hasData = false;

  for (const file of payload.files) {
    const path = String(file.path || '');
    const digest = String(file.sha256 || '').toLowerCase();
    const size = Number(file.size);
    if (!path || path.startsWith('/') || path.includes('..') || path.includes('\\') || seen.has(path)) {
      return json({ error: `Invalid or duplicate release path: ${path}` }, 400);
    }
    if (!/^[0-9a-f]{64}$/.test(digest) || !Number.isSafeInteger(size) || size < 0) {
      return json({ error: `Invalid release metadata: ${path}` }, 400);
    }
    seen.add(path);
    hasIndex ||= path === 'index.html';
    hasLoader ||= /\.loader\.js$/i.test(path);
    hasWasm ||= /\.wasm(?:\.br|\.gz)?$/i.test(path);
    hasData ||= /\.data(?:\.br|\.gz)?$/i.test(path);

    const object = await env.BUILDS.head(releaseKey(sha, path));
    if (!object) return json({ error: `Uploaded object is missing: ${path}` }, 409);
    if (object.size !== size) return json({ error: `Uploaded object size mismatch: ${path}` }, 409);
    if ((object.customMetadata?.sha256 || '').toLowerCase() !== digest) {
      return json({ error: `Uploaded object digest metadata mismatch: ${path}` }, 409);
    }
  }

  if (!hasIndex || !hasLoader || !hasWasm || !hasData) {
    return json({ error: 'Release is not a complete Unity WebGL player.' }, 400);
  }

  const builtAtUtc = new Date().toISOString();
  const marker = {
    sourceRepository: CANONICAL_REPOSITORY,
    sourceRevision: sha,
    buildRevision: payload.buildRevision || claims.sha || 'unknown',
    buildNumber: payload.buildNumber || 'unknown',
    builtAtUtc,
    artifact: 'Unity WebGL',
    transport: 'GitHub OIDC -> Cloudflare R2',
  };

  await env.BUILDS.put(releaseKey(sha, 'unitylaptop-build.json'), JSON.stringify(marker), {
    httpMetadata: { contentType: 'application/json; charset=utf-8' },
    customMetadata: { sourceRevision: sha },
  });

  const manifest = {
    ...payload,
    sourceRepository: CANONICAL_REPOSITORY,
    sourceRevision: sha,
    committedAtUtc: builtAtUtc,
    authenticatedRepository: claims.repository,
    authenticatedWorkflow: claims.workflow || 'unknown',
  };
  await env.BUILDS.put(releaseKey(sha, 'release-manifest.json'), JSON.stringify(manifest), {
    httpMetadata: { contentType: 'application/json; charset=utf-8' },
    customMetadata: { sourceRevision: sha },
  });

  await env.BUILDS.put(CURRENT_KEY, JSON.stringify({
    sourceRepository: CANONICAL_REPOSITORY,
    sourceRevision: sha,
    prefix: `${RELEASE_ROOT}/${sha}`,
    promotedAtUtc: builtAtUtc,
  }), {
    httpMetadata: { contentType: 'application/json; charset=utf-8' },
    customMetadata: { sourceRevision: sha },
  });

  return json({ ok: true, promoted: true, marker });
}

export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    try {
      if (request.method === 'GET' && url.pathname === '/health') {
        return json({ ok: true, service: 'unitylaptop-artifact-bridge', protocol: 1 });
      }

      const parsed = parseReleasePath(url.pathname);
      if (request.method === 'PUT' && parsed) return putArtifact(request, env, parsed);

      const commit = /^\/v1\/releases\/([0-9a-f]{40})\/commit$/i.exec(url.pathname);
      if (request.method === 'POST' && commit) return commitRelease(request, env, commit[1].toLowerCase());

      return json({ error: 'Not found.' }, 404);
    } catch (error) {
      const message = error instanceof Error ? error.message : String(error);
      const authFailure = /OIDC|GitHub|bearer|repository|audience|issuer|token|Uploads are accepted/i.test(message);
      return json({ error: message }, authFailure ? 401 : 500);
    }
  },
};
