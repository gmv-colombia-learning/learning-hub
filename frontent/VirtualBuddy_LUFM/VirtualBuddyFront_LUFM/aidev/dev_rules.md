# Reglas de desarrollo frontend

Estas reglas definen CÓMO implementar una spec aprobada. La arquitectura de referencia está en `aidev/context/design-arch.md`; ninguna regla técnica completa requisitos funcionales ausentes.

## Arquitectura obligatoria

- Organizar el código por dominio o capacidad de negocio dentro de `src/app/features/<dominio>/`, no por tipos técnicos globales.
- Cada feature puede contener `domain/`, `application/`, `infrastructure/` y `ui/`, pero crear únicamente las capas y archivos que tengan una responsabilidad real.
- Dirección de dependencias: `ui -> application -> domain`; `infrastructure` implementa puertos de `application` o `domain`. El dominio no depende de Angular, RxJS, HTTP, almacenamiento, DTOs ni componentes.
- `core/` queda reservado para capacidades técnicas singleton y transversales, como configuración HTTP, autenticación de sesión, guards e interceptores.
- `shared/` contiene UI, utilidades y tipos realmente reutilizables y sin reglas de un dominio. No usarlo como carpeta de código sin dueño.
- Un dominio no importa internals de otro. La colaboración entre dominios se realiza mediante contratos públicos mínimos o desde una capa de composición.
- No ejecutar una migración arquitectónica masiva. Al tocar código existente, adaptar solamente lo requerido por la spec y registrar cualquier deuda fuera de alcance.

## Adopción tecnológica

- Preferir las APIs estables más modernas disponibles en la versión instalada de Angular y TypeScript cuando simplifiquen el diseño, mejoren la seguridad de tipos o reduzcan errores.
- No mantener un patrón obsoleto únicamente por consistencia con código antiguo. Aplicar la alternativa moderna dentro del alcance y proponer por separado cualquier migración adicional.
- No usar APIs experimentales o developer preview en flujos críticos sin evaluar estabilidad, soporte, pruebas y estrategia de reemplazo, y sin aprobación cuando cambien una decisión arquitectónica.
- No agregar librerías para resolver capacidades ya cubiertas adecuadamente por Angular, TypeScript o RxJS. Toda dependencia nueva requiere una necesidad concreta y una evaluación de mantenimiento, seguridad, tamaño y compatibilidad.
- Favorecer soluciones simples y explícitas. “Más moderno” no significa más abstracciones, más dependencias ni adoptar una novedad sin beneficio verificable.

## Dominio y aplicación

- Modelar con el lenguaje del negocio definido por contexto y specs; evitar nombres técnicos cuando exista un concepto de dominio claro.
- Las invariantes y transiciones de estado pertenecen a entidades, Value Objects o funciones puras de dominio, no a componentes, templates ni adaptadores HTTP.
- Usar Value Objects solo cuando un concepto tenga validación, normalización o comportamiento propio; no envolver primitivas sin beneficio concreto.
- Los casos de uso de `application/` coordinan una intención del usuario y dependen de puertos, no de implementaciones HTTP o de almacenamiento.
- Separar modelos de dominio, View Models y DTOs cuando representen responsabilidades diferentes. Todo DTO externo debe mapearse en `infrastructure/`.
- No inventar entidades, estados, permisos, validaciones ni casos de uso por completar la arquitectura.

## Angular y estado

