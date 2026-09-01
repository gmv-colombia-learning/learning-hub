# Specification

## Metadata

- Title: Almacenamiento de imagen de proyecto en Supabase Storage
- Type: FEATURE
- Module: Project
- Status: IMPLEMENTED

## Summary

Permitir almacenar en Supabase Storage una imagen asociada a cada proyecto y guardar su URL de acceso en el campo existente `Project.UrlImage`.

## Context

El módulo `Project` ya contiene el campo obligatorio `UrlImage` y lo expone en sus DTO de creación, actualización y consulta. Actualmente los endpoints reciben una URL como texto y no ofrecen una operación para cargar la imagen.

El proyecto ya dispone de una integración con Supabase Storage mediante `IFileStorageService` y `SupabaseStorageService`, utilizada por el flujo de documentos. La configuración actual utiliza un único bucket y genera URLs firmadas temporales.

## Scope

### In Scope

- Recibir una imagen asociada a un proyecto mediante la API autenticada.
- Permitir crear un proyecto sin imagen y cargar o reemplazar su imagen posteriormente mediante una única operación dedicada.
- Cargar la imagen en Supabase Storage.
- Guardar en `Project.UrlImage` la URL definida para acceder a la imagen.
- Devolver el proyecto con el valor actualizado de `UrlImage`.
- Validar el archivo conforme a los formatos y tamaño definidos.
- Gestionar el reemplazo y la eliminación de las imágenes administradas.
- Almacenar una URL pública y estable de la imagen.
- Configurar Supabase sin almacenar nuevas credenciales en el código fuente.

### Out of Scope

- Implementar componentes o pantallas frontend.
- Editar, redimensionar, comprimir o transformar imágenes, salvo confirmación expresa.
- Migrar a Supabase las URLs de imágenes que ya estén almacenadas en proyectos existentes.
- Modificar el almacenamiento o indexación de documentos, excepto una adaptación compartida estrictamente necesaria y sin cambiar su comportamiento.
- Crear o administrar el proyecto de Supabase desde la aplicación.

## Current Behavior

- `Project.UrlImage` es un `string` obligatorio persistido en PostgreSQL.
- `POST api/Project`, `PUT api/Project/{id}` y `PATCH api/Project/{id}` reciben `UrlImage` como texto dentro de JSON.
- No existe un endpoint para cargar o reemplazar la imagen de un proyecto.
- La infraestructura ya puede cargar y eliminar archivos en el bucket de Supabase configurado.
- El servicio actual devuelve rutas de almacenamiento al cargar y puede generar URLs firmadas con una vigencia predeterminada de una hora.
- La eliminación de un proyecto solo elimina el registro de base de datos y no elimina archivos de Supabase.

## Expected Behavior

- Un usuario autenticado puede cargar una imagen para un proyecto existente.
- Después de una carga exitosa, la imagen queda almacenada en Supabase y `Project.UrlImage` contiene su URL pública estable.
- La API devuelve el proyecto actualizado mediante un DTO.
- Una carga inválida o fallida no debe dejar `Project.UrlImage` apuntando a un archivo inexistente.

## Functional Requirements

### FR-001

La API debe permitir cargar una imagen asociada a un proyecto existente e identificado por su `Id`.

### FR-002

El sistema debe almacenar la imagen en Supabase Storage y actualizar `Project.UrlImage` únicamente después de una carga exitosa.

### FR-003

La respuesta exitosa debe contener el DTO del proyecto con `UrlImage` actualizado.

### FR-004

El sistema debe rechazar la carga cuando el proyecto no exista.

### FR-005

El acceso al endpoint debe requerir la autenticación ya aplicada al módulo `Project`.

### FR-006

La creación del proyecto debe conservar su contrato JSON y permitir que `UrlImage` sea nulo.

### FR-007

La API debe ofrecer una única operación `PUT api/Project/{id}/image` para cargar por primera vez o reemplazar la imagen de un proyecto existente.

### FR-008

Los contratos JSON existentes de creación, actualización completa y actualización parcial deben continuar disponibles para no romper sus consumidores actuales.

### FR-009

`Project.UrlImage` debe almacenar la URL pública y estable del objeto de Supabase, no una URL firmada temporal.

### FR-010

La imagen debe ser opcional al crear un proyecto; el proyecto puede existir con `UrlImage` nulo hasta que se cargue una imagen posteriormente.

### FR-011

Los contratos JSON existentes deben continuar permitiendo una URL externa en `UrlImage`. La limpieza de objetos solo debe aplicarse a URLs que pertenezcan al bucket de imágenes configurado y hayan sido administradas por esta feature.

### FR-012

Una actualización de datos que omita `UrlImage` debe conservar la imagen vigente del proyecto.

## Business Rules

### BR-001

Cada proyecto puede tener como máximo una imagen vigente referenciada por `UrlImage`.

