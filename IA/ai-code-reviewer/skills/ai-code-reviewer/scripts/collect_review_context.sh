#!/usr/bin/env bash
set -eu

base_ref="${1:-origin/main}"

if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
  echo "ERROR: current directory is not a git repository" >&2
  exit 2
fi

repo_root="$(git rev-parse --show-toplevel)"
cd "$repo_root"

printf '# Local Review Context\n\n'
printf 'Repository: `%s`\n' "$repo_root"
printf 'HEAD: `%s`\n' "$(git rev-parse --short HEAD)"
printf 'Branch: `%s`\n' "$(git branch --show-current 2>/dev/null || true)"
printf 'Requested base: `%s`\n\n' "$base_ref"

if git rev-parse --verify "$base_ref" >/dev/null 2>&1; then
  printf '## Changed files vs %s\n\n' "$base_ref"
  git diff --name-status "$base_ref"...HEAD || true
  printf '\n## Diff stat\n\n'
  git diff --stat "$base_ref"...HEAD || true
else
  printf 'WARNING: base ref `%s` was not found locally.\n\n' "$base_ref"
  printf '## Working tree changes\n\n'
  git status --short || true
fi

printf '\n## Candidate documentation\n\n'
find . -type f \( -name 'README.md' -o -name '*.md' \) \
  -not -path './.git/*' \
  -not -path './node_modules/*' \
  -not -path './bin/*' \
  -not -path './obj/*' \
  | sort | head -200

printf '\n## Candidate manifests and project files\n\n'
find . -type f \( \
  -name 'package.json' -o \
  -name '*.csproj' -o \
  -name '*.fsproj' -o \
  -name '*.vbproj' -o \
  -name '*.sln' -o \
  -name 'Directory.Packages.props' -o \
  -name 'Directory.Build.props' -o \
  -name 'global.json' -o \
  -name 'pom.xml' -o \
  -name 'build.gradle' -o \
  -name 'build.gradle.kts' -o \
  -name 'pyproject.toml' -o \
  -name 'requirements*.txt' -o \
  -name 'Pipfile' -o \
  -name 'composer.json' -o \
  -name 'go.mod' -o \
  -name 'Cargo.toml' -o \
  -name 'Gemfile' -o \
  -name 'Package.swift' -o \
  -name 'AndroidManifest.xml' -o \
  -name 'Dockerfile' -o \
  -name '*.tf' \
\) \
  -not -path './.git/*' \
  -not -path './node_modules/*' \
  -not -path './bin/*' \
  -not -path './obj/*' \
  -not -path './vendor/*' \
  -not -path './.venv/*' \
  | sort | head -200