- Usar componentes standalone; configurar providers globales en `app.config.ts` y rutas raíz en `app.routes.ts`. No introducir NgModules.
- Cargar features por rutas lazy cuando constituyan pantallas o flujos independientes. Los providers específicos deben vivir en el límite de ruta o feature.
- Mantener páginas como orquestadores de UI y componentes presentacionales enfocados. Ningún componente debe contener reglas de negocio ni acceso HTTP directo.
- Aplicar Signals-first en la UI: exponer estado de presentación mediante `signal`, entradas con `input`, estado derivado con `computed` y, cuando corresponda, consultas convertidas con `toSignal`.
- Reservar Observables para fuentes y transformaciones asíncronas como HTTP, eventos del router, WebSockets, cancelación, debounce, combinación y reintentos. Signals no reemplaza esas capacidades de RxJS.
- Convertir Observable a Signal una sola vez en el límite de presentación o en `ui/state/`; no repetir `toSignal` sobre el mismo flujo ni alternar innecesariamente entre ambos modelos.
- Usar `toObservable` únicamente cuando un Signal deba entrar en un pipeline RxJS que necesite operadores asíncronos. Evitar conversiones de ida y vuelta sin una responsabilidad clara.
- Evitar `subscribe()` en componentes. Preferir `toSignal`, `async` pipe o composición declarativa; si una suscripción imperativa es inevitable, encapsularla en el límite responsable y cancelarla con `takeUntilDestroyed`.
- Prohibir suscripciones anidadas; componer dependencias asíncronas con operadores como `switchMap`, `concatMap`, `exhaustMap` o `forkJoin` según la semántica definida por la spec.
- Evitar estado duplicado; derivarlo con `computed`. Usar `effect` solo para efectos secundarios externos, nunca para sincronizar Signals ni propagar estado derivable.
- Mantener mutaciones explícitas: una acción de escritura delega a aplicación, representa estados de ejecución y actualiza o invalida el estado afectado sin ocultar efectos secundarios.
- Usar `ChangeDetectionStrategy.OnPush`, control flow moderno (`@if`, `@for`, `@switch`) y `track` estable en colecciones.
- Preferir `inject`, `input`, `output` y queries basadas en Signals frente a APIs legacy cuando la API estable instalada cubra el caso.
- Preferir formularios reactivos para formularios con validación o comportamiento. Los mensajes y reglas visibles deben provenir de la spec.
- Mantener accesibilidad semántica, navegación por teclado, foco visible y diseño responsive. Las imágenes de `aidev/context/ui/` son referencias visuales; no copiarlas a `public/` ni tratarlas como assets de runtime salvo que una spec lo exija.

## Infraestructura y errores

- Encapsular `HttpClient`, almacenamiento y APIs del navegador en adaptadores de `infrastructure/`; exponerlos mediante puertos o servicios de aplicación.
- Centralizar URL base, autenticación y preocupaciones HTTP transversales; no repetirlas en componentes o repositorios.
- Traducir errores técnicos a errores comprensibles para aplicación/UI sin ocultar la causa útil para diagnóstico.
- No inventar contratos de backend ni respuestas exitosas. Si no están definidos por una spec o contrato verificable, solicitar aclaración.

## Estructura y nombres

- Mantener specs `*.spec.ts` junto al archivo probado y estilos/templates junto al componente.
- Usar nombres por intención: `<accion>.use-case.ts`, `<entidad>.repository.ts`, `<entidad>.http-repository.ts`, `<nombre>-page.ts` y `<nombre>.component.ts` cuando corresponda.
- Exponer solo el contrato necesario de una feature. Evitar barrel files globales y dependencias circulares.
- Aplicar SOLID de forma pragmática: no crear interfaces, servicios, facades o abstracciones con una sola implementación si no protegen un límite real.

## Pruebas y calidad

- Traducir cada criterio de aceptación de la spec en pruebas observables o en una validación explícita documentada.
- Probar reglas de dominio con tests unitarios puros; casos de uso con puertos falsos; componentes mediante comportamiento visible, no detalles internos.
- Probar estado derivado como comportamiento observable y usar marble tests solo cuando la complejidad temporal de un flujo RxJS los justifique.
- Cubrir según el alcance: happy path, validaciones, errores, permisos, estados vacíos/carga y regresiones del flujo modificado.
- Durante el desarrollo ejecutar el spec enfocado. Antes de finalizar ejecutar, en orden: Prettier, `npm run build` y `npm test -- --watch=false`.
- No corregir fallos preexistentes fuera del alcance sin aprobación; reportarlos distinguiéndolos de regresiones introducidas.

## Criterio de decisión

- Primero seguir la spec, luego estas reglas y después los patrones válidos existentes.
- Si el código existente contradice esta arquitectura, no copiar el problema ni refactorizar todo: proponer la adaptación mínima compatible con la spec.
- Documentar como recomendación cualquier mejora que cambie alcance, contrato, experiencia de usuario o arquitectura aprobada, y esperar decisión antes de implementarla.
