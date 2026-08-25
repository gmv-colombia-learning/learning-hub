---
name: sdd
description: Gestiona el flujo Spec-Driven Development de este proyecto. Usar para features, bugs, módulos nuevos, cambios funcionales, diseño de especificaciones, validación de specs e implementación basada en specs.
---

# SDD Workflow

Aplicar siempre las reglas de:

- `spec_custom/agents_dev_rules.md`
- `spec_custom/dev_rules.md`

Usar:

- `spec_custom/spec_template.md`
- `spec_custom/context/`
- `spec_custom/specs/`

como fuentes de contexto.

## General discovery

Antes de solicitar información al usuario:

1. Examinar specs existentes.
2. Examinar estructura del código.
3. Consultar contexto relevante.
4. Identificar módulo, dominio, entidad y flujo probable.
5. Clasificar información encontrada como KNOWN, INFERRED o UNKNOWN.

No preguntar información que pueda obtenerse razonablemente del proyecto.

## FEATURE workflow

Cuando la intención sea FEATURE:

1. Identificar el módulo/dominio existente.
2. Identificar entidad o flujo afectado.
3. Buscar specs relacionadas.
4. Informar al usuario dónde se considera que pertenece la feature.
5. Pedir confirmación si existe ambigüedad.
6. Cargar la plantilla.
7. Completar automáticamente toda información KNOWN.
8. Presentar información INFERRED relevante para confirmación.
9. Preguntar información UNKNOWN de forma progresiva.
10. Crear la spec en:
    `spec_custom/specs/<module>/`
11. Mantener estado DRAFT mientras haya preguntas funcionales abiertas.
12. Cambiar a READY únicamente cuando la spec sea implementable.

No escribir código durante este workflow.

## BUG workflow

Cuando la intención sea BUG:

1. Identificar dominio/módulo.
2. Identificar flujo afectado.
3. Localizar spec relacionada.
4. Determinar comportamiento actual.
5. Determinar comportamiento esperado.
6. Obtener pasos de reproducción.
7. Analizar código relevante.
8. Separar síntomas de causa raíz.
9. No asumir solución antes del análisis.
10. Crear bug spec.
11. Definir regresiones y tests requeridos.
12. Marcar READY solo cuando pueda implementarse sin supuestos.

No modificar código durante este workflow.

## MODULE workflow

Cuando la intención sea MODULE:

1. Comprobar si el módulo o concepto ya existe.
2. Buscar dominios relacionados.
3. Evitar crear un nuevo dominio si pertenece claramente a uno existente.
4. Proponer nombre lógico y nombre técnico.
5. Confirmar límites del dominio.
6. Obtener objetivos y responsabilidades.
7. Identificar entidades y conceptos principales.
8. Identificar relaciones.
9. Identificar casos de uso.
10. Identificar integraciones y dependencias.
11. Crear:
    `spec_custom/specs/<module>/README.md`
12. Crear specs individuales para casos de uso cuando corresponda.

No crear código de aplicación durante este workflow.

## IMPLEMENT workflow

Solo implementar specs con estado READY.

Antes de modificar código:

1. Leer spec completa.
2. Leer reglas del agente.
3. Leer reglas de desarrollo.
4. Leer arquitectura relevante.
5. Inspeccionar implementación existente.
6. Identificar archivos potencialmente afectados.
7. Identificar dependencias.
8. Evaluar riesgo de regresión.
9. Elaborar plan breve.

Durante implementación:

- respetar exclusivamente la spec;
- reutilizar patrones existentes;
- evitar modificaciones no relacionadas;
- detener cambios que excedan el scope.

Después:

1. Ejecutar tests aplicables.
2. Añadir tests definidos por la spec.
3. Validar cada Acceptance Criterion.
4. Reportar desviaciones.
5. Marcar IMPLEMENTED.
6. Marcar VERIFIED únicamente cuando todas las validaciones pasen.
