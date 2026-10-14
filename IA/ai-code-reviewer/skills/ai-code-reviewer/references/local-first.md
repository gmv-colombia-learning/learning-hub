# Guía local-first: GitHub + Jira + repositorio local

## Objetivo

Usar GitHub y Jira para obtener contexto colaborativo y mantener el análisis técnico en el entorno local siempre que sea posible.

## Flujo recomendado

1. Obtener el issue de Jira y sus criterios de aceptación.
2. Obtener de GitHub la PR/rama, base, commits y archivos cambiados.
3. Cambiar al repositorio local para leer contexto, arquitectura, manifests y código adyacente.
4. Detectar automáticamente tecnologías y versiones por subproyecto antes de seleccionar comandos/checklists.
5. Ejecutar localmente las validaciones disponibles.
6. Comparar resultados con Jira y GitHub.
7. Emitir el reporte sin publicar comentarios ni modificar la PR salvo petición explícita.

## Resolución de identificadores

Intentar identificar el Jira ID desde:
- Título o descripción de PR.
- Nombre de rama.
- Mensajes de commit.

Si hay múltiples candidatos, pedir confirmación.

Intentar identificar la base branch desde GitHub. Si no está disponible, pedirla o usar la configuración local solo si es inequívoca.

## Privacidad

- No enviar diffs completos a Jira.
- No duplicar código en tickets si basta con una referencia.
- Redactar secretos y datos sensibles de cualquier reporte.
- No usar conectores externos adicionales sin autorización explícita.

## Sonar y herramientas de calidad

Priorizar configuración local existente, por ejemplo:
- SonarScanner configurado en el repositorio o pipeline local reproducible.
- linters del proyecto.
- analyzers de .NET.
- scripts npm definidos en `package.json`.
- SAST, escáneres de dependencias y detectores de secretos ya configurados para ejecutarse localmente o en CI.

Si Sonar solo existe de forma remota y no hay integración autorizada, no bloquear la revisión exclusivamente por eso. Marcar `Sonar: NOT AVAILABLE` o `NOT EXECUTED` y reducir la confianza si la política del proyecto lo exige.

La revisión de la skill puede buscar code smells y patrones de vulnerabilidad leyendo el diff y su contexto, sin Sonar. Tratarla como revisión manual asistida por IA, no como una garantía de detección completa ni como una clasificación oficial de Sonar. Registrar qué analizadores realmente se ejecutaron y no inventar resultados.

## Qué recopila el script de contexto

`scripts/collect_review_context.sh [base-ref]` informa la raíz del repositorio, HEAD/rama, archivos cambiados, resumen del diff y listas de documentación/manifests candidatos. No imprime el contenido del diff, no ejecuta build/tests/analizadores y no invoca Sonar. Después de usarlo, leer el diff y los archivos relevantes por separado antes de emitir hallazgos.

## Detección tecnológica y revisión de manifests

Usar `scripts/detect_technology.py` como señal inicial cuando exista repositorio local y contrastar siempre su salida con los archivos reales. Detectar múltiples subproyectos si existen.

Revisar según señales presentes, entre otros:
- Node/frontend: `package.json`, lockfile, configs de framework y scripts.
- .NET: `*.sln`, `*.csproj`, `Directory.Packages.props`, `Directory.Build.props`, `global.json`.
- JVM: `pom.xml`, `build.gradle`, `build.gradle.kts`, wrappers y toolchains.
- Python: `pyproject.toml`, `requirements*.txt`, `Pipfile`, lockfiles.
- PHP: `composer.json` y `composer.lock`.
- Go: `go.mod` y `go.sum`.
- Rust: `Cargo.toml`, `Cargo.lock`, `rust-toolchain*`.
- Ruby: `Gemfile`, lockfile y gemspec.
- Mobile/infra: archivos de proyecto, Docker, Terraform, Helm o Kubernetes según aplique.

No confiar en una tecnología o dependencia descrita en README si no coincide con los manifests/configuración del repositorio. No pedir al usuario que indique el stack cuando pueda detectarse con evidencia suficiente.
