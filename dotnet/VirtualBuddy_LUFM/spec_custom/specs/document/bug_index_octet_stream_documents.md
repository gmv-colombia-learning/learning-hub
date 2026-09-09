# Specification

## Metadata

- Title: Indexar documentos soportados enviados como application/octet-stream
- Type: BUG
- Module: Document / AI Indexing
- Status: IMPLEMENTED

## Summary

Permitir que los documentos PDF, Word, Excel y TXT soportados por el parser se clasifiquen e indexen cuando el cliente multipart los envie con el MIME generico `application/octet-stream`.

## Context

Durante una carga en el ambiente `Development`, un PDF valido con extension `.pdf` y MIME `application/octet-stream` fue creado con tipo `Unknown` y no genero registros en `KnowledgeChunks`. La conexion con Azure OpenAI fue validada separadamente y responde correctamente para chat y embeddings de 768 dimensiones.

El proyecto ya contempla un caso equivalente para imagenes de proyecto en `spec_custom/specs/project/bug_accept_octet_stream_project_image.md`. Ese antecedente aplica al tratamiento del MIME generico, pero no modifica el alcance ni las reglas de documentos.

## Scope

### In Scope

- Reconocer `application/octet-stream` como MIME de transporte generico para documentos con extensiones ya soportadas por `DocumentParserService`.
- Resolver el MIME efectivo de `.pdf`, `.docx`, `.xls`, `.xlsx` y `.txt`.
- Clasificar esos archivos como `DocumentType.File`.
- Usar el MIME efectivo al almacenar el archivo y persistir el documento.
- Ejecutar la indexacion automatica vigente para esos documentos.
- Mantener el mismo comportamiento en `Local` y `Development`; el proveedor de IA y la base vectorial continuan seleccionandose por ambiente.
- Agregar pruebas de regresion para MIME especificos ya soportados y extensiones desconocidas.

### Out of Scope

- Agregar nuevos formatos de documento.
- Implementar Azure AI Search.
- Modificar Azure OpenAI, Ollama, PostgreSQL, Azure SQL o Supabase Storage.
- Cambiar la dimension de embeddings o crear migraciones.
- Implementar el endpoint placeholder de reindexacion.
- Reindexar automaticamente documentos cargados anteriormente.
- Agregar validacion de firma binaria o inspeccion profunda de archivos, dado que el flujo actual de documentos no la exige.
- Cambiar el comportamiento vigente cuando falla el parser, la generacion de embeddings o la persistencia de chunks.

## Current Behavior

- `UploadDocument` decide si un documento es soportado usando el `ContentType` declarado por el cliente.
- `MapContentTypeToDocumentType` reconoce PDF, Word y texto, pero no `application/octet-stream` ni los MIME de Excel.
- Un PDF valido enviado como `application/octet-stream` se persiste con `DocumentType.Unknown`.
- La condicion `document.Type == DocumentType.File && isSupportedByAI` resulta falsa.
- `IndexDocument.ExecuteAsync` no se invoca, Azure OpenAI u Ollama no reciben texto y no se crean `KnowledgeChunks`.
- El endpoint puede responder `201 Created` porque la carga y persistencia del documento son independientes de la indexacion.

## Expected Behavior

- Un archivo con MIME `application/octet-stream` y extension `.pdf`, `.docx`, `.xls`, `.xlsx` o `.txt` debe resolverse al MIME especifico correspondiente.
- El documento debe persistirse como `DocumentType.File` y con el MIME efectivo.
- El archivo debe enviarse al almacenamiento con el MIME efectivo.
- La indexacion automatica debe ejecutarse igual que para un documento enviado originalmente con un MIME especifico soportado.
- Una extension no soportada no debe habilitar la indexacion por declarar `application/octet-stream`.

## Functional Requirements

### FR-001

Cuando el MIME declarado sea `application/octet-stream`, el sistema debe resolver el MIME efectivo mediante la extension normalizada del nombre de archivo.

### FR-002

El mapeo de extensiones debe ser:

- `.pdf` -> `application/pdf`.
- `.docx` -> `application/vnd.openxmlformats-officedocument.wordprocessingml.document`.
- `.xlsx` -> `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`.
- `.xls` -> `application/vnd.ms-excel`.
- `.txt` -> `text/plain`.

### FR-003

Los documentos resueltos mediante FR-002 deben clasificarse como `DocumentType.File`.

### FR-004

El MIME efectivo debe utilizarse para la carga en `IFileStorageService`, la creacion de la entidad `Document`, el DTO de respuesta y la decision de indexacion.

### FR-005

La indexacion debe continuar delegandose a `IndexDocument` y a las interfaces `IAIService` e `IKnowledgeBaseService` existentes.