### BR-002

Solo se permiten imágenes JPEG, PNG y WebP de hasta 5 MB.

### BR-003

Al reemplazar la imagen, el sistema debe eliminar o sobrescribir el objeto anterior para que no quede huérfano. Al eliminar el proyecto, también debe eliminar su imagen administrada por esta feature.

### BR-004

La lectura de la imagen será pública para quien conozca su URL; la carga y modificación seguirán requiriendo autenticación en la API.

### BR-005

Un fallo al limpiar una imagen administrada no debe ocultarse como una eliminación completa y exitosa; el sistema debe informar el fallo sin perder silenciosamente la referencia vigente.

### BR-006

El sistema no debe intentar eliminar de Supabase una imagen referenciada mediante una URL externa.

## Inputs

- Identificador del proyecto.
- Archivo de imagen enviado como `multipart/form-data`.

## Outputs

- DTO del proyecto con la URL de imagen actualizada.
- Cuando el proyecto no tenga imagen, `UrlImage` será nulo.

## Validations

- El identificador debe corresponder a un proyecto existente.
- El archivo debe existir y no estar vacío.
- El archivo no debe superar 5 MB.
- La extensión, el tipo MIME y la firma del contenido deben corresponder a JPEG, PNG o WebP.

## Errors

- Proyecto inexistente: error estándar `404 Not Found` del proyecto.
- Archivo ausente, vacío, con formato distinto de JPEG/PNG/WebP o mayor de 5 MB: error estándar `400 Bad Request`.
- Fallo de Supabase: `503 Service Unavailable` mediante el `ProblemDetails` estándar, sin actualizar el proyecto.
- Fallo de persistencia después de cargar el archivo: la estrategia de compensación debe evitar archivos huérfanos cuando sea técnicamente posible.

## Edge Cases

- El proyecto no existe.
- El archivo está vacío.
- La extensión no coincide con el tipo de contenido.
- Supabase falla durante la carga.
- La base de datos falla después de cargar el archivo.
- Dos cargas simultáneas para el mismo proyecto.
- Reemplazo de una imagen existente.
- Eliminación de un proyecto que tiene una imagen almacenada.
- Fallo de Supabase al limpiar una imagen durante un reemplazo o eliminación.
- Proyectos existentes cuyo `UrlImage` contiene una URL externa.

## Dependencies

- Proyecto y credenciales operativas de Supabase proporcionados por entorno.
- Bucket público de Supabase Storage `virtualbuddybucket` para imágenes de proyectos, configurable por entorno.
- Abstracción de almacenamiento de archivos de la capa Application.
- Persistencia existente de `Project.UrlImage`.
- Cambio de nulabilidad de `Project.UrlImage` en dominio, DTO y persistencia.

## Affected Flows

- Consulta del proyecto, que seguirá devolviendo `UrlImage`.
- Actualización de la información básica del proyecto.
- Configuración y registro del almacenamiento de Supabase.
- Eliminación del proyecto, que también limpiará la imagen administrada en Supabase.
- Contratos JSON existentes de creación, PUT y PATCH, que continuarán admitiendo URLs externas manuales.
- Operación dedicada de imagen, sin variantes multipart adicionales para crear, actualizar completamente o actualizar parcialmente el proyecto.

## Non-Functional Requirements

- La clave de Supabase no debe incorporarse al código fuente ni exponerse al cliente.
- La carga debe ejecutarse mediante la API autenticada.
- Los nombres o rutas generados deben evitar colisiones entre proyectos y archivos.
- La implementación debe mantener Supabase como detalle de infraestructura y no introducir su SDK en Domain o Application.
- No deben registrarse credenciales de Supabase.

## Acceptance Criteria

### AC-001

Given un proyecto existente y un archivo de imagen válido
When un usuario autenticado carga la imagen y Supabase está disponible
Then el archivo se almacena, `Project.UrlImage` se actualiza y la API devuelve el proyecto con una URL utilizable.

### AC-002

Given un identificador que no corresponde a un proyecto
When se intenta cargar una imagen
Then la API responde `404 Not Found`, no carga el archivo y no modifica la base de datos.

### AC-003

Given un archivo ausente, vacío o inválido
When se intenta cargar como imagen del proyecto
Then la API responde `400 Bad Request`, no carga el archivo y conserva el valor anterior de `UrlImage`.

### AC-004

Given un proyecto existente
When Supabase falla al cargar la imagen
Then la operación informa un error, conserva el valor anterior de `UrlImage` y no persiste un cambio parcial.

### AC-005

Given un proyecto con una imagen vigente
When se carga una imagen nueva
Then la imagen anterior se elimina o sobrescribe y solo la nueva imagen queda referenciada por `UrlImage`.

### AC-006

