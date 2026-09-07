# Specification

## Metadata

- Title: Azure SQL Database para persistencia completa en Development
- Type: FEATURE
- Module: Infrastructure / Persistence & Configuration
- Status: IMPLEMENTED

## Summary

Permitir que la API use PostgreSQL con Npgsql en el ambiente `Local` y Azure SQL Database con SQL Server en el ambiente `Development`, incluyendo la persistencia transaccional, Identity, recuperacion de contrasena, indexacion de documentos y busqueda semantica RAG.

## Context

Actualmente los ambientes `Local` y `Development` usan PostgreSQL. `Local` usa una instancia local y `Development` usa PostgreSQL alojado en Supabase.

Se requiere reemplazar unicamente la base PostgreSQL de Supabase del ambiente `Development` por Azure SQL Database. Supabase Storage debe continuar almacenando documentos e imagenes.

La persistencia RAG actual depende de caracteristicas especificas de PostgreSQL, como `real[]`, `jsonb`, `UNNEST` y parametros Npgsql. Azure SQL Database dispone del tipo `VECTOR(n)` y de `VECTOR_DISTANCE` para implementar la persistencia y busqueda semantica de Development.

## Scope

### In Scope

- Mantener PostgreSQL y Npgsql sin cambios funcionales en `Local`.
- Usar Azure SQL Database y el proveedor SQL Server de Entity Framework Core en `Development`.
- Seleccionar el proveedor de base de datos mediante el ambiente de ejecucion.
- Persistir en Azure SQL todos los datos de Development, incluyendo Identity y recuperacion de contrasena.
- Persistir en Azure SQL los `KnowledgeChunks`, embeddings y metadata.
- Realizar en Azure SQL la busqueda semantica por similitud coseno.
- Permitir configurar la dimension esperada de los embeddings.
- Validar que los embeddings sean compatibles con la dimension configurada y con el esquema.
- Mantener conjuntos de migraciones separados para PostgreSQL y SQL Server.
- Crear una nueva base Azure SQL mediante migraciones explicitas y ejecutar el seed existente.
- Documentar la creacion y configuracion manual del recurso Azure SQL.
- Mantener las cadenas completas en el archivo `appsettings` de cada ambiente durante la etapa actual del prototipo.
- Fallar durante el arranque de Development cuando Azure SQL no sea utilizable.

### Out of Scope

- Migrar datos existentes desde PostgreSQL/Supabase hacia Azure SQL.
- Reemplazar o modificar Supabase Storage.
- Reemplazar Ollama, Semantic Kernel o los modelos configurados.
- Agregar Azure Key Vault u otro almacen de secretos.
- Usar Microsoft Entra ID o identidades administradas para conectar a Azure SQL.
- Aprovisionar recursos mediante Bicep, Terraform u otra herramienta IaC.
- Agregar ambientes de testing, staging o produccion.
- Modificar endpoints, DTOs, reglas de dominio o contratos publicos de la API.
- Ejecutar automaticamente migraciones pendientes durante el arranque de la API.

## Current Behavior

- `Local` selecciona una cadena PostgreSQL y registra `BuddyDBContext` con Npgsql.
- `Development` selecciona una cadena PostgreSQL de Supabase y tambien registra `BuddyDBContext` con Npgsql.
- Las migraciones existentes fueron generadas para PostgreSQL.
- `PostgresKnowledgeBaseService` usa SQL, tipos y parametros exclusivos de PostgreSQL.
- Los embeddings se guardan como `real[]` y la metadata como `jsonb`.
- La similitud coseno se calcula mediante SQL basado en `UNNEST`.
- La recuperacion de contrasena usa un advisory lock cuando el proveedor es Npgsql.
- El inicializador usa `EnsureCreated` antes de ejecutar el seed.

## Expected Behavior

