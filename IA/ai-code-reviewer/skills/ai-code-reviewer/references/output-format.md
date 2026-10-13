# Formato de salida

Usar esta estructura como plantilla por defecto.

## AI Code Review

**Estado:** `CONTEXT REQUIRED | CHANGES REQUIRED | READY FOR TESTING | READY FOR PR REVIEW`
**Confianza:** `Alta | Media | Baja`
**Issue:** `<JIRA-ID o N/A>`
**PR/Rama:** `<referencia o N/A>`
**Base:** `<rama destino o N/A>`

### Tecnologías detectadas

| Tecnología | Tipo | Versión | Alcance | Evidencia | Confianza |
|---|---|---|---|---|---|
| `<detectada>` | `<tipo>` | `<versión o UNKNOWN>` | `<subproyecto>` | `<manifest/config>` | `Alta/Media/Baja` |

Incluir solo tecnologías respaldadas por evidencia. No asumir un stack por contexto histórico.

### Resumen

Máximo 5 líneas. Explicar qué se revisó, resultado general y principal riesgo.

### Cobertura de contexto

| Elemento | Estado | Fuente |
|---|---|---|
| Requisitos | AVAILABLE/MISSING/N/A | Jira/usuario/... |
| Criterios de aceptación | ... | ... |
| Diff/código modificado | ... | ... |
| Arquitectura/reglas | ... | ... |
| Reglas de negocio | ... | ... |
| Diseño visual de Figma (solo frontend/UI) | PROVIDED/NOT PROVIDED/NOT APPLICABLE | imagen adjunta o N/A |

### Hallazgos priorizados

Para cada hallazgo:

#### [P0/P1/P2/P3] Título concreto
- **Evidencia:** `archivo:línea` o referencia disponible.
- **Impacto:** consecuencia técnica/funcional.
- **Requisito/regla:** criterio, issue o regla afectada; `N/A` si no existe.
- **Recomendación:** acción específica.
- **Confianza:** Alta/Media/Baja.

No incluir la sección de hallazgos vacía. Si no hay hallazgos, escribir: `No se identificaron hallazgos P0-P3 en el alcance revisado.`

### Validaciones

| Validación | Resultado | Evidencia/nota |
|---|---|---|
| Build | PASS/FAIL/NOT EXECUTED/NOT AVAILABLE | ... |
| Unit tests | ... | ... |
| Integration tests | ... | ... |
| E2E | ... | ... |
| Lint/static analysis | ... | ... |
| SAST/seguridad estática | PASS/FAIL/NOT EXECUTED/NOT AVAILABLE | indicar herramienta y alcance, si aplica |
| Dependencias/secretos | ... | incluir solo si se revisaron |
| Coverage | ... | ... |
| Sonar local | ... | ... |

Incluir solo las validaciones relevantes al repositorio.

Indicar en el resumen o en riesgos residuales si la revisión de code smells y seguridad fue manual/asistida por IA o respaldada por una herramienta. No presentar la revisión de IA como cobertura exhaustiva ni como resultado de Sonar.

Para cambios de frontend/UI con imágenes de Figma, resumir discrepancias visuales verificables. Si no se pudo ejecutar o capturar la UI, declarar `NOT VERIFIED`; no afirmar equivalencia visual.

### Cumplimiento de criterios de aceptación

| Criterio | Estado | Evidencia |
|---|---|---|
| AC-1 ... | PASS/FAIL/UNVERIFIED | ... |

No marcar `PASS` sin evidencia.

### Plan de ajuste

Incluir solo si el estado es `CHANGES REQUIRED` o si existen P2/P3 accionables. Ordenar por prioridad y dependencia.

1. Acción.
2. Acción.

### Riesgos residuales / no verificado

Listar validaciones o contexto que no pudieron comprobarse y explicar por qué importan.

### Veredicto

Una frase clara y accionable. Ejemplos:
- `No está listo para PR: corregir los P1 y repetir las validaciones fallidas.`
- `Listo para testing, pero todavía falta E2E del flujo X.`
- `Listo para revisión humana de PR; no se detectaron bloqueantes en el alcance evaluado.`

Si aplica la convención solicitada por el equipo, añadir al final `status: 200` únicamente para `READY FOR TESTING` o `READY FOR PR REVIEW`.
