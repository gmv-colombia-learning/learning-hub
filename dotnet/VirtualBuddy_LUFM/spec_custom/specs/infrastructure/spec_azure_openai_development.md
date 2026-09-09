# Specification

## Metadata

- Title: Azure OpenAI para RAG en Development
- Type: FEATURE
- Module: Infrastructure / AI Integration
- Status: IMPLEMENTED

## Summary

Usar Azure OpenAI para chat y generacion de embeddings en el ambiente `Development`, manteniendo Ollama sin cambios funcionales en `Local` y conservando las bases vectoriales existentes.

## Context

El prototipo ya utiliza Semantic Kernel con Ollama y persiste embeddings en PostgreSQL para `Local` y Azure SQL para `Development`. Se requiere consumir los deployments de Azure OpenAI ya creados en Development sin incorporar Azure AI Search.

## Scope

### In Scope

- Mantener Ollama como proveedor de chat y embeddings en `Local`.
- Usar Azure OpenAI como proveedor de chat y embeddings en `Development`.
- Usar el deployment `gpt-4.1-mini-1` para chat.
- Usar el deployment `text-embedding-3-small` para embeddings.
- Solicitar embeddings de 768 dimensiones para conservar el esquema vectorial existente.
- Mantener PostgreSQL como vector store de `Local` y Azure SQL como vector store de `Development`.
- Almacenar temporalmente la configuracion y API key de Azure OpenAI en `appsettings.Development.json` por decision explicita para el prototipo.
- Documentar la configuracion y la necesidad de reindexar los documentos de Development.

### Out of Scope

- Incorporar Azure AI Search.
- Cambiar endpoints o contratos de la API.
- Cambiar PostgreSQL, Azure SQL o Supabase Storage.
- Crear migraciones de base de datos.
- Extraer secretos a Key Vault, variables de entorno o identidades administradas.
- Automatizar la migracion o reindexacion de embeddings existentes.
- Eliminar Ollama del ambiente `Local`.

## Current Behavior

- Ambos ambientes registran Ollama para chat y embeddings.
- La dimension se obtiene de `Ollama:EmbeddingDimension`.
- Development persiste vectores de 768 dimensiones en Azure SQL.

## Expected Behavior

- `Local` continua resolviendo los servicios de IA mediante Ollama.
- `Development` resuelve chat y embeddings mediante los deployments configurados de Azure OpenAI.
- Ninguna solicitud de IA de Development depende de una instancia Ollama.
- El flujo RAG conserva Azure SQL como almacenamiento y busqueda vectorial.

## Functional Requirements

### FR-001

La infraestructura debe seleccionar el proveedor de IA usando el ambiente de ejecucion.

### FR-002

Local debe conservar la configuracion y los conectores Ollama actuales.

### FR-003

Development debe registrar los conectores de chat y embeddings de Azure OpenAI con el endpoint, API key y nombres de deployment configurados.

### FR-004

Azure OpenAI debe generar embeddings de exactamente 768 dimensiones.

### FR-005

La seleccion del proveedor de IA no debe modificar la seleccion del proveedor de persistencia.

## Business Rules

### BR-001

Los embeddings generados por Ollama y Azure OpenAI no se deben mezclar, aunque tengan la misma dimension.

### BR-002

Los documentos existentes en Development deben reindexarse antes de validar busquedas semanticas con Azure OpenAI.

## Inputs

- `ASPNETCORE_ENVIRONMENT` con valor `Local` o `Development`.
- Configuracion `Ollama` para Local.
- Configuracion `AzureOpenAI` para Development.

## Outputs

- Respuestas de chat generadas por el proveedor correspondiente al ambiente.
- Embeddings de 768 dimensiones generados por el proveedor correspondiente al ambiente.

## Validations

- El endpoint de Azure OpenAI debe ser una URL HTTPS absoluta.
- La API key y ambos nombres de deployment deben existir en Development.
- La dimension debe ser positiva, soportada por Azure SQL e igual a 768 para el esquema actual.
- Los errores de configuracion no deben mostrar la API key.

## Errors

- Development debe fallar durante el arranque cuando la configuracion de Azure OpenAI sea incompleta o invalida.
- Los errores remotos de Azure OpenAI deben conservar el manejo estandar de errores del proyecto.