- `Local` conserva PostgreSQL, sus migraciones y todos los flujos actuales, incluido RAG.
- `Development` registra SQL Server como proveedor de `BuddyDBContext` y usa la cadena Azure SQL del ambiente.
- Ningun flujo de persistencia de Development depende de PostgreSQL.
- Indexacion, eliminacion y busqueda de chunks funcionan en Azure SQL.
- La busqueda semantica en Azure SQL conserva la metrica coseno y el limite solicitado.
- La dimension de embeddings se obtiene de configuracion y se valida antes de persistir o buscar.
- Un cambio de dimension requiere una migracion de esquema y la reindexacion de documentos.
- Las migraciones se aplican explicitamente antes de iniciar la API.
- El seed se ejecuta sobre el esquema ya migrado y no crea el esquema mediante `EnsureCreated`.
- Development falla al iniciar si no puede conectarse, autenticar, validar el esquema o ejecutar el seed.

## Functional Requirements

### FR-001

La infraestructura debe seleccionar PostgreSQL para `Local` y SQL Server para `Development` usando el ambiente de ejecucion.

### FR-002

El ambiente `Local` debe continuar usando `ConnectionStrings:Local` y Npgsql.

### FR-003

El ambiente `Development` debe usar `ConnectionStrings:Development` y el proveedor SQL Server de Entity Framework Core.

### FR-004

Todos los datos relacionales de Development deben almacenarse en Azure SQL Database.

### FR-005

Los chunks, embeddings y metadata generados por la indexacion en Development deben almacenarse en Azure SQL Database.

### FR-006

La busqueda RAG en Development debe obtener desde Azure SQL los chunks del proyecto ordenados por similitud coseno y respetar el limite solicitado.

### FR-007

La dimension esperada del embedding debe ser configurable y debe coincidir con la dimension utilizada por el esquema vectorial de cada proveedor.

### FR-008

Las migraciones de PostgreSQL y SQL Server deben mantenerse en conjuntos independientes.

### FR-009

Las migraciones deben ejecutarse de forma explicita antes de iniciar la aplicacion y no automaticamente durante el arranque.

### FR-010

Una base Azure SQL nueva debe poder recibir el esquema completo mediante las migraciones SQL Server y posteriormente los datos iniciales mediante el seed vigente.

### FR-011

Supabase Storage debe continuar operando sin cambios para documentos e imagenes en ambos ambientes.

### FR-012

La documentacion debe describir la configuracion manual de Azure SQL, la configuracion local de la API, la ejecucion de migraciones y las validaciones posteriores.

## Business Rules

### BR-001

Los datos de `Local` y `Development` deben permanecer aislados en bases diferentes.

### BR-002

Development no debe utilizar PostgreSQL como almacenamiento secundario para el flujo RAG.

### BR-003

Un embedding con una dimension diferente a la configurada no debe persistirse ni utilizarse en una busqueda.

### BR-004

Cambiar la dimension configurada requiere crear y aplicar una migracion compatible y reindexar los documentos; no se garantiza compatibilidad con embeddings anteriores.

## Inputs

- `ASPNETCORE_ENVIRONMENT` con valor `Local` o `Development`.
- Cadena `ConnectionStrings:Local` para PostgreSQL local.
- Cadena `ConnectionStrings:Development` para Azure SQL Database.
- Usuario y contrasena de SQL Authentication incluidos en la cadena de Development.
- Dimension configurada para los embeddings.
- Embeddings generados por el modelo configurado en Ollama.

## Outputs

- API Local conectada a PostgreSQL y funcional sin regresiones.
- API Development conectada exclusivamente a Azure SQL para persistencia relacional y RAG.
- Esquema Azure SQL creado mediante migraciones SQL Server.
- Resultados RAG ordenados por similitud coseno.
- Seed inicial almacenado en la base correspondiente al ambiente.

## Validations

