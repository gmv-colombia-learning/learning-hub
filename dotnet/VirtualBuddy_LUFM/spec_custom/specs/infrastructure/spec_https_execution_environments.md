# Specification

## Metadata

- Title: Ejecucion HTTPS de los ambientes Local y Development
- Type: FEATURE
- Module: Infrastructure / API startup & configuration
- Status: VERIFIED

## Summary

Permitir que los perfiles de ejecucion `Local` y `Development` de la API expongan sus endpoints mediante HTTPS.

## Context

La API dispone de perfiles separados para `Local` y `Development`, pero ambos publican actualmente direcciones HTTP. El pipeline ya registra `UseHttpsRedirection()`, aunque los perfiles no declaran un endpoint HTTPS al cual redirigir.

## Scope

### In Scope

- Configurar HTTPS para los perfiles `Local` y `Development`.
- Mantener la seleccion actual de ambiente mediante `ASPNETCORE_ENVIRONMENT`.
- Actualizar los consumidores y la documentacion de las URL locales.
- Documentar la preparacion y confianza del certificado requerido para ejecutar la API.

### Out of Scope

- Modificar proveedores o conexiones de base de datos.
- Modificar endpoints, contratos, autenticacion o reglas de negocio.
- Modificar el cifrado TLS usado por Azure SQL, Supabase u Ollama.
- Agregar ambientes de testing, staging o produccion.
- Aprovisionar certificados de produccion o infraestructura cloud, salvo aprobacion explicita.

## Current Behavior

- El perfil `Local` publica `http://localhost:5089`.
- El perfil `Development` publica `http://localhost:5090`.
- IIS Express tiene configurado el puerto SSL `44367`, pero los perfiles `Project` no exponen HTTPS.
- `UseHttpsRedirection()` esta habilitado en el pipeline.
- La documentacion y `VirtualBuddy.Api.http` utilizan HTTP.

## Expected Behavior

- Los perfiles `Local` y `Development` permiten acceder a la API mediante una URL HTTPS valida para el contexto de ejecucion acordado.
- Los perfiles no publican endpoints HTTP.
- Swagger y los endpoints de la API son accesibles mediante HTTPS.
- La configuracion no altera la seleccion de ambiente ni de proveedor de persistencia.

## Functional Requirements

### FR-001

- El perfil `Local` debe exponer la API mediante HTTPS.

### FR-002

- El perfil `Development` debe exponer la API mediante HTTPS.

### FR-003

- Cada perfil debe conservar su valor actual de `ASPNETCORE_ENVIRONMENT`.

### FR-004

- Las URL documentadas y usadas por las solicitudes locales deben coincidir con los endpoints HTTPS configurados.

### FR-005

- `Local` debe usar `https://localhost:7089` y `Development` debe usar `https://localhost:7090`.

## Business Rules

### BR-001

- Habilitar HTTPS no debe cambiar el aislamiento ni el proveedor de datos de cada ambiente.

## Inputs

- Perfil de ejecucion `Local` o `Development`.
- Certificado de desarrollo HTTPS administrado por el SDK de .NET.

## Outputs

- API accesible mediante HTTPS en el perfil seleccionado.

## Validations

- El certificado debe ser utilizable y confiable en el equipo o destino donde se ejecute la API.
- Los puertos HTTPS de los perfiles deben ser distintos y estar disponibles.

## Errors

- Si el certificado requerido no existe o no es utilizable, el arranque HTTPS debe fallar con el diagnostico estandar de ASP.NET Core sin exponer secretos.

## Edge Cases

- Certificado de desarrollo ausente, expirado o no confiable.
- Puerto HTTPS ocupado.
- Solicitud realizada por error a las URL HTTP anteriores.

## Dependencies

- ASP.NET Core Kestrel y configuracion de perfiles.
- Certificado de desarrollo HTTPS administrado por el SDK de .NET.

## Affected Flows

- Arranque de la API mediante los perfiles `Local` y `Development`.
- Acceso a Swagger y a los endpoints.
- Ejecucion de solicitudes desde `VirtualBuddy.Api.http`.
- Documentacion de ambientes.