## Edge Cases

- Ejecutar Development sin API key o deployment configurado.
- Usar por error el endpoint de proyecto de Foundry en lugar del endpoint del recurso OpenAI.
- Consultar chunks generados previamente por Ollama.
- Recibir un embedding con una dimension distinta de 768.

## Dependencies

- Microsoft Semantic Kernel.
- Conectores Semantic Kernel para Ollama y Azure OpenAI.
- Recurso y deployments existentes en Azure OpenAI.
- PostgreSQL Local y Azure SQL Development existentes.

## Affected Flows

- Arranque y registro de dependencias.
- Carga e indexacion de documentos.
- Busqueda semantica y chat RAG.

## Non-Functional Requirements

- Mantener el cambio limitado a infraestructura y configuracion.
- No registrar API keys ni incluirlas en mensajes de error.
- Evitar abstracciones adicionales para solo dos ambientes conocidos.

## Acceptance Criteria

### AC-001

Given la aplicacion ejecutada en Local
When se construye el Kernel
Then se registran Ollama chat y embeddings con la configuracion Local.

### AC-002

Given la aplicacion ejecutada en Development con configuracion valida
When se construye el Kernel
Then se registran Azure OpenAI chat y embeddings y no se requiere Ollama.

### AC-003

Given el deployment `text-embedding-3-small`
When Development genera un embedding
Then solicita y obtiene exactamente 768 dimensiones compatibles con Azure SQL.

### AC-004

Given una configuracion Azure OpenAI ausente o invalida
When inicia Development
Then el inicio falla con un mensaje que identifica la seccion sin revelar secretos.

### AC-005

Given el cambio de proveedor de IA
When se ejecutan Local y Development
Then Local conserva PostgreSQL y Development conserva Azure SQL como vector stores.

## Required Tests

- Seleccion de configuracion de embeddings por ambiente.
- Validacion de configuracion Azure OpenAI incompleta o invalida.
- Resolucion del Kernel en Local y Development.
- Suite automatizada existente.
- Smoke test real de embedding y chat en Development cuando los recursos externos esten disponibles.

## Open Questions

- Ninguna.

## Implementation Notes

- El endpoint de proyecto recibido fue `https://<resource>.services.ai.azure.com/api/projects/<project>`; Semantic Kernel usara el endpoint de recurso `https://<resource>.openai.azure.com`.
- `AddAzureOpenAIEmbeddingGenerator` permite indicar `dimensions: 768` para modelos `text-embedding-3`.
- No se requiere migracion porque Azure SQL ya utiliza `VECTOR(768)`.

## Implementation Progress

- Seleccion de proveedor implementada: Ollama para `Local` y Azure OpenAI para `Development`.
- Configuracion Azure OpenAI validada durante el arranque sin incluir la API key en errores.
- Conector de chat registrado con `gpt-4.1-mini-1`.
- Conector de embeddings registrado con `text-embedding-3-small` y `dimensions: 768`.
- PostgreSQL Local y Azure SQL Development permanecen sin cambios.
- Smoke test directo completado correctamente contra Azure OpenAI: embedding de 768 dimensiones y respuesta de chat recibida.
- Las 12 pruebas de configuracion de infraestructura pasan.
- Suite global: 76 de 79 pruebas superadas. Permanecen tres fallos preexistentes y ajenos en `ResendEmailSenderTests` por API key invalida.
- No se marca `VERIFIED` hasta reindexar documentos de Development y ejecutar el flujo RAG completo mediante la API.
- No se elimino documentacion anterior: la revision no encontro notas temporales; los archivos existentes son reglas, contexto, documentacion operativa o specs que conservan decisiones funcionales.

## Change Log

- 2026-09-09: Spec creada con alcance confirmado: Ollama Local, Azure OpenAI Development, sin Azure AI Search y secretos temporalmente en settings.
- 2026-09-09: Se confirmaron los deployments de chat y embeddings y se conserva la dimension 768.
- 2026-09-09: Implementacion iniciada.
- 2026-09-09: Implementacion y smoke tests Azure completados; spec marcada `IMPLEMENTED`.
