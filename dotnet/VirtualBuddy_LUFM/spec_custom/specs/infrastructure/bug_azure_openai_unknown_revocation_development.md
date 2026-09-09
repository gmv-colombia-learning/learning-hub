# Specification

## Metadata

- Title: Tolerar revocacion desconocida de Azure OpenAI en Development
- Type: BUG
- Module: Infrastructure / AI Integration
- Status: VERIFIED

## Summary

Permitir que Azure OpenAI funcione en `Development` cuando la inspeccion TLS corporativa de Netskope impide comprobar la revocacion, sin aceptar otros errores de certificado ni afectar otros clientes HTTP.

## Context

La generacion de embeddings falla antes de autenticar contra Azure OpenAI con `CRYPT_E_NO_REVOCATION_CHECK`. El diagnostico en la maquina de ejecucion confirma que Netskope sustituye el certificado publico de Azure por una cadena corporativa valida y confiable, pero tanto el certificado final como el intermedio carecen de AIA y CRL Distribution Points. Windows reporta exclusivamente `RevocationStatusUnknown` para ambos certificados.

La correccion definitiva corresponde a la politica corporativa de inspeccion TLS. Se requiere un workaround temporal y limitado al ambiente `Development` para continuar el prototipo.

## Scope

### In Scope

- Configurar un cliente HTTP exclusivo para Azure OpenAI en `Development`.
- Aceptar una cadena TLS solamente cuando su unico error sea `RevocationStatusUnknown`.
- Reutilizar ese cliente en chat y embeddings de Azure OpenAI.
- Mantener la validacion del hostname y de los demas estados de la cadena.
- Documentar el caracter temporal del workaround.

### Out of Scope

- Modificar Windows, Netskope, certificados instalados o politicas corporativas.
- Deshabilitar globalmente la validacion TLS o de revocacion.
- Aceptar certificados revocados, vencidos, aun no vigentes, no confiables o emitidos para otro hostname.
- Modificar los clientes de Azure SQL, Supabase, Resend u otros servicios HTTP.
- Cambiar la API key, deployments, modelos, dimension de embeddings o contratos de aplicacion.
- Aplicar el workaround en `Local` o en futuros ambientes distintos de `Development`.

## Current Behavior

- `Development` registra los conectores de Semantic Kernel para Azure OpenAI sin un cliente HTTP especifico.
- Schannel no puede determinar la revocacion de la cadena emitida por Netskope y cancela el handshake TLS.
- La indexacion captura el fallo y devuelve el documento persistido sin crear sus `KnowledgeChunks`.

## Expected Behavior

- Chat y embeddings de Azure OpenAI en `Development` usan un cliente HTTP aislado.
- Ese cliente continua cuando el unico defecto de la cadena es que la revocacion no puede determinarse.
- Cualquier otro error TLS continua rechazando la conexion.
- Los demas ambientes y clientes conservan la validacion vigente.

## Functional Requirements

### FR-001

`Development` debe proporcionar a ambos conectores de Azure OpenAI el mismo cliente HTTP con validacion TLS especifica.

### FR-002

La validacion especifica debe aceptar certificados validos sin errores y cadenas cuyo unico estado sea `RevocationStatusUnknown`.

### FR-003

La validacion especifica debe rechazar errores de hostname, certificados ausentes, cadenas ausentes y cualquier estado diferente de `RevocationStatusUnknown`.

### FR-004

`Local` no debe registrar ni utilizar el cliente HTTP del workaround.

## Business Rules

### BR-001

El workaround es temporal para el prototipo en `Development` y no sustituye la correccion de la politica TLS de Netskope.

## Inputs

- Ambiente de ejecucion `Development`.
- Certificado y cadena entregados por la negociacion TLS de Azure OpenAI.
- Errores TLS calculados por la plataforma.

## Outputs

- Decision de aceptar o rechazar el certificado para la conexion exclusiva de Azure OpenAI.

## Validations

- Sin errores TLS, el certificado se acepta.
- Con `RemoteCertificateNameMismatch` o `RemoteCertificateNotAvailable`, se rechaza.
- Con errores de cadena, todos los estados deben ser exactamente `RevocationStatusUnknown`.
- Una cadena sin estados de error no puede justificar la tolerancia si la plataforma reporto `RemoteCertificateChainErrors`.

## Errors

- Los errores TLS no tolerados deben conservar el comportamiento estandar del cliente y abortar la solicitud.
- No se deben registrar API keys, tokens, contenido documental ni detalles sensibles.