- La cadena correspondiente al ambiente debe existir y no estar vacia.
- La cadena de `Local` debe ser compatible con Npgsql.
- La cadena de `Development` debe ser compatible con SQL Server y Azure SQL.
- La dimension configurada debe ser un entero positivo soportado por Azure SQL.
- Todo embedding recibido debe tener exactamente la dimension configurada.
- La base Development debe contener el esquema y las migraciones SQL Server requeridas.
- Azure SQL debe soportar el tipo vectorial y la funcion de distancia utilizados.
- La conexion debe validarse sin registrar la cadena ni sus credenciales.

## Errors

- Un ambiente no soportado debe impedir el inicio con un mensaje claro.
- Una cadena ausente o invalida debe impedir el inicio e identificar la clave de configuracion, sin mostrar su valor.
- Credenciales incorrectas o una regla de red que impida la conexion deben provocar fallo de arranque en Development.
- Un esquema ausente o con migraciones pendientes debe provocar fallo de arranque en Development e indicar que deben aplicarse las migraciones.
- Una dimension invalida o incompatible debe producir un error claro sin persistir datos parciales.
- Un fallo al indexar chunks debe conservar el manejo estandar de errores y no afectar Supabase Storage fuera del comportamiento ya existente.

## Edge Cases

- Ejecutar Development sin haber aplicado las migraciones SQL Server.
- Ejecutar una migracion usando accidentalmente el proveedor equivocado.
- Azure SQL inaccesible por firewall, pausa del servicio o credenciales incorrectas.
- Modelo de embeddings cambiado sin actualizar dimension, esquema y datos indexados.
- Embedding vacio o con una dimension distinta.
- Proyecto sin chunks indexados.
- Solicitar mas resultados que los chunks disponibles.
- Metadata nula o vacia.
- Fallo de conexion durante el seed.
- Ejecutar Local despues de agregar las migraciones SQL Server.

## Dependencies

- ASP.NET Core Configuration.
- Entity Framework Core con Npgsql para `Local`.
- Entity Framework Core con SQL Server para `Development`.
- Microsoft.Data.SqlClient compatible con las capacidades vectoriales utilizadas.
- Azure SQL Database con soporte para `VECTOR(n)` y `VECTOR_DISTANCE`.
- Instancia PostgreSQL local existente.
- Ollama y el modelo de embeddings configurado.
- Supabase Storage existente.

## Affected Flows

- Arranque de la API.
- Resolucion de configuracion por ambiente.
- Registro y uso de `BuddyDBContext`.
- Creacion y aplicacion de migraciones.
- Seed inicial.
- Registro y autenticacion mediante Identity.
- Recuperacion de contrasena y su control de concurrencia.
- CRUD de proyectos y documentos.
- Indexacion y eliminacion de `KnowledgeChunks`.
- Busqueda semantica y chat RAG.

## Non-Functional Requirements

- No registrar cadenas de conexion, usuarios ni contrasenas.
- El uso de credenciales en texto plano dentro de los `appsettings` queda limitado a la etapa actual del prototipo por decision explicita del usuario.
- La implementacion debe mantener PostgreSQL y SQL Server encapsulados en Infrastructure.
- Domain y Application no deben depender de Npgsql, SqlClient ni detalles de Azure SQL.
- Las consultas vectoriales deben ejecutarse en la base de datos, no cargar todos los embeddings en memoria para ordenarlos.
- La seleccion de proveedor debe ser determinista y verificable.
- Los errores de arranque deben permitir distinguir configuracion ausente, conexion fallida, esquema pendiente y dimension incompatible sin revelar secretos.

## Acceptance Criteria

### AC-001

Given el ambiente `Local` y una cadena PostgreSQL valida
When se inicia la API
Then `BuddyDBContext` usa Npgsql y los flujos existentes, incluido RAG, conservan su comportamiento.

### AC-002

Given el ambiente `Development`, una cadena Azure SQL valida y el esquema actualizado
When se inicia la API
Then `BuddyDBContext` usa SQL Server y no abre conexiones PostgreSQL para persistencia.

### AC-003

