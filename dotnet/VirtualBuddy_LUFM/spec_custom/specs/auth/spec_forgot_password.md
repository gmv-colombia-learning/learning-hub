# Specification

## Metadata

- Title: Recuperación y restablecimiento de contraseña por email
- Type: FEATURE
- Module: Auth
- Status: IMPLEMENTED

## Summary

Permitir que un usuario no autenticado recupere el acceso a su cuenta mediante un código temporal enviado por email y establezca una contraseña nueva sin revelar la contraseña anterior.

## Context

El módulo `Auth` actualmente permite registrar usuarios e iniciar sesión, pero no ofrece un flujo para recuperar el acceso cuando un usuario olvida su contraseña.

La autenticación utiliza ASP.NET Core Identity y JWT. Los JWT emitidos actualmente permanecen válidos hasta su expiración, por lo que el restablecimiento también debe incorporar la revocación de las sesiones existentes de la cuenta.

## Scope

### In Scope

- Solicitar un código de recuperación mediante el email de la cuenta.
- Enviar códigos y avisos mediante un servicio SMTP configurable por entorno.
- Validar el código y restablecer la contraseña en una única operación final.
- Persistir los códigos para conservar su validez ante reinicios o despliegues de la API.
- Limitar solicitudes e intentos de validación.
- Invalidar los JWT emitidos antes del restablecimiento.
- Enviar un aviso por email después de un restablecimiento exitoso.
- Exponer el flujo mediante la API del módulo `Auth`.

### Out of Scope

- Implementar una interfaz web o pantallas de recuperación.
- Mostrar, recuperar o enviar la contraseña anterior.
- Cambiar la contraseña de un usuario autenticado mediante su contraseña vigente.
- Restablecimientos ejecutados por un administrador.
- Mantener un historial de contraseñas anteriores.
- Seleccionar o contratar un proveedor SMTP específico.

## Current Behavior

- `Auth` solo permite registro e inicio de sesión.
- No existe un servicio de envío de email.
- No se generan ni almacenan códigos de recuperación.
- Los JWT existentes continúan válidos hasta su expiración aunque cambie la contraseña del usuario.

## Expected Behavior

- Un usuario solicita la recuperación indicando su email.
- La API devuelve una respuesta genérica que no permite determinar si el email está registrado.
- Si existe una cuenta y el envío puede completarse, el sistema envía un código temporal por email.
- El usuario envía el email, el código, la contraseña nueva y su confirmación en una única operación final.
- Si todos los datos son válidos, la contraseña se cambia, el código se consume y las sesiones anteriores quedan invalidadas.
- El usuario debe iniciar sesión con la contraseña nueva; el restablecimiento no devuelve un JWT.

## Functional Requirements

### FR-001

La API debe permitir solicitar la recuperación de contraseña mediante un email.

### FR-002

Para una cuenta registrada, el sistema debe generar y enviar por email un código de recuperación de seis caracteres compuesto por letras mayúsculas y dígitos.

### FR-003

La comparación del código debe aceptar letras minúsculas como equivalentes a sus correspondientes mayúsculas.

### FR-004

El código debe ser válido durante 15 minutos desde su emisión.

### FR-005

La emisión de un código nuevo debe invalidar cualquier código anterior de la misma cuenta.

### FR-006

El código debe permitir un máximo de tres intentos incorrectos. Al alcanzar el tercer intento incorrecto, debe invalidarse y el usuario debe solicitar uno nuevo.

### FR-007

La operación final debe recibir el email, el código, la contraseña nueva y la confirmación de la contraseña.

### FR-008

La validación del código y el cambio de contraseña deben realizarse como una única operación final: el cambio solo debe completarse cuando todos los datos sean válidos.

### FR-009

Después de un restablecimiento exitoso, el código debe quedar consumido y no debe poder reutilizarse.

### FR-010

Después de un restablecimiento exitoso, todos los JWT emitidos previamente para la cuenta deben dejar de autorizar solicitudes protegidas.

