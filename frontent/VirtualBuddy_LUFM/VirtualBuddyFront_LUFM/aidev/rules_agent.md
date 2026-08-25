# Agent Development Rules

## 1. Scope

- No implementar comportamiento fuera de las specifactions.
- No ampliar requerimientos implícitamente.
- No realizar refactors no solicitados.
- No modificar módulos no relacionados salvo necesidad demostrada.

## 2. Existing code

- No eliminar código existente sin evaluar su propósito e impacto.
- No asumir que código aparentemente no utilizado puede eliminarse.
- Antes de modificar un flujo existente, identificar consumidores,
  dependencias y posibles regresiones.
- Finalmente verificar todos los factores para realizar siempre un desarrollo, actuando como profesional y un Senior developer.

## 3. Missing information

Clasificar la información encontrada como:

### KNOWN

Información explícita proporcionada por:

- usuario
- spec
- contexto oficial del proyecto

Puede utilizarse directamente.

### INFERRED

Información deducida del:

- código
- arquitectura
- otras specs

No convertirla automáticamente en requisito.
Cuando sea relevante al comportamiento funcional, solicitar confirmación.

### UNKNOWN

Información necesaria que no está disponible.

Si afecta comportamiento funcional, reglas de negocio,
persistencia, permisos, errores o criterios de aceptación:
preguntar al usuario antes de continuar.

## 4. No invention

- No inventar reglas de negocio.
- No inventar entidades.
- No inventar validaciones.
- No inventar estados.
- No inventar permisos.
- No inventar errores.
- No inventar integraciones.
- No inventar requisitos visuales.

## 5. Impact protection

Si implementar una spec requiere modificar un flujo que
no está contemplado por ella:

1. Detener esa modificación.
2. Informar qué flujo sería afectado.
3. Explicar por qué.
4. Solicitar decisión al usuario.

## 6. Specs

- Al finalizar, validar código y tests contra cada criterio de aceptación.

## 7. Communication

Cuando falte información:

- preguntar solamente lo necesario;
- no realizar grandes cuestionarios de una sola vez;
- aprovechar primero información existente en proyecto y código.
- Brindar recomendaciones pero esperar aprobación siempre.
