#!/usr/bin/env python3
import json
import re
from pathlib import Path
from urllib.error import HTTPError
from urllib.parse import urljoin
from urllib.request import Request, urlopen

BASE = "https://unitylaptop.karlolegend.workers.dev/"
EXPECTED_SOURCE = "50a5cb56ea8be69e50662384f0242e7920a1526a"
OUT = Path(__file__).resolve().parent / "app" / "src" / "main" / "assets" / "www"
UA = "ZRAKOPERKA-v16-offline-packager/1.1"

def fetch(url: str) -> bytes:
    req = Request(url, headers={"User-Agent": UA, "Cache-Control": "no-cache"})
    with urlopen(req, timeout=90) as resp:
        return resp.read()

def save_rel(rel: str, data: bytes):
    rel = rel.split("?", 1)[0].split("#", 1)[0].lstrip("/")
    if not rel:
        rel = "index.html"
    path = OUT / rel
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(data)
    print(f"saved {rel} ({len(data)} bytes)")
    return path

def resolve_expr(expr: str, build_url: str) -> str | None:
    expr = expr.strip().rstrip(";").rstrip(",").strip()
    m = re.fullmatch(r'["\']([^"\']+)["\']', expr)
    if m:
        return m.group(1)
    m = re.fullmatch(r'buildUrl\s*\+\s*["\']([^"\']+)["\']', expr)
    if m:
        return build_url.rstrip("/") + m.group(1)
    return None

def main():
    if OUT.exists():
        for p in sorted(OUT.rglob("*"), reverse=True):
            if p.is_file() or p.is_symlink():
                p.unlink()
            elif p.is_dir():
                p.rmdir()
    OUT.mkdir(parents=True, exist_ok=True)

    marker = json.loads(fetch(urljoin(BASE, "unitylaptop-build.json")).decode("utf-8-sig"))
    print("marker:", json.dumps(marker, ensure_ascii=False))
    if marker.get("sourceRevision") != EXPECTED_SOURCE:
        raise SystemExit(
            f"Refusing to package unexpected public build. Expected {EXPECTED_SOURCE}, "
            f"got {marker.get('sourceRevision')}"
        )

    html_bytes = fetch(BASE)
    html = html_bytes.decode("utf-8", errors="replace")
    save_rel("index.html", html_bytes)
    save_rel("unitylaptop-build.json", json.dumps(marker, ensure_ascii=False).encode("utf-8"))

    bm = re.search(r'var\s+buildUrl\s*=\s*["\']([^"\']+)["\']', html)
    build_url = bm.group(1) if bm else "Build"
    print("buildUrl:", build_url)

    wanted: set[str] = set()
    required: set[str] = set()

    lm = re.search(r'var\s+loaderUrl\s*=\s*([^;]+);', html)
    if lm:
        rel = resolve_expr(lm.group(1), build_url)
        if rel:
            wanted.add(rel)
            required.add(rel)

    for key, expr in re.findall(r'([A-Za-z0-9_]+Url)\s*:\s*([^,\n}]+)', html):
        rel = resolve_expr(expr, build_url)
        if rel and not rel.startswith(("http://", "https://", "blob:", "data:")):
            wanted.add(rel)
            if key in {"dataUrl", "frameworkUrl", "codeUrl"}:
                required.add(rel)

    for attr in re.findall(r'(?:src|href)=["\']([^"\']+)["\']', html, flags=re.I):
        if not attr.startswith(("http://", "https://", "//", "data:", "#")):
            wanted.add(attr)

    print("required Unity files:", sorted(required))
    print("optional/template files:", sorted(wanted - required))

    downloaded: set[str] = set()
    skipped: set[str] = set()
    queue = list(sorted(wanted))
    while queue:
        rel = queue.pop(0).split("?", 1)[0].split("#", 1)[0]
        if not rel or rel in downloaded or rel in skipped or rel.endswith("/"):
            continue
        try:
            data = fetch(urljoin(BASE, rel))
        except HTTPError as e:
            if e.code == 404 and rel not in required:
                print(f"optional 404 skipped: {rel}")
                skipped.add(rel)
                continue
            raise

        downloaded.add(rel)
        path = save_rel(rel, data)

        if path.suffix.lower() in {".css", ".html"}:
            text = data.decode("utf-8", errors="ignore")
            refs = re.findall(r'(?:url\(|src=["\']|href=["\'])([^)"\']+)', text, flags=re.I)
            for ref in refs:
                ref = ref.strip("'\" ")
                if ref and not ref.startswith(("http://", "https://", "//", "data:", "#")):
                    queue.append(urljoin(rel, ref))

    missing_required = sorted(required - downloaded)
    if missing_required:
        raise SystemExit("Missing required Unity files: " + ", ".join(missing_required))

    files = [p.relative_to(OUT).as_posix() for p in OUT.rglob("*") if p.is_file()]
    print("embedded files:", files)

    if not any(name.endswith(".loader.js") for name in files):
        raise SystemExit("Unity loader was not downloaded.")
    if not any(".wasm" in name for name in files):
        raise SystemExit("Unity WASM payload was not downloaded.")
    if not any(".data" in name for name in files):
        raise SystemExit("Unity data payload was not downloaded.")

    manifest = {
        "sourceRevision": EXPECTED_SOURCE,
        "sourceBuildNumber": marker.get("buildNumber"),
        "buildRevision": marker.get("buildRevision"),
        "fullyOffline": True,
        "files": files,
    }
    save_rel("offline-manifest.json", json.dumps(manifest, indent=2).encode("utf-8"))
    print("OFFLINE_WEBGL_READY")

if __name__ == "__main__":
    main()