### FR-011

El restablecimiento exitoso debe confirmar el resultado sin emitir un JWT. El usuario debe iniciar sesión de nuevo.

### FR-012

Después del restablecimiento exitoso, el sistema debe enviar un email en español informando del cambio de contraseña, sin incluir contraseñas ni códigos.

### FR-013

Si falla el envío del aviso posterior al cambio, la contraseña nueva debe mantenerse y el restablecimiento debe continuar considerándose exitoso.

### FR-014

Los códigos vigentes deben conservarse ante reinicios o despliegues de la API.

## Business Rules

### BR-001

La respuesta a una solicitud de recuperación debe ser la misma tanto para un email registrado como para uno no registrado, excepto cuando el servicio de correo no esté disponible para completar un envío requerido.

### BR-002

El sistema no debe enviar ningún correo ni crear un código utilizable para un email no registrado.

### BR-003

Solo el código más reciente de una cuenta puede ser válido.

### BR-004

Un código expirado, consumido, sustituido o invalidado por intentos no puede restablecer una contraseña.

### BR-005

La contraseña nueva debe cumplir la política vigente de Identity: mínimo ocho caracteres, al menos una letra mayúscula, una letra minúscula y un dígito; no se exige carácter no alfanumérico.

### BR-006

La contraseña nueva y su confirmación deben coincidir.

### BR-007

La contraseña nueva debe ser diferente de la contraseña vigente.

### BR-008

Las solicitudes de código deben limitarse a una por minuto y cinco por hora, aplicando el control tanto al email normalizado como al origen de la solicitud.

Los límites deben calcularse mediante ventanas móviles sobre los últimos 60 segundos y los últimos 60 minutos.

### BR-009

Si falla el envío de un código nuevo, cualquier código anterior de la cuenta debe permanecer invalidado.

### BR-010

Solo un código bien formado de seis caracteres que no coincida debe incrementar el contador de intentos. Los valores vacíos o con formato inválido deben rechazarse como validación básica sin consumir intentos.

## Inputs

### Solicitud de código

- Email de la cuenta.

### Restablecimiento de contraseña

- Email de la cuenta.
- Código temporal de seis caracteres.
- Contraseña nueva.
- Confirmación de la contraseña nueva.

## Outputs

### Solicitud aceptada

- Confirmación genérica indicando que, si existe una cuenta asociada, se enviarán instrucciones de recuperación.

### Restablecimiento exitoso

- Confirmación de que la contraseña fue restablecida y que el usuario debe iniciar sesión nuevamente.
- No debe incluir un JWT, contraseña ni código.

## Validations

- El email es obligatorio y debe tener un formato válido.
- El código es obligatorio y debe contener exactamente seis caracteres alfanuméricos.
- El código debe estar vigente, activo y corresponder al email indicado.
- La contraseña nueva y su confirmación son obligatorias y deben coincidir.
- La contraseña nueva debe cumplir la política vigente.
- La contraseña nueva debe ser diferente de la contraseña actual.
- La solicitud de código debe respetar los límites por email y origen.

## Errors

- Los códigos incorrectos, expirados, consumidos, sustituidos o invalidados deben producir el mismo error genérico, sin indicar la causa ni los intentos restantes.
- Al tercer intento incorrecto, el error genérico debe indicar que es necesario solicitar un código nuevo sin revelar información adicional del código.
- Una contraseña que no cumple la política vigente debe producir el error estándar de validación del proyecto.
- La falta de coincidencia entre contraseña y confirmación debe producir el error estándar de validación del proyecto.
- Una contraseña nueva igual a la vigente debe producir el error estándar de validación del proyecto.
- Superar los límites de solicitudes debe rechazar temporalmente nuevos envíos sin revelar si la cuenta existe.
- Si SMTP no puede enviar el código a una cuenta registrada, la API debe responder con un error temporal genérico y no debe dejar un código utilizable.
- El fallo del email de aviso posterior debe registrarse para diagnóstico, pero no debe convertir en fallido ni revertir un restablecimiento ya completado.