Given un proyecto existente con o sin imagen
When un usuario autenticado envía una imagen válida mediante `PUT api/Project/{id}/image`
Then la imagen se carga o reemplaza y la respuesta contiene el proyecto con `UrlImage` actualizado.

### AC-007

Given un consumidor de los contratos JSON existentes
When crea, actualiza completamente o actualiza parcialmente un proyecto sin enviar un archivo
Then los endpoints continúan aceptando el contrato existente sin una ruptura causada por esta feature.

### AC-008

Given un proyecto cuya imagen fue almacenada por esta feature
When el proyecto se elimina exitosamente
Then su imagen también se elimina de Supabase y no queda como archivo huérfano.

### AC-009

Given datos válidos de un proyecto sin imagen
When el usuario crea el proyecto
Then la creación es exitosa y `UrlImage` queda nulo hasta una carga posterior.

### AC-010

Given un proyecto con imagen
When se actualizan sus datos sin enviar `UrlImage`
Then la imagen existente se conserva y no se elimina de Supabase.

## Required Tests

- Carga exitosa y actualización de `UrlImage`.
- Proyecto inexistente sin llamada de carga.
- Archivo ausente o vacío.
- Cada formato permitido y un formato no permitido.
- Tamaño en el límite permitido y por encima del límite.
- Fallo de Supabase sin cambio en `UrlImage`.
- Fallo de persistencia con compensación del archivo cargado.
- Reemplazo de imagen y limpieza del objeto anterior.
- Eliminación del proyecto y de su imagen administrada.
- Regresión de consulta y actualización de proyectos.
- Conservación de URLs externas y ausencia de intentos de eliminarlas desde Supabase.
- Validación de configuración de Supabase al iniciar la aplicación.

## Open Questions

- Ninguna.

## Implementation Notes

- Se conservará el CRUD JSON existente y habrá un único endpoint multipart `PUT api/Project/{id}/image` para cargar o reemplazar la imagen.
- La configuración del bucket de imágenes `virtualbuddybucket` se mantendrá separada de la configuración del bucket de documentos.
- La abstracción actual está ligada a un único bucket; podría requerir una adaptación mínima para seleccionar el bucket sin introducir Supabase fuera de infraestructura.
- Cada carga utilizará una ruta única bajo el proyecto; la URL nueva se persistirá antes de limpiar la versión anterior y una carga no persistida se compensará eliminando el objeto nuevo.
- Se deberá obtener la URL pública estable del objeto; no se reutilizará para `UrlImage` el método actual que genera URLs firmadas de una hora.
- La configuración sensible debe suministrarse mediante variables de entorno, secretos de usuario o el gestor de secretos del entorno de despliegue.
- La nulabilidad del campo requiere una migración de base de datos y actualización de los contratos correspondientes.
- La detección de imágenes administradas deberá basarse en la ruta/URL del bucket configurado, sin efectuar eliminaciones sobre hosts externos.

## Change Log

- 2026-08-31: Spec DRAFT creada a partir de la solicitud y del análisis del módulo `Project` y la integración Supabase existente.
- 2026-08-31: Se confirmó que la carga estará disponible tanto en creación/actualización como mediante un endpoint separado, conservando los contratos JSON existentes.
- 2026-08-31: Se confirmó que las imágenes tendrán acceso público y `UrlImage` almacenará una URL estable.
- 2026-08-31: Se limitaron las cargas a JPEG, PNG y WebP de hasta 5 MB, con validación del contenido real.
- 2026-08-31: Se confirmó la limpieza de la imagen anterior al reemplazar y de la imagen vigente al eliminar el proyecto.
- 2026-08-31: Se indicó el bucket `virtualbuddybucket` para almacenar las imágenes de proyectos sin cambiar el bucket de documentos.
- 2026-08-31: Se confirmó que un proyecto podrá crearse sin imagen y cargarla posteriormente.
- 2026-08-31: Se confirmó que los contratos JSON conservarán soporte para URLs externas, excluidas de la limpieza de Supabase.
- 2026-08-31: Spec validada sin preguntas abiertas y marcada READY para implementación.
- 2026-08-31: Implementación completada con endpoints JSON y multipart, almacenamiento público, validación de archivos, compensación, limpieza y migración de nulabilidad.
- 2026-08-31: Compilación y 23 pruebas de Project superadas; verificación contra Supabase real pendiente. La suite global conserva tres fallos ajenos en `ResendEmailSenderTests` por una API key inválida.
- 2026-09-01: Alcance simplificado por solicitud del usuario: CRUD JSON de Project y una única operación PUT para cargar o reemplazar la imagen; spec marcada READY.
- 2026-09-01: Simplificación implementada; se eliminaron tres variantes `with-image`, se conservó el CRUD JSON y quedó una única operación `PUT api/Project/{id}/image`. Compilación y 28 pruebas de Project superadas.
