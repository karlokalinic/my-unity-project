const CURRENT_KEY = 'unitylaptop/current.json';

const MIME = new Map([
  ['.html', 'text/html; charset=utf-8'],
  ['.js', 'text/javascript; charset=utf-8'],
  ['.json', 'application/json; charset=utf-8'],
  ['.css', 'text/css; charset=utf-8'],
  ['.wasm', 'application/wasm'],
  ['.data', 'application/octet-stream'],
  ['.symbols.json', 'application/json; charset=utf-8'],
  ['.png', 'image/png'],
  ['.jpg', 'image/jpeg'],
  ['.jpeg', 'image/jpeg'],
  ['.svg', 'image/svg+xml'],
  ['.ico', 'image/x-icon'],
]);

function responseText(message, status) {
  return new Response(message, {
    status,
    headers: {
      'content-type': 'text/plain; charset=utf-8',
      'cache-control': 'no-store',
      'x-content-type-options': 'nosniff',
    },
  });
}

function extensionFor(path) {
  const uncompressed = path.replace(/\.(br|gz)$/i, '');
  if (uncompressed.endsWith('.symbols.json')) return '.symbols.json';
  const index = uncompressed.lastIndexOf('.');
  return index >= 0 ? uncompressed.slice(index).toLowerCase() : '';
}

function safeRelativePath(url) {
  let path = decodeURIComponent(url.pathname);
  if (path === '/' || path === '') return 'index.html';
  path = path.replace(/^\/+/, '');
  if (!path || path.includes('..') || path.includes('\\') || path.includes('\0')) return null;
  return path;
}

async function currentRelease(env) {
  const object = await env.BUILDS.get(CURRENT_KEY);
  if (!object) return null;
  try {
    const pointer = JSON.parse(await object.text());
    if (!pointer || !/^[0-9a-f]{40}$/i.test(pointer.sourceRevision || '') || !String(pointer.prefix || '').startsWith('unitylaptop/releases/')) {
      return null;
    }
    return pointer;
  } catch {
    return null;
  }
}

export default {
  async fetch(request, env) {
    if (!['GET', 'HEAD'].includes(request.method)) return responseText('Method not allowed.', 405);

    const url = new URL(request.url);
    const relative = safeRelativePath(url);
    if (!relative) return responseText('Invalid path.', 400);

    const release = await currentRelease(env);
    if (!release) return responseText('No verified UNITYLAPTOP Unity WebGL release is promoted.', 503);

    const object = await env.BUILDS.get(`${release.prefix}/${relative}`);
    if (!object) return responseText('Not found.', 404);

    const headers = new Headers();
    object.writeHttpMetadata(headers);
    if (!headers.has('content-type')) headers.set('content-type', MIME.get(extensionFor(relative)) || 'application/octet-stream');
    if (!headers.has('content-encoding')) {
      if (/\.br$/i.test(relative)) headers.set('content-encoding', 'br');
      else if (/\.gz$/i.test(relative)) headers.set('content-encoding', 'gzip');
    }

    headers.set('etag', object.httpEtag);
    headers.set('x-content-type-options', 'nosniff');
    headers.set('cross-origin-resource-policy', 'same-origin');
    headers.set('x-unitylaptop-source-revision', release.sourceRevision);

    if (relative === 'index.html' || relative === 'unitylaptop-build.json') {
      headers.set('cache-control', 'no-store, max-age=0');
    } else {
      headers.set('cache-control', 'public, max-age=31536000, immutable');
    }

    return new Response(request.method === 'HEAD' ? null : object.body, {
      status: 200,
      headers,
    });
  },
};