## Edge Cases

- Solicitud con un email no registrado.
- Solicitudes simultáneas o consecutivas para la misma cuenta.
- Uso de un código anterior después de solicitar uno nuevo.
- Código introducido con letras minúsculas.
- Código utilizado exactamente al vencer su periodo de validez.
- Tercer intento incorrecto y posterior uso del código correcto.
- Reutilización de un código después de un restablecimiento exitoso.
- Reinicio o despliegue de la API mientras existe un código vigente.
- Dos intentos simultáneos de restablecimiento con el mismo código.
- Fallo SMTP durante el envío inicial.
- Fallo SMTP durante el aviso posterior al cambio.
- Uso de un JWT anterior después del restablecimiento.

## Dependencies

- ASP.NET Core Identity para usuarios, política de contraseñas y restablecimiento.
- Persistencia compartida para códigos, expiración, intentos y consumo.
- Servicio SMTP configurable por entorno.
- Configuración segura del host, puerto, TLS, remitente y credenciales SMTP.
- Mecanismo de revocación o versión de sesión integrado con la validación JWT.
- Mecanismo de limitación de solicitudes por email y origen.

## Affected Flows

- Registro e identidad de `ApplicationUser` como fuente de las cuentas recuperables.
- Inicio de sesión con la contraseña nueva.
- Autorización de todas las solicitudes protegidas para rechazar JWT anteriores al restablecimiento.
- Configuración de infraestructura y persistencia.
- Manejo estándar de validaciones y errores de la API.

## Non-Functional Requirements

- Los códigos deben generarse mediante un generador criptográficamente seguro.
- Los códigos no deben almacenarse en texto legible.
- Contraseñas, códigos y credenciales SMTP no deben escribirse en logs.
- Las credenciales SMTP no deben almacenarse en el repositorio y deben configurarse mediante secretos por entorno.
- La comunicación SMTP debe utilizar transporte cifrado.
- Las respuestas y el comportamiento observable deben minimizar la enumeración de cuentas.
- La validación y consumo del código deben ser seguros ante solicitudes concurrentes.
- Los fallos de SMTP y del aviso posterior deben registrarse sin incluir datos sensibles.

## Acceptance Criteria

### AC-001

Given un email asociado a una cuenta
When se solicita recuperar la contraseña dentro de los límites permitidos y SMTP está disponible
Then el sistema envía un código de seis caracteres compuesto por mayúsculas y dígitos, válido durante 15 minutos, y devuelve la respuesta genérica.

### AC-002

Given un email no asociado a una cuenta
When se solicita recuperar la contraseña
Then el sistema devuelve la misma respuesta genérica, sin crear un código utilizable ni enviar un correo.

### AC-003

Given un código vigente
When se solicita otro código para la misma cuenta
Then el código anterior queda invalidado y solo el nuevo puede utilizarse.

### AC-004

Given un código vigente
When se introduce incorrectamente tres veces
Then el código queda invalidado y el usuario debe solicitar uno nuevo.

### AC-005

Given un código vigente emitido con letras mayúsculas
When el usuario introduce las letras equivalentes en minúsculas junto con contraseñas válidas y coincidentes
Then el código se acepta y la contraseña se restablece.

### AC-006

Given un email, código vigente, contraseña nueva válida y confirmación coincidente
When se ejecuta el restablecimiento
Then la contraseña cambia, el código queda consumido, no se emite un JWT y se informa que el usuario debe iniciar sesión nuevamente.

### AC-007

Given una contraseña restablecida correctamente
When se intenta reutilizar el mismo código
Then el sistema responde con el error genérico de código no válido.

### AC-008

Given un código expirado, sustituido, consumido o invalidado
When se intenta restablecer la contraseña
Then el sistema rechaza la operación con el mismo error genérico y no cambia la contraseña.

### AC-009

