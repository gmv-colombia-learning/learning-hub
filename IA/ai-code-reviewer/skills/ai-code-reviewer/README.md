# AI Code Reviewer

Skill para revisar código generado o asistido por IA antes de abrir o integrar una Pull Request (PR) o Merge Request (MR). Contrasta los cambios con requisitos, criterios de aceptación, arquitectura, reglas de negocio, seguridad, mantenibilidad y pruebas.

El proyecto contiene instrucciones para un agente y dos scripts auxiliares de lectura. No incluye un servidor, una API ni un pipeline que publique revisiones automáticamente. Compilar o tener tests verdes no equivale a aprobar el cambio: el mejor resultado es `READY FOR PR REVIEW`, sujeto a revisión humana.

## Qué revisa

- Cumplimiento de requisitos y criterios de aceptación.
- Defectos funcionales, casos límite y riesgos de regresión.
- Arquitectura, responsabilidades, legibilidad y code smells.
- Seguridad, privacidad y manejo de datos sensibles.
- Calidad de las pruebas y validaciones disponibles en el proyecto.
- Cambios de frontend/UI, con imágenes de Figma como referencia opcional.

Detecta señales del stack desde los manifests del repositorio y adapta la revisión a las tecnologías encontradas. Incluye criterios específicos para Angular y .NET; para otros stacks utiliza la configuración, las convenciones y la evidencia del proyecto. La detección es orientativa y debe contrastarse con los archivos reales.

## Requisitos

- Un agente compatible con skills, como Codex, con acceso al repositorio que se va a revisar.
- Git para consultar ramas, commits y diferencias.
- Python 3.7 o superior para ejecutar `detect_technology.py`; utiliza únicamente la biblioteca estándar.
- Bash y las utilidades `find`, `sort` y `head` para `collect_review_context.sh`. En Windows puedes usar Git Bash o WSL con Git disponible en ese entorno.
- Las herramientas del proyecto revisado para ejecutar sus propias pruebas, compilación y análisis.

GitHub y Jira son opcionales y requieren accesos autorizados configurados en el agente. Instalar esta skill no instala conectores ni configura credenciales. También puedes aportar los requisitos y criterios de aceptación directamente en la conversación.

## Estructura

```text
ai-code-reviewer/
├── SKILL.md                         # Instrucciones y flujo de revisión
├── agents/
│   └── openai.yaml                  # Nombre, descripción y prompt para la interfaz
├── references/
│   ├── local-first.md              # Uso de GitHub/Jira y herramientas locales
│   ├── output-format.md            # Plantilla del reporte
│   ├── review-checklist.md         # Criterios generales, Angular y .NET
│   └── technology-detection.md     # Detección de tecnologías y subproyectos
└── scripts/
    ├── collect_review_context.sh   # Resumen de Git, documentación y manifests
    └── detect_technology.py        # Señales del stack en formato JSON
```

## Instalación en Codex

Clona o descarga este repositorio. Los comandos siguientes se ejecutan desde su raíz y copian la carpeta completa de la skill, incluidas sus referencias y scripts. Sustituye las rutas de ejemplo por las de tu entorno.

### Integrar en un proyecto

Agrega esta carpeta en `<proyecto-destino>/.agents/skills/ai-code-reviewer/`. 


Abre el proyecto destino en Codex y comprueba que aparezca `ai-code-reviewer` entre las skills disponibles.

