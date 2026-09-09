# Specification

## Metadata

- Title: Manejo transversal y observable de excepciones y errores
- Type: FEATURE
- Module: API / Common Error Handling
- Status: READY

## Summary

Fortalecer el manejo de excepciones de todos los procesos de la aplicación para que cada fallo pueda diagnosticarse, sin introducir formatos de error nuevos, duplicar registros ni cambiar el resultado funcional de los flujos existentes.

## Context

La API ya dispone de `ExceptionMiddleware`, excepciones tipadas y respuestas `ProblemDetails`, pero la observabilidad no es uniforme. Todas las excepciones HTTP se registran actualmente como `Error`, algunos procesos consumen excepciones sin registrarlas y ciertas traducciones reemplazan la excepción original sin conservar su causa.

La necesidad es conocer por qué falló cualquier proceso, incluidos los procesos HTTP, de inicio, integración, compensación y operaciones secundarias tolerantes a fallos. Esto no implica agregar `try/catch` indiscriminadamente: cada excepción debe alcanzar un límite responsable que la registre o debe registrarse en el punto donde se consume o traduce.

## Scope

### In Scope

- Fortalecer el `ExceptionMiddleware` como límite global de errores HTTP no controlados.
- Mantener `ProblemDetails` como formato estándar de error HTTP.
- Incluir un `traceId` correlacionable en cada `ProblemDetails` generado por el middleware y en su registro asociado.
- Clasificar el nivel y detalle del log según el tipo e impacto de la excepción.
- Garantizar que toda excepción capturada sea propagada hasta un límite que la registre, o registrada donde sea consumida o traducida.
- Hacer observables los fallos actualmente consumidos de forma silenciosa, incluidos los fallos de parsing de Word y Excel.
- Mantener observables los fallos de operaciones secundarias que, por especificación, no deben hacer fallar la operación principal.
- Conservar la causa técnica de una excepción cuando se traduzca a otra excepción segura, mediante la excepción interna o un registro seguro en el límite de traducción.
- Mantener el registro crítico y la propagación de los fallos de inicialización.
- Tratar correctamente las cancelaciones confirmadas por el cliente y las respuestas HTTP que ya hayan comenzado.
- Probar los mapeos, niveles, trazabilidad, protección de datos y regresiones del mecanismo transversal.

### Out of Scope

- Cambiar los estados HTTP o mensajes funcionales que ya producen los flujos existentes.
- Normalizar en esta feature los `BadRequest` directos que devuelven texto.
- Reemplazar el `Result` local del flujo de tecnologías.
- Cambiar los contratos de autenticación o autorización.
- Convertir los `KeyNotFoundException` actuales del módulo Document a otro tipo o estado HTTP.
- Cambiar el comportamiento best-effort de la indexación de documentos o del aviso posterior al restablecimiento de contraseña.
- Agregar reintentos, circuit breakers o nuevas políticas de resiliencia.
- Incorporar un catálogo de códigos de error.
- Crear un segundo middleware, filtros de excepciones o una abstracción `Result` global.
- Registrar cada lanzamiento de excepción en todas las capas cuando la excepción ya se propagará hasta un límite responsable.

## Current Behavior

- `ExceptionMiddleware` convierte excepciones tipadas en `ProblemDetails` con estados 400, 404, 409, 429 y 503; las demás se convierten en 500.
- Todas las excepciones capturadas por el middleware se registran como `Error`, incluidas las validaciones y condiciones de negocio esperadas.
- Las respuestas del middleware contienen `status`, `title`, `detail` e `instance`, pero no un identificador correlacionable con logs.
- El detalle real de una excepción 500 solo se expone en `Development`.
- Los parsers de Word y Excel consumen cualquier excepción y devuelven texto vacío sin registrar la causa.
- Algunos procesos secundarios registran el tipo de excepción y continúan para conservar el resultado principal, conforme a sus specs.
- Algunas excepciones de infraestructura se traducen a mensajes seguros, pero la causa original puede perderse.
- Los fallos de inicialización de base de datos se registran como críticos y se propagan para detener el inicio.
- Una cancelación del cliente puede llegar al manejo genérico y tratarse como 500.
- El middleware no define explícitamente qué hacer si la respuesta HTTP ya comenzó.

## Expected Behavior

- Ninguna excepción capturada por la aplicación debe quedar silenciosa.
- Cada fallo debe tener un único límite principal de observabilidad con contexto suficiente para identificar proceso, tipo de excepción y correlación disponible.
- Las excepciones propagadas hasta el middleware deben registrarse allí; no requieren registros repetidos en todas las capas intermedias.
- Las excepciones consumidas para mantener exitoso un flujo, o traducidas de forma que se pierda su causa, deben registrarse de forma segura en el punto de captura.
- Los contratos y resultados funcionales existentes deben conservarse.
- El cliente debe poder proporcionar el `traceId` recibido para localizar el evento correspondiente en logs.