## Edge Cases

- Error combinado de revocacion desconocida y certificado vencido.
- Certificado emitido para un hostname diferente.
- Certificado o cadena nulos.
- Cadena que contiene varios elementos con el mismo estado `RevocationStatusUnknown`.
- Ambiente `Local` con Ollama.

## Dependencies

- `HttpClientHandler` y validacion X.509 de .NET.
- Microsoft Semantic Kernel y su conector Azure OpenAI.
- Configuracion de infraestructura por ambiente existente.

## Affected Flows

- Registro del Kernel de Semantic Kernel en `Development`.
- Chat con proyecto mediante Azure OpenAI.
- Generacion de embeddings durante busqueda e indexacion.

## Non-Functional Requirements

- Aplicar minimo privilegio y limitar la excepcion al cliente y ambiente afectados.
- No aceptar indiscriminadamente certificados.
- Mantener el codigo de validacion aislado y cubierto por pruebas unitarias.
- Mantener observabilidad mediante una advertencia cuando se tolere revocacion desconocida, sin datos sensibles.

## Acceptance Criteria

### AC-001

Given `Development` y una cadena sin errores TLS
When Azure OpenAI valida el certificado
Then la conexion se acepta.

### AC-002

Given `Development` y una cadena cuyo unico error es `RevocationStatusUnknown`
When Azure OpenAI valida el certificado
Then la conexion se acepta y se registra una advertencia segura.

### AC-003

Given `Development` y cualquier error TLS adicional o diferente
When Azure OpenAI valida el certificado
Then la conexion se rechaza.

### AC-004

Given el ambiente `Local`
When se construye el Kernel
Then Ollama conserva su cliente y no utiliza el workaround TLS.

### AC-005

Given `Development`
When se construye el Kernel
Then los conectores de chat y embeddings reciben el cliente HTTP exclusivo de Azure OpenAI.

## Required Tests

- Aceptacion sin errores TLS.
- Aceptacion de uno o varios estados exclusivamente `RevocationStatusUnknown`.
- Rechazo de hostname incorrecto, certificado ausente y cadena ausente.
- Rechazo de certificado revocado, vencido o con errores combinados.
- Resolucion del Kernel en `Local` y `Development`.
- Suite automatizada aplicable.
- Smoke test real de embeddings en `Development` cuando Azure OpenAI este disponible.

## Open Questions

- Ninguna.

## Implementation Notes

- `HttpClientHandler.ServerCertificateCustomValidationCallback` permite conservar los errores calculados por la plataforma y decidir solo sobre el caso conocido.
- No usar `DangerousAcceptAnyServerCertificateValidator` ni desactivar la comprobacion global de certificados.

## Implementation Progress

- Se registro un `HttpClient` exclusivo y scoped para Azure OpenAI solamente en `Development`.
- Chat y embeddings reutilizan el mismo cliente administrado por inyeccion de dependencias.
- El callback acepta certificados sin errores o cadenas cuyo unico estado sea `RevocationStatusUnknown`.
- Se rechazan certificados ausentes, cadenas ausentes, hostname incorrecto, revocacion confirmada, vigencia invalida, raiz no confiable y estados combinados.
- Las redirecciones automaticas estan deshabilitadas para impedir que la excepcion TLS se extienda a otro destino.
- La tolerancia registra una unica advertencia segura por proceso.
- Las 26 pruebas enfocadas de validacion TLS y configuracion de infraestructura pasan.
- La regresion aplicable, excluyendo los tres tests preexistentes de Resend, supera 102 de 102 pruebas.
- El smoke test real genero un embedding de 768 dimensiones y obtuvo una respuesta de chat mediante Azure OpenAI desde la maquina afectada por Netskope.
- Todos los criterios de aceptacion fueron comprobados; la spec queda `VERIFIED`.

## Change Log

- 2026-09-09: Bug diagnosticado como incompatibilidad entre la comprobacion de revocacion de Schannel y certificados emitidos por la inspeccion TLS de Netskope sin AIA ni CRL Distribution Points.
- 2026-09-09: El usuario aprobo iniciar un workaround limitado a `Development`; spec creada y marcada `READY` sin preguntas funcionales abiertas.
- 2026-09-09: Implementacion iniciada contra la spec `READY`.
- 2026-09-09: Implementacion endurecida para rechazar cadenas ausentes y redirecciones automaticas.
- 2026-09-09: Pruebas automatizadas y smoke tests reales de embeddings y chat superados; spec marcada `VERIFIED`.