### FR-006

Los MIME especificos actualmente soportados deben conservar su comportamiento.

## Business Rules

### BR-001

`application/octet-stream` solo debe habilitar la normalizacion para extensiones que el parser ya soporta.

### BR-002

Una extension desconocida o no soportada debe conservarse fuera del flujo de indexacion automatica.

### BR-003

La seleccion del proveedor permanece determinada por el ambiente: Ollama y PostgreSQL en `Local`; Azure OpenAI y Azure SQL en `Development`.

## Inputs

- Archivo multipart no vacio.
- Nombre de archivo con extension.
- MIME declarado por el cliente, incluido `application/octet-stream`.
- Identificador de un proyecto existente.

## Outputs

- El mismo `DocumentResponseDto` del endpoint de carga.
- Para extensiones soportadas, `Type` igual a `File` y `ContentType` igual al MIME efectivo.
- Chunks generados por el flujo de indexacion vigente cuando el parser extrae texto.

## Validations

- La comparacion de MIME y extension debe ser insensible a mayusculas y minusculas.
- Solo se deben normalizar las cinco extensiones incluidas en FR-002.
- El proyecto debe continuar existiendo antes de almacenar el documento.
- Las validaciones actuales del parser, embeddings y vector store se mantienen sin cambios.

## Errors

- Los errores existentes de proyecto inexistente, almacenamiento y persistencia deben conservarse.
- Los errores ocurridos dentro de la indexacion deben conservar el manejo vigente: registrar el tipo de excepcion sin exponer secretos y permitir que finalice la carga del documento.
- Esta correccion no introduce un nuevo formato de error HTTP.

## Edge Cases

- Extension soportada escrita con mayusculas, por ejemplo `.PDF` o `.DOCX`.
- Archivo `.xls` y archivo `.xlsx` enviados como `application/octet-stream`.
- Archivo sin extension enviado como `application/octet-stream`.
- Archivo con extension desconocida enviado como `application/octet-stream`.
- MIME especifico soportado enviado con una extension soportada.
- Parser que devuelve texto vacio para un archivo soportado.
- Fallo de Azure OpenAI u Ollama despues de persistir el documento.

## Dependencies

- `UploadDocument`.
- `DocumentParserService`.
- `IndexDocument`.
- `IFileStorageService`.
- `IAIService`.
- `IKnowledgeBaseService`.
- Spec `spec_custom/specs/infrastructure/spec_azure_openai_development.md`.
- Spec relacionada `spec_custom/specs/project/bug_accept_octet_stream_project_image.md` como antecedente de normalizacion de MIME generico.

## Affected Flows

- `POST /api/projects/{projectId}/documents`.
- Almacenamiento de documentos en Supabase Storage.
- Persistencia de la entidad `Document`.
- Indexacion automatica al cargar documentos.
- Generacion y persistencia de embeddings.

## Non-Functional Requirements

- No exponer claves, cadenas de conexion ni contenido completo de documentos en logs o errores.
- No duplicar el pipeline de indexacion ni agregar proveedores nuevos.
- Mantener la correccion dentro del modulo Document y reutilizar las interfaces existentes.
- Evitar llamadas de IA para extensiones no soportadas.

## Acceptance Criteria

### AC-001

Given un PDF valido con extension `.pdf` y MIME `application/octet-stream`
When se carga mediante el endpoint de documentos
Then se almacena con `application/pdf`, se clasifica como `File` y se invoca la indexacion automatica.

### AC-002

Given un DOCX con MIME `application/octet-stream`
When se carga mediante el endpoint de documentos
Then se normaliza al MIME de Word, se clasifica como `File` y se invoca la indexacion automatica.

### AC-003

Given un XLS o XLSX con MIME `application/octet-stream`
When se carga mediante el endpoint de documentos
Then se normaliza al MIME correspondiente, se clasifica como `File` y se invoca la indexacion automatica.

### AC-004

Given un TXT con MIME `application/octet-stream`
When se carga mediante el endpoint de documentos
Then se normaliza a `text/plain`, se clasifica como `File` y se invoca la indexacion automatica.

### AC-005

Given un archivo con extension no soportada y MIME `application/octet-stream`
When se carga mediante el endpoint de documentos
Then no se clasifica como `File` por esta regla y no se invoca la indexacion automatica.

### AC-006

Given un documento con un MIME especifico actualmente soportado
When se carga mediante el endpoint
Then conserva el comportamiento previo de almacenamiento, clasificacion e indexacion.

### AC-007

Given la aplicacion ejecutada en Development
When un documento soportado produce texto durante la indexacion
Then sus embeddings se generan mediante Azure OpenAI y se persisten mediante `SqlServerKnowledgeBaseService` en Azure SQL.

