# Specification

## Metadata

- Title: Configuracion de base de datos por ambiente Local y Development
- Type: FEATURE
- Module: Infrastructure / Configuration
- Status: IMPLEMENTED

## Summary

Permitir ejecutar la API contra una base de datos PostgreSQL local o contra la base de datos PostgreSQL del ambiente de desarrollo alojada en Supabase, seleccionando el destino mediante el ambiente de ejecucion.

## Context

La API utiliza Entity Framework Core con Npgsql, pero actualmente no dispone de configuraciones separadas para ejecucion local y desarrollo remoto. Se requieren inicialmente dos ambientes: `Local` y `Development`.

## Scope

### In Scope

- Definir los ambientes `Local` y `Development` para la API.
- Usar una base PostgreSQL local cuando el ambiente sea `Local`.
- Usar PostgreSQL alojado en Supabase cuando el ambiente sea `Development`.
- Permitir seleccionar ambos ambientes mediante perfiles de ejecucion claros.
- Mantener cada cadena de conexion en el archivo `appsettings` de su ambiente durante la etapa actual de pruebas.
- Documentar la configuracion y los comandos de ejecucion de ambos ambientes.

### Out of Scope

- Agregar ambientes de testing, staging o produccion.
- Cambiar el proveedor PostgreSQL/Npgsql.
- Cambiar el comportamiento de Supabase Storage, Ollama, Resend u otros servicios externos.
- Crear o administrar el proyecto remoto de Supabase.
- Modificar reglas de negocio, endpoints o contratos de la API.

## Current Behavior

- Los perfiles existentes ejecutan con `ASPNETCORE_ENVIRONMENT=Development`.
- No existe un perfil `Local`.
- La configuracion base contiene una cadena SQL Server, aunque la infraestructura usa Npgsql/PostgreSQL.
- No hay una separacion explicita entre la conexion local y la conexion remota de desarrollo.

## Expected Behavior

- El perfil local inicia la API con el ambiente `Local` y obtiene `ConnectionStrings:DefaultConnection` para PostgreSQL local.
- El perfil dev inicia la API con el ambiente `Development` y obtiene `ConnectionStrings:DefaultConnection` para PostgreSQL de Supabase.
- Cada ambiente mantiene su conexion completa en su propio archivo de configuracion.
- La aplicacion falla al iniciar con un mensaje de configuracion claro si falta la cadena de conexion del ambiente seleccionado.

## Functional Requirements

### FR-001

La API debe seleccionar la configuracion del ambiente mediante `ASPNETCORE_ENVIRONMENT`.

### FR-002

El ambiente `Local` debe usar una instancia PostgreSQL local.

### FR-003

El ambiente `Development` debe usar la base PostgreSQL del proyecto de desarrollo en Supabase.

### FR-004

Debe existir un perfil de ejecucion identificable para cada ambiente.

### FR-005

Las cadenas deben configurarse como `ConnectionStrings:Local` y `ConnectionStrings:Development` en sus respectivos archivos de ambiente. `ConnectionStrings:DefaultConnection` debe mantenerse como fallback.

## Business Rules

### BR-001

Los ambientes `Local` y `Development` deben mantener datos aislados mediante bases de datos diferentes.

## Inputs

- Ambiente seleccionado mediante `ASPNETCORE_ENVIRONMENT`.
- Cadena PostgreSQL correspondiente al ambiente seleccionado o, como fallback, en `ConnectionStrings:DefaultConnection`.

## Outputs

- API conectada a PostgreSQL local en `Local`.
- API conectada a PostgreSQL de Supabase en `Development`.

## Validations

- La cadena correspondiente al ambiente, o su fallback `ConnectionStrings:DefaultConnection`, debe existir y no estar vacia.
- La cadena debe ser consumible por Npgsql.

## Errors

- Una cadena ausente debe impedir el inicio con un mensaje que identifique las claves admitidas.
- Los errores de conexion deben conservar el manejo y logging de infraestructura existente.