Given un código válido
When la contraseña y la confirmación no coinciden, la contraseña incumple la política o es igual a la vigente
Then el sistema rechaza la operación con el error estándar de validación y no cambia la contraseña ni consume exitosamente el código.

### AC-010

Given que una solicitud supera una por minuto o cinco por hora para el email o el origen
When se intenta solicitar otro código
Then el sistema rechaza temporalmente el envío sin revelar si el email está registrado.

### AC-011

Given una cuenta registrada y un fallo SMTP al enviar el código
When se solicita recuperar la contraseña
Then la API devuelve un error temporal genérico y no deja un código utilizable.

### AC-012

Given un restablecimiento exitoso
When el sistema completa el cambio
Then envía un aviso en español al email de la cuenta sin incluir contraseña ni código.

### AC-013

Given un restablecimiento exitoso y un fallo al enviar el aviso posterior
When finaliza la operación
Then la contraseña nueva se mantiene, la operación continúa siendo exitosa y el fallo queda registrado sin datos sensibles.

### AC-014

Given un código vigente almacenado
When la API se reinicia o se despliega antes de los 15 minutos
Then el código conserva su vigencia restante y puede utilizarse si cumple las demás reglas.

### AC-015

Given un JWT emitido antes del restablecimiento
When la contraseña se restablece y ese JWT intenta acceder a un recurso protegido
Then la solicitud no es autorizada.

### AC-016

Given dos solicitudes simultáneas con el mismo código válido
When ambas intentan restablecer la contraseña
Then como máximo una puede completarse exitosamente.

## Required Tests

- Solicitud exitosa para una cuenta registrada.
- Respuesta indistinguible para una cuenta no registrada.
- Formato, normalización y vigencia del código.
- Invalidación del código anterior al emitir uno nuevo.
- Invalidación al tercer intento incorrecto.
- Error genérico para todos los estados inválidos del código.
- Restablecimiento exitoso y consumo de un solo uso.
- Rechazo por contraseñas no coincidentes.
- Rechazo por incumplimiento de la política de contraseña.
- Rechazo al reutilizar la contraseña vigente.
- Límites por minuto y por hora para email y origen.
- Persistencia del código tras reinicio de la aplicación.
- Concurrencia al consumir el mismo código.
- Fallo SMTP durante el envío inicial sin código utilizable.
- Fallo del aviso posterior sin reversión del cambio.
- Contenido del aviso sin contraseña ni código.
- Revocación de JWT anteriores.
- Inicio de sesión exitoso con la contraseña nueva.
- Inicio de sesión fallido con la contraseña anterior.
- Regresión de registro, login y autorización con JWT emitidos después del restablecimiento.

## Open Questions

- Ninguna.

## Implementation Notes

- Mantener los casos de uso dentro de `VirtualBuddy.Application.Auth` y la integración de Identity/SMTP dentro de infraestructura, respetando Clean Architecture.
- Reutilizar el formato estándar de errores del proyecto; cualquier extensión necesaria para indisponibilidad temporal o límites debe conservar `ProblemDetails`.
- El origen usado para limitar solicitudes debe derivarse de la solicitud HTTP y considerar la configuración de proxies confiables del entorno.
- La persistencia debe permitir invalidación, expiración, conteo de intentos y consumo atómico sin guardar el código legible.
- La estrategia concreta de revocación JWT debe definirse durante el diseño técnico, pero debe cumplir AC-015 para todos los recursos protegidos.
- Los valores concretos del servidor y las credenciales SMTP son configuración operativa, no requisitos funcionales de esta spec.

## Change Log

- 2026-08-20: Implementación completada y validada con 42 pruebas automatizadas. La verificación en PostgreSQL y SMTP reales queda pendiente por falta de servicios disponibles en el entorno.
- 2026-08-20: Se aclaró el comportamiento ante reenvío fallido, las ventanas móviles y qué solicitudes consumen intentos.
- 2026-08-20: Spec creada en estado READY con los requisitos confirmados para recuperación por código temporal y revocación de sesiones.