Referencia de instalación y descubrimiento: [documentación oficial de skills de Codex](https://developers.openai.com/codex/skills/).

Si no quieres enviar esto al repositoroio no olvides enlazarla en el fichero `.gitignore`

### Revisión local

```text
Usa $ai-code-reviewer para revisar mi rama actual contra origin/main.
```

## Revisión antes de PR

Para revisar cambios antes de una PR, utiliza la skill `ai-code-reviewer`
instalada en `.agents/skills/ai-code-reviewer/`.
Contrasta el diff con los requisitos, criterios de aceptación y reglas del proyecto.
Identifica la rama destino y registra las validaciones ejecutadas y las pendientes.
Entrega hallazgos con evidencia y prioridad P0–P3.
```

La instalación y este bloque no crean un disparador automático ni un check obligatorio en GitHub. Una integración con CI requiere configurar por separado la ejecución del agente, sus permisos y la publicación del reporte; este repositorio no incluye ese pipeline.

## Scripts auxiliares

Los siguientes ejemplos asumen la instalación por proyecto y se ejecutan desde la raíz del repositorio destino. Para una instalación por usuario, sustituye `.agents/skills` por la ruta de instalación correspondiente.

### Detectar tecnologías

```bash
python .agents/skills/ai-code-reviewer/scripts/detect_technology.py .
```

En sistemas donde Python 3 se invoca como `python3`, utiliza ese comando.

Devuelve JSON con `repository` y `technologies`; cada tecnología incluye nombre, tipo, versión o `UNKNOWN`, evidencia, alcance y confianza. Reconoce manifests de Node.js, .NET, Python, JVM, Go, Rust, PHP, Ruby y Swift, además de señales de Android, Docker y Terraform. Algunas detecciones solo identifican la plataforma o herramienta, sin resolver versiones ni frameworks.

Omite directorios habituales de dependencias y artefactos y limita el recorrido a 50 000 archivos. No instala dependencias, no compila y no ejecuta escáneres.

### Recopilar contexto de Git

Ejecuta en Bash, dentro del repositorio que quieres revisar:

```bash
bash .agents/skills/ai-code-reviewer/scripts/collect_review_context.sh origin/main
```

La base predeterminada es `origin/main`; puedes sustituirla por `origin/develop` u otra referencia local válida. El script muestra raíz, HEAD, rama, archivos cambiados y estadísticas de `base...HEAD`, además de listas de documentación y manifests candidatos.

La comparación usa los commits desde el ancestro común con la base. No incluye cambios sin commit en ese resumen. Si la base no existe localmente, muestra una advertencia y el estado del árbol de trabajo; esa salida no sustituye la comparación contra la base correcta.

No imprime el contenido del diff ni ejecuta tests o analizadores. Para revisar también modificaciones pendientes, consulta por separado `git diff`, `git diff --cached` y los archivos sin seguimiento indicados por `git status --short`.

## Reporte y estados

El reporte incluye tecnologías detectadas, cobertura de contexto, hallazgos priorizados con evidencia, validaciones, cumplimiento de criterios, riesgos residuales y veredicto. La plantilla completa está en [output-format.md](skills/ai-code-reviewer/references/output-format.md).

| Estado | Significado |
|---|---|
| `CONTEXT REQUIRED` | Falta información crítica para validar el cambio. |
| `CHANGES REQUIRED` | Existen bloqueantes, incumplimientos o fallos relevantes. |
| `READY FOR TESTING` | No hay P0/P1 conocidos, pero faltan validaciones relevantes. |
| `READY FOR PR REVIEW` | El alcance revisado está listo para revisión humana, con riesgos residuales documentados. |

Los hallazgos se clasifican como `P0 BLOCKER`, `P1 HIGH`, `P2 MEDIUM` o `P3 LOW`. Cada validación se registra como `PASS`, `FAIL`, `NOT EXECUTED` o `NOT AVAILABLE`; una validación pendiente nunca cuenta como aprobada.

## Alcance y confidencialidad

La skill prioriza la lectura del repositorio y las herramientas locales. Por defecto revisa y propone cambios; no modifica código ni publica comentarios en GitHub/Jira sin una solicitud explícita.

Sus instrucciones restringen el envío de código o datos de cliente a servicios externos adicionales y exigen ocultar secretos en los reportes. Esta política de trabajo no configura el procesamiento ni la retención de datos del proveedor del agente: utiliza un entorno autorizado por tu organización.

La revisión asistida por IA puede detectar riesgos y code smells, pero no garantiza encontrar todos los defectos ni sustituye un SAST, un análisis de Sonar o la revisión humana. Solo reporta como ejecutadas las herramientas que realmente se hayan utilizado.

## Personalización

- Ajusta [SKILL.md](skills/ai-code-reviewer/SKILL.md) para cambiar el flujo o la política de revisión.
- Amplía [review-checklist.md](skills/ai-code-reviewer/references/review-checklist.md) con criterios específicos del equipo.
- Modifica [output-format.md](skills/ai-code-reviewer/references/output-format.md) para adaptar el reporte.
- Mantén las reglas propias de cada proyecto en su documentación y `AGENTS.md`, para que puedan versionarse junto con el código.