## Functional Requirements

### FR-001

El sistema debe utilizar el mecanismo existente de excepciones tipadas, `ExceptionMiddleware` y `ProblemDetails` para los errores HTTP que atraviesen el pipeline.

### FR-002

Cada `ProblemDetails` generado por el middleware debe incluir un campo `traceId` no vacío y el registro asociado debe contener el mismo valor.

### FR-003

El middleware debe conservar los mapeos actuales: `ValidationException` a 400, `NotFoundException` a 404, `ConflictException` a 409, `TooManyRequestsException` a 429, `TemporaryServiceUnavailableException` a 503 y cualquier excepción no reconocida a 500.

### FR-004

Las excepciones que producen respuestas 4xx deben registrarse como `Warning`, incluyendo como mínimo tipo de excepción, estado, ruta y `traceId`, sin stack trace.

### FR-005

Las excepciones que producen respuestas 5xx deben registrarse como `Error`, incluyendo tipo de excepción, estado, ruta, `traceId` y stack trace suficiente para diagnóstico, sin exponer información sensible conocida.

### FR-006

Fuera de `Development`, una excepción inesperada debe producir el detalle genérico vigente. Solo en `Development` puede exponerse `exception.Message` en el detalle de una respuesta 500.

### FR-007

Cuando una `OperationCanceledException` corresponda a una cancelación confirmada por `HttpContext.RequestAborted`, no debe convertirse en una respuesta 500. Debe registrarse como `Information` con la ruta y el `traceId`, sin intentar escribir una respuesta de error para un cliente desconectado.

### FR-008

Una `OperationCanceledException` que no corresponda a `RequestAborted` debe conservar el tratamiento de excepción inesperada.

### FR-009

Si la respuesta HTTP ya comenzó, el middleware no debe sobrescribir ni anexar un segundo cuerpo `ProblemDetails`. La excepción debe permanecer observable y debe respetarse el manejo seguro del pipeline.

### FR-010

Toda excepción capturada en cualquier proceso debe cumplir al menos una de estas condiciones: propagarse hasta un límite que la registre; registrarse donde se consume para continuar; o conservarse como causa interna al traducirse y quedar registrada posteriormente.

### FR-011

Los bloques `catch` que mantienen exitoso un proceso principal deben registrar el fallo secundario con su operación, tipo de excepción y correlación disponible, sin cambiar el resultado definido por la spec del flujo.

### FR-012

Los procesos de parsing de Word y Excel deben registrar los fallos de extracción antes de conservar su resultado actual de texto vacío. El registro no debe incluir el contenido del documento.

### FR-013

Cuando una excepción técnica se traduzca a una excepción segura para el cliente, la causa original debe preservarse como excepción interna o registrarse de forma segura en el límite de traducción. El cliente solo debe recibir el mensaje seguro vigente.

### FR-014

Los fallos de inicio que impiden operar a la aplicación deben registrarse como `Critical` y propagarse para impedir un inicio parcial, conservando el comportamiento actual.

### FR-015

La implementación debe evitar registros duplicados de la misma excepción en capas consecutivas. Solo se permite un registro adicional cuando aporte contexto operativo que el límite final no pueda reconstruir, o cuando la excepción vaya a ser consumida o traducida perdiendo su causa.

## Business Rules

### BR-001

Observabilidad no significa alterar el éxito o fracaso funcional de un flujo. Una operación secundaria definida como best-effort debe continuar siendo best-effort.

### BR-002

Una excepción no debe capturarse únicamente para ocultarla, devolver un valor sustituto o cambiar su mensaje sin dejar evidencia diagnóstica segura.

### BR-003

No se deben agregar bloques `try/catch` a métodos que no puedan recuperar el flujo, aportar contexto necesario o traducir la excepción. En esos casos, la excepción debe propagarse naturalmente.

### BR-004

Los mensajes dirigidos al cliente y los datos registrados deben respetar las restricciones de seguridad de las specs de cada módulo.

## Inputs

- Excepciones tipadas de dominio y aplicación.
- Excepciones no controladas de aplicación, infraestructura y proveedores.
- Excepciones de procesos secundarios y compensaciones.
- Excepciones de inicio de la aplicación.
- Estado de `HttpContext.RequestAborted`.
- Identificador de actividad o `HttpContext.TraceIdentifier` disponible.

## Outputs

- El mismo contrato `ProblemDetails` vigente, extendido con `traceId` cuando sea generado por el middleware.
- Logs estructurados con severidad y contexto acordes al impacto.
- Procesos no HTTP con un registro correlacionable mediante el contexto disponible de la operación.
- Conservación de los resultados funcionales existentes.