## Non-Functional Requirements

- No almacenar contrasenas de certificados ni claves privadas en el repositorio.
- No deshabilitar la validacion o confianza del certificado como solucion permanente.
- Mantener TLS separado de las credenciales y conexiones de persistencia.

## Acceptance Criteria

### AC-001

Given un certificado valido y confiable
When se inicia la API con el perfil `Local`
Then Swagger y los endpoints quedan disponibles mediante HTTPS y el ambiente activo sigue siendo `Local`.

### AC-002

Given un certificado valido y confiable
When se inicia la API con el perfil `Development`
Then Swagger y los endpoints quedan disponibles mediante HTTPS y el ambiente activo sigue siendo `Development`.

### AC-003

Given la configuracion HTTPS aplicada
When se compila la solucion y se ejecutan las pruebas aplicables
Then no se introducen regresiones en el arranque ni en la configuracion por ambiente.

### AC-004

Given las instrucciones y solicitudes locales del repositorio
When un desarrollador utiliza las URL documentadas
Then estas coinciden con los endpoints HTTPS configurados.

## Required Tests

- Compilacion de la solucion.
- Arranque de `Local` y comprobacion HTTPS de Swagger o un endpoint disponible.
- Arranque de `Development` y comprobacion HTTPS de Swagger o un endpoint disponible, sujeto a disponibilidad de Azure SQL.
- Verificacion de que ambos perfiles conservan su ambiente y proveedor de persistencia.
- Verificacion de las URL documentadas y de `VirtualBuddy.Api.http`.

## Open Questions

- Ninguna.

## Implementation Notes

- Se usara el certificado de desarrollo administrado por `dotnet dev-certs https` y no se versionaran archivos de certificado.
- `UseHttpsRedirection()` se conserva para no alterar el pipeline, aunque los perfiles no publicaran HTTP.

## Implementation Progress

- `Local` publica exclusivamente `https://localhost:7089`.
- `Development` publica exclusivamente `https://localhost:7090`.
- `VirtualBuddy.Api.http` y `docs/environments.md` utilizan las nuevas URL HTTPS.
- La documentacion incluye la comprobacion, generacion y confianza del certificado mediante `dotnet dev-certs https`.
- Se verifico un certificado confiable y vigente hasta el 2027-02-03.
- La solucion compila sin errores y conserva 32 advertencias preexistentes.
- `Local` se inicio con ambiente `Local` y respondio HTTP 200 sobre HTTPS en `/swagger/v1/swagger.json`.
- Azure SQL quedo accesible desde la IP publica autorizada y se confirmo que no existen migraciones pendientes.
- `Development` se inicio con ambiente `Development` y respondio HTTP 200 sobre HTTPS en `/swagger/v1/swagger.json`.
- La suite conserva 71 de 74 pruebas superadas; los tres fallos preexistentes pertenecen a `ResendEmailSenderTests` por una API key invalida.
- La regresion ejecutada excluyendo exclusivamente `ResendEmailSenderTests` supera 71 de 71 pruebas.
- `git diff --check` no reporto errores; solo advertencias de conversion LF/CRLF.
- Todos los criterios de aceptacion y pruebas aplicables de esta spec fueron comprobados; la spec queda `VERIFIED`.

## Change Log

- 2026-09-08: Spec DRAFT creada a partir de la solicitud de ejecutar los ambientes con HTTPS/SSL y del analisis de la configuracion actual.
- 2026-09-08: Se aprobo HTTPS exclusivo para ambos perfiles con el certificado de desarrollo de ASP.NET Core; no quedan preguntas abiertas y la spec se marca READY.
- 2026-09-08: Implementacion completada y Local validado mediante HTTPS; la verificacion total queda bloqueada por Azure SQL inaccesible y tres fallos preexistentes de Resend.
- 2026-09-08: Se autorizo la IP publica correcta en Azure SQL, Development respondio mediante HTTPS y la regresion aplicable supero 71 de 71 pruebas; la spec se marca VERIFIED.
