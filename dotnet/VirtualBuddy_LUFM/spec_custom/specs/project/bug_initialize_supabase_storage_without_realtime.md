# Specification

## Metadata

- Title: Inicializar Supabase Storage sin Realtime
- Type: BUG
- Module: Project / Document
- Status: IMPLEMENTED

## Summary

Permitir que documentos e imágenes utilicen Supabase Storage sin iniciar una conexión WebSocket de Realtime innecesaria.

## Context

El proyecto anterior de Supabase caducó y fue reemplazado por uno nuevo con los buckets `documents` y `virtualbuddybucket`. La carga falla con `503` porque el cliente intenta conectar Realtime y recibe `401` durante el handshake WebSocket.

## Scope

### In Scope

- Usar la URL base del nuevo proyecto Supabase.
- Desactivar Realtime y renovación automática de sesión en el cliente usado por Storage.
- Mantener `documents` para documentos y `virtualbuddybucket` para imágenes.

### Out of Scope

- Cambiar endpoints, formatos de archivo, RAG o autenticación de VirtualBuddy.
- Crear buckets o políticas RLS desde la API.

## Current Behavior

- La URL configurada incluye `/rest/v1/`.
- El cliente intenta iniciar Realtime y la operación termina en `503` por un `401` WebSocket.

## Expected Behavior

- Storage se inicializa sin conectar Realtime.
- Los flujos de documentos e imágenes usan sus buckets configurados.

## Functional Requirements

### FR-001

El cliente Supabase usado por Storage no debe conectar Realtime ni renovar una sesión de usuario.

### FR-002

La configuración debe usar la URL base del proyecto Supabase.

## Business Rules

### BR-001

Los documentos deben usar `documents` y las imágenes deben usar `virtualbuddybucket`.

## Inputs

- Configuración `Supabase` existente.

## Outputs

- Operaciones Storage sin intento de conexión WebSocket.

## Validations

- La URL debe ser absoluta y corresponder a la base del proyecto.

## Errors

- Los errores reales de Storage conservan el manejo `503` existente.

## Edge Cases

- Clave sin permisos de escritura según las políticas RLS del proyecto Supabase.

## Dependencies

- Supabase Storage y los buckets configurados.

## Affected Flows

- Carga y eliminación de documentos.
- Carga, reemplazo y eliminación de imágenes de proyectos.

## Non-Functional Requirements

- No exponer claves de backend al frontend.

## Acceptance Criteria

### AC-001

Given una configuración válida de Supabase Storage
When se carga un documento o imagen
Then el cliente no intenta abrir un WebSocket de Realtime.

### AC-002

Given los dos buckets configurados
When se carga un documento o una imagen
Then se usa `documents` o `virtualbuddybucket`, respectivamente.

## Required Tests

- Compilación de la solución.
- Regresión de pruebas de Project.

## Open Questions

- Ninguna.

## Implementation Notes

- Mantener el cambio limitado a opciones del cliente y configuración local.

## Change Log

- 2026-09-01: Bug diagnosticado y spec creada en estado READY.
- 2026-09-01: URL base corregida, Realtime y refresh desactivados; compilación y 28 pruebas de Project superadas.