## Validations

- El `traceId` de la respuesta debe coincidir con el registrado para la misma excepción.
- El estado indicado en `ProblemDetails` debe coincidir con el estado HTTP.
- Los errores 500 fuera de `Development` no deben exponer el mensaje interno.
- Los logs no deben incorporar cuerpos de request, tokens, contraseñas, credenciales, códigos de recuperación ni contenido de documentos.
- Una excepción consumida o traducida no puede quedar sin registro ni causa preservada.
- La misma excepción no debe generar registros redundantes sin contexto adicional justificable.

## Errors

- 400 `Validation Error`: conserva el detalle funcional vigente.
- 404 `Resource Not Found`: conserva el detalle funcional vigente.
- 409 `Business Rule Violation`: conserva el detalle funcional vigente.
- 429 `Too Many Requests`: conserva el detalle funcional vigente.
- 503 `Service Temporarily Unavailable`: conserva el detalle seguro vigente.
- 500 `Internal Server Error`: conserva el detalle real solo en `Development`; en los demás ambientes utiliza el mensaje genérico vigente.
- Cancelación confirmada por el cliente: no se convierte en 500 ni intenta generar un cuerpo de error para el cliente desconectado.

## Edge Cases

- Excepción mientras la respuesta todavía no comenzó.
- Excepción después de iniciar headers o cuerpo de respuesta.
- Solicitud cancelada por el cliente.
- `OperationCanceledException` sin cancelación de `RequestAborted`.
- Excepción durante una compensación posterior a un fallo principal.
- Excepción en una operación best-effort después de completar la operación principal.
- Excepción de proveedor con datos potencialmente sensibles en su mensaje.
- Ausencia de una actividad de diagnóstico activa, utilizando el identificador HTTP disponible.
- Excepción traducida varias capas antes de alcanzar el límite HTTP.
- Fallo durante el inicio antes de aceptar solicitudes.

## Dependencies

- `VirtualBuddy.Api.Middleware.ExceptionMiddleware`.
- `Microsoft.AspNetCore.Mvc.ProblemDetails`.
- `Microsoft.Extensions.Logging`.
- Excepciones de `VirtualBuddy.Domain.Common.Exceptions`.
- Trazabilidad proporcionada por ASP.NET Core y `System.Diagnostics.Activity`.
- Procesos de inicio en `Program` y `DatabaseStartup`.
- Procesos best-effort definidos en las specs de Auth y Document.

## Affected Flows

- Solicitudes HTTP de Auth, Project, Document, Technology y AI que propaguen excepciones.
- Inicialización y validación de base de datos.
- Integraciones con almacenamiento, email, IA y persistencia.
- Parsing e indexación de documentos.
- Compensación de carga y eliminación de imágenes de proyecto.
- Cualquier proceso actual o futuro que capture una excepción dentro de la aplicación.

## Non-Functional Requirements

- Seguridad: no registrar ni responder con secretos, credenciales, tokens, contraseñas, códigos de recuperación, cuerpos completos ni contenido documental.
- Observabilidad: cada excepción capturada debe poder asociarse con el proceso que falló y con la correlación disponible.
- Mantenibilidad: debe existir un único mecanismo principal por tipo de límite; no se deben combinar middleware y filtros para el mismo propósito.
- Rendimiento: no serializar cuerpos ni objetos de dominio completos para construir logs de excepción.
- Compatibilidad: preservar estados, títulos, detalles y resultados funcionales existentes, salvo la adición de `traceId` al `ProblemDetails` del middleware.
- Simplicidad: no introducir una jerarquía nueva, un catálogo de códigos ni captura repetitiva en cada método.

## Acceptance Criteria

### AC-001

Given una excepción tipada que alcanza el middleware
When se genera la respuesta HTTP
Then conserva su estado, título y detalle vigentes, incluye un `traceId` no vacío y registra el mismo identificador.

### AC-002

Given una excepción que produce un estado 4xx
When el middleware la captura
Then la registra como `Warning` con tipo, estado, ruta y `traceId`, sin stack trace.

### AC-003

Given una excepción que produce un estado 5xx
When el middleware la captura
Then la registra como `Error` con tipo, estado, ruta, `traceId` y stack trace diagnóstico, sin datos sensibles conocidos.

### AC-004

Given una excepción inesperada fuera de `Development`
When el middleware genera un 500
Then el cliente recibe el mensaje genérico vigente y un `traceId`, sin recibir el mensaje interno ni el stack trace.

### AC-005

Given una excepción inesperada en `Development`
When el middleware genera un 500
Then el detalle puede contener `exception.Message`, incluye `traceId` y nunca contiene el stack trace.

### AC-006