Given una base Azure SQL vacia
When se aplican explicitamente las migraciones SQL Server
Then se crea el esquema completo, incluidas Identity, recuperacion de contrasena y persistencia vectorial.

### AC-004

Given una base Azure SQL migrada sin datos iniciales
When se inicia Development
Then el seed vigente se ejecuta correctamente sin usar `EnsureCreated` para crear el esquema.

### AC-005

Given un documento valido y un embedding con la dimension configurada
When se indexa en Development
Then sus chunks, metadata y embeddings se almacenan en Azure SQL.

### AC-006

Given chunks indexados para un proyecto en Azure SQL
When se realiza una busqueda semantica
Then Azure SQL devuelve hasta el limite solicitado ordenados por similitud coseno.

### AC-007

Given un embedding cuya dimension no coincide con la configuracion
When se intenta indexar o buscar
Then la operacion falla con un error claro y no persiste datos parciales.

### AC-008

Given un cambio de modelo que modifica la dimension
When se prepara Development
Then se requiere una migracion de esquema y la reindexacion antes de aceptar embeddings de la nueva dimension.

### AC-009

Given Development sin cadena, con credenciales incorrectas, sin acceso de red o con esquema pendiente
When se inicia la API
Then el proceso falla con un mensaje diagnostico que no revela secretos.

### AC-010

Given la integracion Azure SQL activa en Development
When se cargan o consultan documentos e imagenes
Then Supabase Storage conserva el comportamiento existente.

### AC-011

Given los dos conjuntos de migraciones
When se aplican al proveedor correspondiente
Then las migraciones PostgreSQL solo afectan Local y las migraciones SQL Server solo afectan Development.

### AC-012

Given la feature implementada
When se compila la solucion y se ejecutan las pruebas requeridas
Then no existen regresiones en los contratos y flujos existentes.

## Required Tests

- Prueba de seleccion de Npgsql para `Local`.
- Prueba de seleccion de SQL Server para `Development`.
- Prueba de rechazo de ambientes no soportados.
- Pruebas de configuracion ausente e invalida sin exposicion de secretos.
- Pruebas de migraciones PostgreSQL sobre PostgreSQL.
- Pruebas de migraciones SQL Server sobre SQL Server compatible.
- Prueba de deteccion de migraciones pendientes en Development.
- Prueba de seed sobre una base Azure SQL nueva y migrada.
- Pruebas de CRUD e Identity con SQL Server.
- Pruebas de concurrencia de recuperacion de contrasena con ambos proveedores.
- Prueba de insercion, consulta y eliminacion de chunks en PostgreSQL.
- Prueba de insercion, consulta y eliminacion de chunks en SQL Server.
- Prueba de ranking por similitud coseno en Azure SQL.
- Pruebas de dimension valida, dimension invalida y cambio de dimension.
- Pruebas de regresion de Supabase Storage.
- Compilacion de la solucion y suite completa existente.
- Smoke test documentado contra la instancia Azure SQL real.

## Open Questions

- Ninguna.

## Implementation Notes

- EF Core requiere mantener migraciones independientes cuando un mismo modelo soporta varios proveedores.
- Las migraciones PostgreSQL existentes deben conservarse; Azure SQL debe iniciar con un conjunto SQL Server nuevo.
- La configuracion del proveedor puede derivarse del ambiente, ya que la decision aprobada es fija: `Local` usa PostgreSQL y `Development` usa SQL Server.
- La implementacion RAG debe permanecer detras de `IKnowledgeBaseService`; los detalles Npgsql y SqlClient pertenecen a Infrastructure.
- Azure SQL expone `VECTOR(n)` con dimension fija. La dimension configurada debe coincidir con la migracion aplicada.
- Para cambiar la dimension se debe generar una migracion, aplicarla y reindexar los documentos. No se migraran embeddings anteriores.
- Se recomienda usar `VECTOR_DISTANCE('cosine', ...)` para mantener la metrica actual y evitar calculo en memoria.
- El control de concurrencia de recuperacion de contrasena debe preservar su garantia en SQL Server sin trasladar detalles del proveedor al dominio.
- El arranque puede validar conectividad, migraciones pendientes y dimension, pero no debe aplicar migraciones automaticamente.
- El seed debe ejecutarse despues de validar que el esquema esta actualizado.

