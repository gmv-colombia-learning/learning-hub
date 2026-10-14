#!/usr/bin/env python3
"""Local, read-only technology detector for repository review context."""

from __future__ import annotations

import json
import os
import re
import sys
from pathlib import Path
from xml.etree import ElementTree

SKIP_DIRS = {".git", "node_modules", "bin", "obj", "vendor", ".venv", "venv", "dist", "build", ".next", "coverage"}
MAX_FILES = 50000


def walk_files(root: Path):
    count = 0
    for current, dirs, files in os.walk(root):
        dirs[:] = [d for d in dirs if d not in SKIP_DIRS]
        base = Path(current)
        for name in files:
            yield base / name
            count += 1
            if count >= MAX_FILES:
                return


def add(items, name, kind, version, evidence, scope=".", confidence="High"):
    key = (name, kind, version or "UNKNOWN", evidence, scope)
    if key not in {(i["name"], i["kind"], i["version"], i["evidence"], i["scope"]) for i in items}:
        items.append({
            "name": name,
            "kind": kind,
            "version": version or "UNKNOWN",
            "evidence": evidence,
            "scope": scope,
            "confidence": confidence,
        })


def read_json(path: Path):
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except Exception:
        return None


def detect_package_json(path: Path, root: Path, items):
    data = read_json(path)
    if not isinstance(data, dict):
        return
    scope = str(path.parent.relative_to(root)) or "."
    deps = {}
    for field in ("dependencies", "devDependencies", "peerDependencies"):
        val = data.get(field)
        if isinstance(val, dict):
            deps.update(val)

    engines = data.get("engines") if isinstance(data.get("engines"), dict) else {}
    if "node" in engines:
        add(items, "Node.js", "runtime", str(engines["node"]), str(path.relative_to(root)), scope)
    else:
        add(items, "Node.js", "runtime", None, str(path.relative_to(root)), scope, "Medium")

    frameworks = [
        ("Angular", "@angular/core"),
        ("React", "react"),
        ("Vue", "vue"),
        ("Next.js", "next"),
        ("NestJS", "@nestjs/core"),
        ("Svelte", "svelte"),
        ("Express", "express"),
    ]
    for label, dep in frameworks:
        if dep in deps:
            add(items, label, "framework", str(deps[dep]), f"{path.relative_to(root)}:{dep}", scope)

    tools = [
        ("TypeScript", "typescript", "language/tooling"),
        ("Jest", "jest", "test"),
        ("Vitest", "vitest", "test"),
        ("Cypress", "cypress", "e2e"),
        ("Playwright", "@playwright/test", "e2e"),
        ("ESLint", "eslint", "quality"),
    ]
    for label, dep, kind in tools:
        if dep in deps:
            add(items, label, kind, str(deps[dep]), f"{path.relative_to(root)}:{dep}", scope)


def detect_csproj(path: Path, root: Path, items):
    scope = str(path.parent.relative_to(root)) or "."
    frameworks = []
    try:
        tree = ElementTree.parse(path)
        for tag in ("TargetFramework", "TargetFrameworks"):
            for elem in tree.iter():
                if elem.tag.split("}")[-1] == tag and elem.text:
                    frameworks.extend(x.strip() for x in elem.text.split(";") if x.strip())
    except Exception:
        pass
    if frameworks:
        for tfm in frameworks:
            add(items, ".NET", "runtime/framework", tfm, str(path.relative_to(root)), scope)
    else:
        add(items, ".NET", "runtime/framework", None, str(path.relative_to(root)), scope, "Medium")
    add(items, "C#", "language", None, str(path.relative_to(root)), scope, "High")


def detect_pyproject(path: Path, root: Path, items):
    scope = str(path.parent.relative_to(root)) or "."
    text = path.read_text(encoding="utf-8", errors="ignore")
    m = re.search(r"requires-python\s*=\s*[\"']([^\"']+)", text)
    add(items, "Python", "runtime/language", m.group(1) if m else None, str(path.relative_to(root)), scope, "High")
    for label, token in (("Django", "django"), ("FastAPI", "fastapi"), ("Flask", "flask"), ("Pytest", "pytest"), ("Ruff", "ruff")):
        if re.search(rf"(?im)^\s*{re.escape(token)}\s*(?:=|[<>=])", text) or f'"{token}"' in text or f"'{token}'" in text:
            kind = "test" if label == "Pytest" else "quality" if label == "Ruff" else "framework"
            add(items, label, kind, None, str(path.relative_to(root)), scope, "Medium")


def main():
    root = Path(sys.argv[1] if len(sys.argv) > 1 else ".").resolve()
    if not root.exists() or not root.is_dir():
        print(json.dumps({"error": "repository path not found"}, indent=2))
        return 2

    items = []
    files = list(walk_files(root))

    for path in files:
        name = path.name
        rel = str(path.relative_to(root))
        scope = str(path.parent.relative_to(root)) or "."
        if name == "package.json":
            detect_package_json(path, root, items)
        elif path.suffix.lower() in {".csproj", ".fsproj", ".vbproj"}:
            detect_csproj(path, root, items)
        elif name == "pyproject.toml":
            detect_pyproject(path, root, items)
        elif name.startswith("requirements") and path.suffix == ".txt":
            add(items, "Python", "runtime/language", None, rel, scope, "High")
        elif name == "pom.xml":
            add(items, "Java/JVM", "runtime/language", None, rel, scope, "High")
            add(items, "Maven", "build", None, rel, scope, "High")
        elif name in {"build.gradle", "build.gradle.kts"}:
            add(items, "Java/Kotlin JVM", "runtime/language", None, rel, scope, "Medium")
            add(items, "Gradle", "build", None, rel, scope, "High")
        elif name == "go.mod":
            text = path.read_text(encoding="utf-8", errors="ignore")
            m = re.search(r"(?m)^go\s+([0-9.]+)", text)
            add(items, "Go", "runtime/language", m.group(1) if m else None, rel, scope, "High")
        elif name == "Cargo.toml":
            add(items, "Rust", "runtime/language", None, rel, scope, "High")
        elif name == "composer.json":
            add(items, "PHP", "runtime/language", None, rel, scope, "High")
            add(items, "Composer", "build/dependency", None, rel, scope, "High")
        elif name == "Gemfile":
            add(items, "Ruby", "runtime/language", None, rel, scope, "High")
        elif name == "Package.swift":
            add(items, "Swift", "language", None, rel, scope, "High")
            add(items, "Swift Package Manager", "build/dependency", None, rel, scope, "High")
        elif name == "AndroidManifest.xml":
            add(items, "Android", "platform", None, rel, scope, "High")
        elif name == "Dockerfile" or name.startswith("Dockerfile."):
            add(items, "Docker", "infrastructure", None, rel, scope, "High")
        elif path.suffix == ".tf":
            add(items, "Terraform", "infrastructure", None, rel, scope, "High")

    items.sort(key=lambda x: (x["scope"], x["kind"], x["name"], x["version"]))
    print(json.dumps({"repository": str(root), "technologies": items}, indent=2, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
