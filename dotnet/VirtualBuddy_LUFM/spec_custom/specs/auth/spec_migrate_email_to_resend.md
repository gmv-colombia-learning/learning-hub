# Specification

## Metadata

- Title: Migrar el envio de correos de recuperacion a Resend
- Type: FEATURE
- Module: Auth
- Status: IMPLEMENTED

## Summary

Reemplazar la implementacion SMTP del puerto `IEmailSender` por una integracion con la API de Resend, sin modificar el comportamiento funcional del flujo de recuperacion de contrasena.

## Context

El flujo de recuperacion ya envia el codigo temporal y el aviso de cambio mediante una implementacion SMTP de infraestructura. Se requiere utilizar Resend como proveedor de correo y conservar la separacion definida por Clean Architecture.

## Scope

### In Scope

- Reemplazar `SmtpEmailSender` por una implementacion de `IEmailSender` basada en la API HTTPS de Resend.
- Configurar API key, email remitente y nombre remitente por entorno.
- Actualizar el registro de dependencias y el ejemplo de configuracion.
- Mantener los dos mensajes y las reglas de manejo de fallos existentes.

### Out of Scope

- Cambiar endpoints, DTOs, codigos, vigencia, limites o reglas de restablecimiento.
- Cambiar el contenido funcional o idioma de los correos.
- Agregar plantillas administradas en Resend, webhooks, tracking o reintentos.
- Almacenar una API key real en el repositorio.

## Current Behavior

- `IEmailSender` se implementa mediante SMTP con host, puerto y credenciales configurables.

## Expected Behavior

- Los correos del flujo se envian mediante la API HTTPS de Resend.
- Los casos de uso y el servicio de recuperacion continúan dependiendo solo de `IEmailSender`.

## Functional Requirements

### FR-001

- El codigo de recuperacion debe enviarse mediante Resend conservando destinatario, asunto, contenido y vigencia informada actualmente.

### FR-002

- El aviso posterior al cambio de contrasena debe enviarse mediante Resend conservando destinatario, asunto y contenido actuales.

### FR-003

- Una respuesta no exitosa de Resend debe considerarse un fallo de envio y propagarse al flujo existente.

## Business Rules

### BR-001

- Se mantienen sin cambios todas las reglas de `spec_forgot_password.md`, sustituyendo las referencias al transporte SMTP por Resend.

## Inputs

- Destinatario, asunto y cuerpo producidos por el flujo existente.
- API key, email remitente y nombre remitente configurados por entorno.

## Outputs

- Solicitud aceptada por Resend o fallo de envio propagado al consumidor.

## Validations

- La API key y el email remitente son obligatorios al iniciar la aplicacion.
- El nombre remitente debe disponer de un valor predeterminado no vacio.

## Errors

- Una respuesta HTTP no exitosa de Resend debe producir una excepcion de infraestructura sin incluir la API key en mensajes ni logs.
- Se conserva el manejo actual del fallo inicial y del fallo del aviso posterior.

## Edge Cases

- Configuracion incompleta.
- Cancelacion de la solicitud HTTP.
- Respuesta HTTP no exitosa de Resend.

## Dependencies

- API HTTPS de Resend.
- `IEmailSender` de la capa Application.

## Affected Flows

- Solicitud de recuperacion de contrasena.
- Aviso posterior al restablecimiento exitoso.
- Configuracion de infraestructura.

## Non-Functional Requirements

- La API key no debe almacenarse en el repositorio ni escribirse en logs.
- La integracion debe utilizar HTTPS y respetar `CancellationToken`.
- La dependencia de Resend debe permanecer dentro de Infrastructure.

## Acceptance Criteria

### AC-001

Given una cuenta registrada y configuracion valida de Resend
When se solicita recuperar la contrasena
Then el sistema envia mediante Resend el mismo codigo y contenido definidos por el flujo actual.

### AC-002

Given un restablecimiento exitoso y configuracion valida de Resend
When se genera el aviso posterior
Then el sistema envia mediante Resend el aviso en espanol sin contrasena ni codigo.

### AC-003

Given que Resend devuelve una respuesta no exitosa al enviar el codigo
When se solicita recuperar la contrasena
Then el fallo se propaga y el flujo existente no deja un codigo utilizable.

### AC-004

Given que Resend devuelve una respuesta no exitosa al enviar el aviso posterior
When la contrasena ya fue restablecida
Then el flujo existente mantiene el cambio y registra el fallo sin datos sensibles.

### AC-005

Given el codigo de la aplicacion y sus archivos de configuracion versionados
When se inspeccionan credenciales de correo
Then no existen credenciales SMTP ni una API key real de Resend en ellos.

## Required Tests

- Solicitud a Resend con autenticacion, remitente, destinatario, asunto y contenido del codigo.
- Solicitud a Resend con el contenido del aviso posterior.
- Propagacion de respuestas HTTP no exitosas.
- Regresion de las pruebas existentes del flujo de recuperacion.

## Open Questions

- Ninguna. Los valores concretos de API key y remitente son configuracion operativa por entorno.

## Implementation Notes

- Implementar el adaptador en `VirtualBuddy.Infraestructure` y conservar `IEmailSender` sin cambios.
- Consumir el endpoint oficial `POST /emails` mediante `HttpClient` para evitar agregar una dependencia de SDK innecesaria.
- Usar variables de entorno o user secrets con claves `Resend__ApiKey`, `Resend__SenderEmail` y opcionalmente `Resend__SenderName`.

## Change Log

- 2026-08-25: Implementacion completada y validada con 45 pruebas automatizadas. El envio contra una cuenta real de Resend queda pendiente de validacion operativa.
- 2026-08-25: Spec creada en estado READY a solicitud del usuario para sustituir SMTP por Resend.