### Secuencia De Implementacion Y Configuracion

1. Preparar en el repositorio la seleccion de proveedor por ambiente y los componentes de persistencia especificos, sin cambiar `Local`.
2. Crear y validar el conjunto inicial de migraciones SQL Server contra una instancia SQL Server compatible de desarrollo o pruebas.
3. Adaptar y probar indexacion, eliminacion y busqueda vectorial para Azure SQL.
4. Adaptar y probar los flujos dependientes del proveedor, incluida recuperacion de contrasena.
5. Crear manualmente en Azure el servidor logico y la base Azure SQL vacia.
6. Configurar SQL Authentication, guardar el usuario y la contrasena administrativamente y habilitar solo las reglas de red necesarias.
7. Verificar en Azure SQL la disponibilidad del tipo `VECTOR` y de `VECTOR_DISTANCE` para la base seleccionada.
8. Obtener la cadena ADO.NET desde Azure y colocarla en `ConnectionStrings:Development` de `appsettings.Development.json`.
9. Configurar en Development la dimension que utiliza el modelo de embeddings seleccionado.
10. Aplicar explicitamente las migraciones SQL Server a Azure SQL usando el ambiente y proveedor Development.
11. Confirmar que no existen migraciones pendientes antes de iniciar la API.
12. Iniciar la API en Development para validar conectividad y ejecutar el seed.
13. Ejecutar smoke tests de autenticacion, proyectos, documentos, indexacion y chat RAG.
14. Iniciar la API en Local y ejecutar la regresion PostgreSQL completa.

### Guia Manual De Azure

- Crear un servidor logico Azure SQL y una base Azure SQL nueva para Development.
- Seleccionar SQL Authentication y definir un administrador SQL.
- Configurar una regla de firewall para la IP publica desde la que se aplicaran migraciones y ejecutara la API.
- No habilitar acceso publico mas amplio que el necesario para esta etapa.
- Confirmar que la base permite ejecutar una expresion de prueba con `VECTOR(n)` y `VECTOR_DISTANCE` usando una dimension pequena temporal.
- Copiar la cadena ADO.NET para SQL Authentication y sustituir servidor, base, usuario y contrasena con los valores reales.
- Mantener cifrado habilitado y no desactivar la validacion del certificado para Azure SQL.
- Aplicar las migraciones SQL Server desde el repositorio antes del primer arranque.
- Revisar en Azure que las tablas, el historial de migraciones y `KnowledgeChunks` fueron creados.
- Eliminar cualquier tabla temporal usada para verificar capacidades vectoriales.

## Implementation Progress

Estado final de la implementacion el 2026-09-03:

### Completado

