# Detección automática de tecnología

## Objetivo

Detectar el stack real del repositorio antes de aplicar reglas de revisión específicas. No asumir Angular, .NET ni ninguna otra tecnología por contexto histórico del equipo.

## Reglas

1. Detectar por evidencia del repositorio, no por intuición.
2. Priorizar manifests, archivos de proyecto, lockfiles y configuración sobre extensiones sueltas.
3. Detectar múltiples tecnologías y múltiples subproyectos en monorepos.
4. Identificar versiones cuando exista evidencia confiable.
5. Separar framework, runtime, lenguaje, build tool, test framework y herramientas de calidad.
6. Si dos señales se contradicen, reportar la ambigüedad y no inventar una versión.
7. Si no hay evidencia suficiente, marcar la tecnología como `UNCONFIRMED` y pedir el dato solo si afecta la revisión.

## Evidencia de alta confianza

- Node/Frontend: `package.json`, lockfile y archivos de configuración del framework.
- Angular: dependencia `@angular/core` y configuración Angular del proyecto.
- React: dependencias `react`/`react-dom`; frameworks como Next.js se detectan por sus dependencias/configuración.
- Vue: dependencia `vue` y configuración asociada.
- .NET: `*.csproj`, `*.fsproj`, `*.vbproj`, `global.json`, `Directory.Packages.props`.
- Java/Kotlin JVM: `pom.xml`, `build.gradle`, `build.gradle.kts`, wrappers Maven/Gradle.
- Python: `pyproject.toml`, `requirements*.txt`, `Pipfile`, `poetry.lock`, `uv.lock`.
- PHP: `composer.json`, `composer.lock`.
- Go: `go.mod`, `go.sum`.
- Rust: `Cargo.toml`, `Cargo.lock`.
- Ruby: `Gemfile`, `Gemfile.lock`, gemspec.
- Swift/iOS: `Package.swift`, `Podfile`, `.xcodeproj`/`.xcworkspace`.
- Android: `settings.gradle*`, `build.gradle*`, `AndroidManifest.xml`.
- Infra: `Dockerfile`, compose files, `*.tf`, Helm charts, Kubernetes manifests.

## Detección de versión

Extraer versiones desde fuentes autoritativas cuando existan:
- Frameworks JS: `package.json` + lockfile si se necesita resolver versión instalada.
- .NET: `TargetFramework`/`TargetFrameworks`, `global.json` y paquetes centrales.
- Java: `pom.xml` o Gradle toolchain/plugins.
- Python: `requires-python`, archivos de entorno o toolchain.
- Go: directiva `go` de `go.mod`.
- Rust: `rust-toolchain*` cuando exista.

No convertir rangos de versión en una versión exacta si el lockfile no lo demuestra.

## Resultado esperado

Antes de revisar, construir una tabla conceptual con:
- Tecnología/componente.
- Tipo: lenguaje, runtime, framework, build, tests, calidad o infraestructura.
- Versión detectada o `UNKNOWN`.
- Evidencia.
- Alcance/subproyecto.
- Confianza: Alta/Media/Baja.

Aplicar después únicamente los checklists específicos que correspondan a las tecnologías detectadas.