### AC-008

Given la aplicacion ejecutada en Local
When un documento soportado produce texto durante la indexacion
Then conserva Ollama y PostgreSQL sin cambios funcionales.

## Required Tests

- Teoria para `.pdf`, `.docx`, `.xls`, `.xlsx` y `.txt` enviados como `application/octet-stream`, verificando MIME efectivo, `DocumentType.File` e indexacion.
- Extension en mayusculas enviada como `application/octet-stream`.
- Extension desconocida y archivo sin extension, verificando que no se indexen.
- Regresion de los MIME especificos soportados.
- Regresion de seleccion de `IKnowledgeBaseService` e IA por ambiente.
- Suite automatizada existente.
- Smoke test Development con un PDF real enviado como `application/octet-stream`, verificando la existencia de `KnowledgeChunks` asociados al documento.

## Reproduction

1. Ejecutar la API con el perfil `Development`.
2. Enviar un PDF valido al endpoint `POST /api/projects/{projectId}/documents` declarando `Content-Type: application/octet-stream` en la parte del archivo multipart.
3. Observar una respuesta `201 Created` cuyo documento queda clasificado como `Unknown`.
4. Consultar `KnowledgeChunks` por el identificador del documento.
5. Observar que no existen chunks porque la indexacion no fue invocada.

## Diagnosis

### Symptom

- El documento se crea, pero aparece como `Unknown` y no tiene chunks.

### Root Cause

- `UploadDocument` utiliza exclusivamente el MIME declarado para clasificar y decidir si indexa.
- `application/octet-stream` no coincide con los MIME soportados y produce `DocumentType.Unknown`.
- La condicion que invoca `IndexDocument` resulta falsa antes de llegar al parser o al proveedor de IA.

### Proposed Solution

- Resolver un MIME efectivo por extension solo cuando el MIME declarado sea `application/octet-stream` y la extension pertenezca al conjunto soportado.
- Reutilizar ese MIME efectivo en almacenamiento, entidad, DTO y seleccion de indexacion.
- Mantener sin cambios el pipeline posterior de parser, embeddings y vector store.

## Open Questions

- Ninguna.

## Implementation Notes

- La resolucion del MIME debe mantenerse cerca del caso de uso de carga de documentos; no corresponde reutilizar `ProjectImageService`, cuyo dominio, formatos y validacion de firmas son diferentes.
- Conviene derivar la decision de indexacion del MIME efectivo o del tipo resuelto una sola vez para evitar las dos listas inconsistentes actuales.
- El comentario de `IndexDocument` que menciona exclusivamente PostgreSQL esta desactualizado; el almacenamiento real depende de `IKnowledgeBaseService` y puede corregirse sin alterar comportamiento.

## Implementation Progress

- `UploadDocument` resuelve el MIME efectivo una sola vez para `application/octet-stream` y extensiones soportadas.
- El MIME efectivo se utiliza en Supabase Storage, entidad, DTO, clasificacion y seleccion de indexacion.
- `MapContentTypeToDocumentType` y la decision de indexacion comparten la misma regla para documentos soportados.
- PDF, DOCX, XLS, XLSX y TXT quedan clasificados como `DocumentType.File` con MIME normalizado.
- Extensiones desconocidas o ausentes conservan `application/octet-stream`, tipo `Unknown` y no invocan IA.
- Se agregaron 12 casos de carga entre normalizacion, MIME especificos y extensiones no soportadas.
- Las 14 pruebas de `DocumentUseCasesTests` pasan.
- Suite global: 88 de 91 pruebas superadas. Permanecen tres fallos preexistentes y ajenos en `ResendEmailSenderTests` por API key invalida.
- `git diff --check` no reporta errores.
- La API Development inicio correctamente durante la preparacion del smoke test. La herramienta termino el proceso por timeout del lanzador en segundo plano antes de ejecutar la carga HTTP, por lo que la spec no se marca `VERIFIED`.

## Change Log

- 2026-09-09: Bug reportado con un PDF valido enviado como `application/octet-stream`, respuesta exitosa, tipo `Unknown` y ausencia de chunks.
- 2026-09-09: Azure OpenAI y Azure SQL descartados como causa inicial; el flujo se detiene antes de invocar `IndexDocument`.
- 2026-09-09: El usuario confirmo que la correccion debe cubrir todos los formatos ya soportados por el parser.
- 2026-09-09: Spec completada sin preguntas funcionales abiertas y marcada `READY`.
- 2026-09-09: Implementacion iniciada.
- 2026-09-09: Correccion y pruebas automatizadas completadas; spec marcada `IMPLEMENTED`.