Given una `OperationCanceledException` y `RequestAborted` cancelado
When el middleware la captura
Then no genera un 500, no intenta escribir `ProblemDetails` y registra la cancelación como `Information` con su `traceId`.

### AC-007

Given una excepción después de comenzar la respuesta HTTP
When alcanza el middleware
Then no se agrega ni reemplaza el cuerpo con `ProblemDetails` y el fallo permanece registrado.

### AC-008

Given un fallo al extraer texto de Word o Excel
When el parser conserva su resultado actual de texto vacío
Then registra el tipo de excepción y la operación sin registrar el contenido del documento.

### AC-009

Given una operación secundaria definida como best-effort
When esa operación lanza una excepción
Then el proceso principal conserva el resultado establecido por su spec y el fallo secundario queda registrado con contexto seguro.

### AC-010

Given una excepción técnica traducida a una excepción segura
When la excepción traducida alcanza su límite final
Then la causa original puede diagnosticarse mediante la excepción interna o mediante un registro seguro realizado al traducirla, sin exponerse al cliente.

### AC-011

Given una excepción que puede propagarse hasta un límite responsable
When atraviesa capas intermedias que no pueden recuperarla ni aportar contexto necesario
Then no se generan logs duplicados ni bloques `try/catch` innecesarios.

### AC-012

Given un fallo que impide inicializar la aplicación
When ocurre durante la validación o seed de base de datos
Then se registra como `Critical`, se propaga y la aplicación no continúa en estado parcial.

### AC-013

Given cualquiera de los flujos existentes de Auth, Project, Document o AI
When no ocurre una excepción nueva
Then su respuesta y comportamiento permanecen sin cambios por esta feature.

## Required Tests

- Mapeo de 400, 404, 409, 429, 503 y 500.
- Contenido y tipo `application/problem+json` de cada respuesta del middleware.
- Presencia y correlación de `traceId` entre respuesta y log.
- Nivel `Warning` y ausencia de stack trace para 4xx.
- Nivel `Error` y diagnóstico de excepción para 5xx.
- Detalle de 500 en `Development` y redacción en los demás ambientes.
- Cancelación confirmada por `RequestAborted` sin respuesta 500.
- `OperationCanceledException` no asociada al cliente tratada como inesperada.
- Respuesta ya iniciada sin segundo cuerpo de error.
- Registro seguro de fallos de parsing de Word y Excel conservando texto vacío.
- Registro de fallos best-effort sin cambiar el éxito de carga de documentos ni del restablecimiento de contraseña.
- Conservación o registro de la causa al traducir excepciones técnicas.
- Registro crítico y propagación de fallos de inicio.
- Ausencia de datos sensibles en respuestas y eventos verificables.
- Ausencia de logs duplicados en el recorrido normal de una misma excepción.
- Regresión de las suites existentes de Auth, Project, Document, AI e infraestructura.

## Open Questions

- Ninguna.

## Implementation Notes

- Reutilizar `Activity.Current?.Id` cuando exista y usar `HttpContext.TraceIdentifier` como respaldo.
- Centralizar el mapeo y la decisión de severidad dentro del mecanismo HTTP existente, sin agregar filtros paralelos.
- Auditar los bloques `catch` actuales. No agregar logs locales cuando la excepción se propaga intacta y el middleware o el límite de inicio ya aporta contexto suficiente.
- Los fallos consumidos deben registrarse en su punto de captura porque no alcanzarán otro límite.
- Para traducciones, preferir preservar `InnerException` cuando no acople el dominio a detalles de infraestructura; en caso contrario, registrar la causa en el límite que conoce la integración.
- Si `HttpResponse.HasStarted` es verdadero, no intentar serializar `ProblemDetails`.
- La mejora del parser solo debe agregar observabilidad; su retorno de texto vacío y el comportamiento best-effort de indexación permanecen sin cambios.

## Change Log

- 2026-09-09: Feature identificada como capacidad transversal de `API / Common Error Handling`.
- 2026-09-09: Se confirmó fortalecer el mecanismo actual sin normalizar ni romper contratos existentes.
- 2026-09-09: Se confirmó agregar únicamente `traceId`, sin catálogo de códigos de error.
- 2026-09-09: Se definieron logs `Warning` para 4xx y `Error` para 5xx, con diagnóstico selectivo y protección de información sensible.
- 2026-09-09: Se confirmó que solo `Development` puede exponer el mensaje interno de una excepción 500.
- 2026-09-09: Se confirmó que una cancelación del cliente no debe tratarse como 500.
- 2026-09-09: El alcance se amplió para impedir excepciones capturadas silenciosamente en cualquier proceso, sin agregar captura o logging duplicado en cada capa.
- 2026-09-09: Spec completada sin preguntas funcionales abiertas y marcada `READY`.
