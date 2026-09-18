#!/usr/bin/env python3
import json
import re
import sys
from pathlib import Path
from urllib.parse import urljoin, urlparse
from urllib.request import Request, urlopen

BASE = "https://unitylaptop.karlolegend.workers.dev/"
EXPECTED_SOURCE = "50a5cb56ea8be69e50662384f0242e7920a1526a"
OUT = Path(__file__).resolve().parent / "app" / "src" / "main" / "assets" / "www"
UA = "ZRAKOPERKA-v16-offline-packager/1.0"

def fetch(url: str) -> bytes:
    req = Request(url, headers={"User-Agent": UA, "Cache-Control": "no-cache"})
    with urlopen(req, timeout=60) as resp:
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

def same_origin_abs(rel: str) -> str:
    return urljoin(BASE, rel)

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

    lm = re.search(r'var\s+loaderUrl\s*=\s*([^;]+);', html)
    if lm:
        rel = resolve_expr(lm.group(1), build_url)
        if rel:
            wanted.add(rel)

    for key, expr in re.findall(r'([A-Za-z0-9_]+Url)\s*:\s*([^,\n}]+)', html):
        rel = resolve_expr(expr, build_url)
        if rel and not rel.startswith(("http://", "https://", "blob:", "data:")):
            wanted.add(rel)

    for attr in re.findall(r'(?:src|href)=["\']([^"\']+)["\']', html, flags=re.I):
        if not attr.startswith(("http://", "https://", "//", "data:", "#")):
            wanted.add(attr)

    downloaded: set[str] = set()
    queue = list(wanted)
    while queue:
        rel = queue.pop(0)
        rel = rel.split("?", 1)[0].split("#", 1)[0]
        if not rel or rel in downloaded or rel.endswith("/"):
            continue
        downloaded.add(rel)
        data = fetch(same_origin_abs(rel))
        path = save_rel(rel, data)

        if path.suffix.lower() in {".css", ".html"}:
            text = data.decode("utf-8", errors="ignore")
            refs = re.findall(r'(?:url\(|src=["\']|href=["\'])([^)"\']+)', text, flags=re.I)
            for ref in refs:
                ref = ref.strip("'\" ")
                if ref and not ref.startswith(("http://", "https://", "//", "data:", "#")):
                    queue.append(urljoin(rel, ref))

    required_suffixes = (".loader.js", ".data", ".framework.js", ".wasm", ".unityweb")
    files = [p.relative_to(OUT).as_posix() for p in OUT.rglob("*") if p.is_file()]
    print("embedded files:", files)

    if not any(name.endswith(".loader.js") for name in files):
        raise SystemExit("Unity loader was not downloaded.")
    if not any(name.endswith((".wasm", ".unityweb")) for name in files):
        raise SystemExit("Unity code payload was not downloaded.")
    if not any(name.endswith((".data", ".unityweb")) for name in files):
        raise SystemExit("Unity data payload was not downloaded.")

    manifest = {
        "sourceRevision": EXPECTED_SOURCE,
        "sourceBuildNumber": marker.get("buildNumber"),
        "files": files,
    }
    save_rel("offline-manifest.json", json.dumps(manifest, indent=2).encode("utf-8"))
    print("OFFLINE_WEBGL_READY")

if __name__ == "__main__":
    main()