## Edge Cases

- Ejecucion sin especificar un ambiente.
- Perfil seleccionado sin secretos configurados.
- Base local apagada o base remota inaccesible.
- Caracteres especiales en la contrasena de Supabase.

## Dependencies

- ASP.NET Core Configuration.
- Entity Framework Core con Npgsql.
- Instancia PostgreSQL local.
- Proyecto Supabase de desarrollo y su cadena de conexion.

## Affected Flows

- Arranque de la API.
- Registro de `BuddyDBContext`.
- Inicializacion de datos al arrancar.
- Ejecucion de migraciones de Entity Framework Core.

## Non-Functional Requirements

- La gestion mediante vaults o almacenes de secretos queda fuera del alcance de esta etapa de pruebas por decision del usuario.
- Los nombres de perfiles y ambientes deben ser claros para desarrollo local.
- No registrar la cadena de conexion completa.

## Acceptance Criteria

### AC-001

Given una cadena valida para PostgreSQL local
When se inicia la API con el perfil `Local`
Then el ambiente activo es `Local` y `BuddyDBContext` utiliza la base local.

### AC-002

Given una cadena valida para PostgreSQL de Supabase
When se inicia la API con el perfil `Development`
Then el ambiente activo es `Development` y `BuddyDBContext` utiliza la base remota de desarrollo.

### AC-003

Given los dos archivos de ambiente configurados
When se selecciona un perfil
Then la aplicacion utiliza la cadena completa correspondiente a ese ambiente.

### AC-004

Given un ambiente sin una cadena especifica ni `ConnectionStrings:DefaultConnection`
When se inicia la API
Then el inicio falla con un mensaje claro que identifica la configuracion faltante.

### AC-005

Given la configuracion de ambientes aplicada
When se compila y ejecutan las pruebas existentes
Then no se introducen regresiones en los flujos de aplicacion.

## Required Tests

- Prueba de resolucion de la cadena local.
- Prueba de resolucion de la cadena remota de desarrollo.
- Prueba de fallo al faltar `ConnectionStrings:DefaultConnection`.
- Compilacion de la solucion.
- Suite de pruebas existente.

## Open Questions

- Ninguna.

## Implementation Notes

- ASP.NET Core carga automaticamente `appsettings.{Environment}.json` despues de `appsettings.json`.
- Se recomienda usar `Local` para la maquina del desarrollador y conservar `Development` para el entorno remoto dev.
- Supabase ofrece conexion directa y pooler; debe usarse la cadena compatible con el destino donde se ejecutara la API.
- La instancia PostgreSQL local sera instalada y administrada fuera del proyecto; no se agregara Docker Compose.
- La infraestructura resolvera primero `ConnectionStrings:{Environment}` y utilizara `ConnectionStrings:DefaultConnection` como fallback.

## Change Log

- 2026-09-03: Spec DRAFT creada a partir de la solicitud y del analisis de la configuracion existente.
- 2026-09-03: Se confirmo que PostgreSQL local ya se administra fuera del proyecto; no se incluira Docker Compose.
- 2026-09-03: Spec aprobada sin preguntas abiertas y marcada READY para implementacion.
- 2026-09-03: Implementacion completada con perfiles Local y Development, seleccion automatica de conexiones, User Secrets, ejemplos, documentacion y validacion de configuracion.
- 2026-09-03: Compilacion correcta y 4 pruebas de configuracion superadas. La suite global supera 63 de 66 pruebas; conserva 3 fallos ajenos en ResendEmailSenderTests por una API key invalida. Verificacion contra las bases reales pendiente.
- 2026-09-03: Los archivos de ambiente adoptan los nombres estandar `appsettings.Local.json` y `appsettings.Development.json`; permanecen libres de credenciales.
- 2026-09-03: Por decision del usuario, se simplifico la etapa de pruebas guardando las conexiones completas en los archivos de ambiente y se retiro User Secrets.
