# Project Agent Instructions

Este proyecto trabaja utilizando Spec-Driven Development (SDD).

## Fuentes de verdad

- Reglas obligatorias del agente: `aidev/rules_agent.md`.
- Reglas y convenciones de desarrollo: `aidev/dev_rules.md`.
- Contexto general del proyecto: `aidev/context/`.
- Contexto o descripción del proyecto: `aidev/context/desc.md`.
- Contexto o descripción del diseño y arquitectura: `aidev/context/design-arch.md`.
- Referencias visuales, no assets de producción: `aidev/context/ui/`.
- Especificaciones: `aidev/specifications/`.

Las rutas anteriores son relativas a `frontent/VirtualBuddy_LUFM/VirtualBuddyFront_LUFM`, que es el único proyecto ejecutable del repositorio. La escritura `frontent` es la ruta real; no renombrarla como limpieza incidental.

## Protocolo obligatorio

Para cualquier solicitud relacionada con una feature, bug, módulo, cambio funcional o implementación:

1. Aplicar las reglas de `aidev/rules_agent.md` y `aidev/dev_rules.md`.
2. Identificar el dominio, entidad, módulo o flujo afectado.
3. Consultar las specs existentes.
4. Consultar únicamente el contexto del proyecto que sea relevante.
5. No inventar requisitos faltantes.
6. No modificar código antes de tener una spec válida y lista para implementar.
7. No ampliar el alcance de una spec.
8. No eliminar código existente sin evaluar su impacto y respetar las reglas del proyecto.
9. No afectar otros flujos fuera del alcance sin informar al usuario.
10. Validar la implementación contra la spec al finalizar.
11. Realizar recomendaciones con criterio de Senior Developer, Arquitecto, QA y Analista de Requisitos, explicándolas de forma comprensible tanto para perfiles senior como junior y esperando aprobación cuando cambien requisitos, alcance o decisiones no definidas.

Las principales intenciones de trabajo son:

- `FEATURE`: funcionalidad sobre un módulo o dominio existente.
- `BUG`: corrección de un comportamiento existente.
- `MODULE`: creación o definición de un nuevo módulo o dominio, por ejemplo `customer/cliente` o `bill/factura`.
- `IMPLEMENT`: implementación de una spec aprobada.

## Regla principal

La spec define QUÉ debe hacerse.

Las reglas de desarrollo definen CÓMO debe implementarse.

Nunca utilizar decisiones técnicas como sustituto de requisitos funcionales faltantes. Recomendar las mejores prácticas o decisiones pertinentes y solicitar aprobación antes de incorporarlas al alcance.

Priorizar APIs modernas y estables compatibles con la versión instalada del framework. No conservar patrones obsoletos por inercia ni adoptar APIs experimentales o dependencias nuevas sin una ventaja verificable y, cuando afecten arquitectura o alcance, sin aprobación.

## Contexto operativo

- Ejecutar todos los comandos desde `frontent/VirtualBuddy_LUFM/VirtualBuddyFront_LUFM`; no existe un runner en la raíz.
- Usar npm y el `package-lock.json`: instalación `npm ci`, desarrollo `npm start` y build estricto `npm run build`.
- Suite completa sin watch: `npm test -- --watch=false`. Un spec: `npm test -- --watch=false --include src/app/ruta/example.spec.ts`. Por nombre: `npm test -- --watch=false --filter "^Nombre"`.
- Formato: `npx prettier --check "src/**/*.{ts,html,scss}"`; sustituir `--check` por `--write` para corregir.
- Verificación final de código Angular: formato, build de producción y suite completa sin watch. No existe runner e2e ni script separado de lint o typecheck.
- No versionar `node_modules/`, `dist/`, `coverage/`, `out-tsc/` ni `.angular/cache/`.
- Todo cambio se integra mediante Pull Request; no realizar push directo a `main`.