- Seleccion determinista de proveedor por ambiente: Npgsql para `Local` y SQL Server para `Development`.
- Registro de `PostgresKnowledgeBaseService` o `SqlServerKnowledgeBaseService` segun el ambiente.
- Validacion de cadenas por proveedor sin incluir sus valores en los errores.
- Configuracion y validacion de `Ollama:EmbeddingDimension` con valor actual `768`.
- Validacion completa del lote de embeddings antes de persistirlo.
- Insercion atomica de chunks en ambos proveedores.
- Implementacion Azure SQL de insercion con `VECTOR(768)`, busqueda con `VECTOR_DISTANCE('cosine', ...)` y eliminacion por documento.
- Implementacion de bloqueo SQL Server mediante `sp_getapplock` para recuperacion de contrasena.
- Eliminacion de `EnsureCreated` del seed.
- Validacion de conectividad, migraciones pendientes y esquema vectorial antes del seed.
- Fallo de arranque sanitizado cuando la base no esta preparada.
- Proyecto independiente `VirtualBuddy.Migrations.SqlServer` agregado a la solucion.
- Migracion `20260903212618_InitialAzureSql` generada y aplicada correctamente en Azure SQL.
- Azure SQL validado con arranque Development, seed, login y consulta autenticada de tres proyectos.
- Smoke test RAG real completado: carga TXT en Supabase Storage, embedding Ollama de 768 dimensiones, insercion en Azure SQL, busqueda coseno y respuesta contextual correcta.
- Smoke test real de `sp_getapplock` completado mediante solicitud de recuperacion para una cuenta inexistente.
- Documentos, chunks, objetos Storage y solicitudes de recuperacion creados para diagnostico eliminados al finalizar.
- Flujo de eliminacion de documento actualizado para borrar tambien sus `KnowledgeChunks` dentro de la misma transaccion relacional.
- Seed convertido en idempotente y transaccional para evitar estados parciales.
- Documentacion `docs/environments.md` actualizada para Azure SQL y comandos por proveedor.
- Compilacion de la solucion completada correctamente.
- Migracion PostgreSQL preexistente `20260831183801_MakeProjectImageOptional` aplicada y Local sin migraciones pendientes.
- Local validado con arranque Npgsql y smoke RAG real de indexacion, busqueda y eliminacion.
- Pruebas de la feature y regresion no relacionada con Resend: 71 de 71 superadas.
- Suite global: 71 de 74 superadas; conserva tres fallos preexistentes y ajenos en `ResendEmailSenderTests` por API key invalida.
- `git diff --check` completado sin errores; solo se reportaron advertencias de conversion LF/CRLF del entorno Windows.
- La spec queda `IMPLEMENTED`. No se marca `VERIFIED` porque la suite global aun contiene tres fallos, aunque sean ajenos a esta feature.

## Change Log

- 2026-09-03: Spec DRAFT iniciada a partir de la solicitud de reemplazar PostgreSQL/Supabase por Azure SQL en Development.
- 2026-09-03: Se clasifico en `Infrastructure / Persistence & Configuration` y se identifico impacto transversal sobre persistencia, Identity y RAG.
- 2026-09-03: Se confirmo que Local conserva PostgreSQL completo y Development usa Azure SQL para toda la persistencia, incluida indexacion y busqueda semantica.
- 2026-09-03: Se confirmo que Supabase Storage permanece sin cambios.
- 2026-09-03: Se decidio iniciar Azure SQL con una base nueva y seed, sin migracion de datos existentes.
- 2026-09-03: Se incluyo aprovisionamiento manual documentado, sin IaC.
- 2026-09-03: Se aprobo SQL Authentication y el almacenamiento temporal de cadenas completas en los archivos de ambiente, aceptando expresamente el riesgo.
- 2026-09-03: Se aprobaron migraciones explicitas y fallo de arranque ante una base Development no operativa.
- 2026-09-03: Se aprobo dimension configurable con migracion y reindexacion obligatorias ante cada cambio.
- 2026-09-03: No quedan preguntas funcionales abiertas; spec aprobada y marcada READY.
- 2026-09-03: Implementacion iniciada con proveedores y migraciones separados por ambiente.
- 2026-09-03: Baseline SQL Server aplicado y Development validado contra Azure SQL con seed, autenticacion, CRUD de lectura, RAG y bloqueo de recuperacion.
- 2026-09-03: Implementacion pausada antes de aplicar una migracion PostgreSQL preexistente pendiente en Local; continuidad detallada en `Implementation Progress`.
- 2026-09-03: Migracion PostgreSQL pendiente aplicada y Local validado con arranque y smoke RAG.
- 2026-09-03: Revision final incorporo seed transaccional e idempotente, eliminacion relacional atomica y logging sanitizado.
- 2026-09-03: Implementacion completada y marcada IMPLEMENTED; verificacion global bloqueada unicamente por tres pruebas Resend preexistentes.
