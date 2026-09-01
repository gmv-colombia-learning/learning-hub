# Specification

## Metadata

- Title: Aceptar imágenes válidas enviadas como application/octet-stream
- Type: BUG
- Module: Project
- Status: IMPLEMENTED

## Summary

Permitir cargar imágenes JPEG, PNG y WebP válidas cuando el cliente multipart declare el tipo genérico `application/octet-stream`, sin debilitar la validación del contenido.

## Context

Postman puede enviar un archivo de imagen como `application/octet-stream`. La validación actual rechaza el archivo antes de comprobar su firma, aunque su extensión y contenido correspondan a uno de los formatos permitidos.

## Scope

### In Scope

- Reconocer `application/octet-stream` como tipo de transporte genérico para extensiones permitidas.
- Detectar el MIME real mediante la extensión y firma binaria.
- Enviar a Supabase el MIME normalizado detectado.
- Mantener el límite de 5 MB y los formatos JPEG, PNG y WebP.

### Out of Scope

- Admitir formatos adicionales.
- Aceptar archivos arbitrarios declarados como `application/octet-stream`.
- Modificar autenticación, endpoints o almacenamiento de documentos.

## Current Behavior

- Una imagen válida declarada como `application/octet-stream` produce `400 Bad Request` con el detalle `Project image must be a JPEG, PNG, or WebP file.`

## Expected Behavior

- Una imagen con extensión permitida y firma válida se acepta aunque llegue como `application/octet-stream`.
- El archivo se sube con su MIME real normalizado.
- Una extensión o firma inválida continúa siendo rechazada sin llamar a Supabase.

## Functional Requirements

### FR-001

El sistema debe inferir el formato esperado desde `.jpg`, `.jpeg`, `.png` o `.webp` cuando el MIME recibido sea `application/octet-stream`.

### FR-002

El formato inferido debe confirmarse mediante la firma binaria antes de cargar el archivo.

### FR-003

Supabase debe recibir `image/jpeg`, `image/png` o `image/webp` según el formato confirmado.

## Business Rules

### BR-001

`application/octet-stream` no amplía los formatos permitidos ni sustituye la validación de firma.

## Inputs

- Archivo multipart con MIME `application/octet-stream`.
- Nombre con extensión JPEG, PNG o WebP.

## Outputs

- El mismo DTO de proyecto definido por la feature de imágenes.

## Validations

- Se conservan el límite de 5 MB, las extensiones permitidas y la validación de firma.
- Un MIME específico de imagen debe seguir coincidiendo con la extensión y firma.

## Errors

- Extensión no permitida o firma incompatible: `400 Bad Request` estándar.
- Los errores de Supabase conservan el comportamiento vigente.

## Edge Cases

- JPEG enviado como octet-stream con extensión `.jpg` o `.jpeg`.
- PNG o WebP enviado como octet-stream.
- Archivo ejecutable renombrado con extensión de imagen.
- Extensión permitida cuya firma corresponde a otro formato.

## Dependencies

- Validación existente de `ProjectImageService`.

## Affected Flows

- Todos los endpoints multipart de imagen de `Project` que reutilizan `ProjectImageService`.

## Non-Functional Requirements

- No leer ni cargar archivos mayores de 5 MB.
- No reducir las validaciones de seguridad existentes.

## Acceptance Criteria

### AC-001

Given un JPEG válido con extensión `.jpg` y MIME `application/octet-stream`
When se carga como imagen de proyecto
Then se acepta y Supabase recibe `image/jpeg`.

### AC-002

Given un PNG o WebP válido con MIME `application/octet-stream`
When se carga como imagen de proyecto
Then se acepta y Supabase recibe su MIME normalizado.

### AC-003

Given un archivo con extensión permitida pero firma inválida
When se carga como `application/octet-stream`
Then se responde `400 Bad Request` y no se llama a Supabase.

### AC-004

Given un archivo con extensión no permitida
When se carga como `application/octet-stream`
Then se responde `400 Bad Request` y no se llama a Supabase.

## Required Tests

- JPEG, PNG y WebP válidos como octet-stream.
- Firma inválida como octet-stream.
- Extensión inválida como octet-stream.
- Regresión de MIME específicos válidos e inválidos.

## Open Questions

- Ninguna.

## Implementation Notes

- Centralizar la resolución del MIME efectivo en `ProjectImageService`.
- No confiar únicamente en extensión ni MIME declarado por el cliente.

## Change Log

- 2026-09-01: Bug reproducido mediante Postman y spec creada en estado READY.
- 2026-09-01: Corrección implementada y validada con 16 pruebas específicas y 28 pruebas del módulo Project. La suite global conserva tres fallos ajenos en Resend por una API key inválida.
