# Spec: Tabs de detalle y consulta con IA

## Estado

VERIFIED el 8 de septiembre de 2026. Aprobada mediante decisiones explicitas del usuario durante la solicitud de implementacion.

## Intencion

Permitir que un usuario final autenticado alterne entre la informacion de un proyecto y una consulta asistida por IA sin abandonar su pagina de detalle.

## Alcance

- Incorporar los tabs `Detalle del Proyecto` y `Consultar con IA` en la ruta protegida `/projects/:id`.
- Conservar sin cambios funcionales la tarjeta de detalle definida por `project-detail.md`.
- Mostrar debajo del detalle una tarjeta visual de resumen generado por IA con contenido demostrativo temporal.
- Permitir preguntas reales sobre el proyecto mediante `POST {apiBaseUrl}/api/AI/chat/{projectId}`.
- Conservar en memoria el historial de preguntas y respuestas mientras el usuario permanece en la pagina.
- Representar el envio pendiente, el error recuperable y el reintento de una pregunta fallida.
- Basar la presentacion visual en `aidev/context/ui/tabsDetail&Chat.png` y `aidev/context/ui/GestionDocumentos&Consultaia.png`, adaptandola al estilo actual.

Esta spec reemplaza exclusivamente la exclusion de `Consulta con IA` y el criterio de aceptacion 12 de `project-detail.md`. El resto de esa spec permanece vigente.

## Fuera de alcance

- Generar, consultar, actualizar o persistir el resumen real del proyecto.
- Persistir o recuperar conversaciones al recargar, abandonar o volver a la pagina.
- Enviar el historial como contexto al backend.
- Renderizar Markdown o HTML devuelto por la IA.
- Gestionar, subir, descargar, indexar o eliminar documentos.
- Agregar controles de administracion, roles o permisos nuevos.
- Modificar el backend .NET.

## Contrato verificado

### Fuente

El contrato fue validado en modo solo lectura en `C:\Users\EXLUFM\source\repos\CareerPlan\VirtualBuddy_LUFM\dotnet\VirtualBuddy_LUFM`.

### Solicitud

`POST {apiBaseUrl}/api/AI/chat/{projectId}`

- Requiere `Authorization: Bearer {token}`.
- `projectId` es el identificador `Guid` del proyecto abierto.
- El cuerpo JSON es `{ "question": string }`.
- El backend rechaza preguntas nulas, vacias o compuestas solo por espacios con `400` y el texto `La pregunta no puede estar vacia.`.

### Respuesta exitosa

La respuesta es un objeto JSON directo:

```ts
interface ProjectAiResponseDto {
  response: string;
}
```

Cada solicitud es independiente. El backend no recibe ni conserva el historial visible del frontend.

### Errores relevantes

- `401`: sesion ausente, invalida o vencida; el cuerpo no esta garantizado.
- `400`: pregunta vacia rechazada por el backend.
- Otros fallos HTTP, de red, del proveedor de IA o tiempos de espera representan una consulta fallida recuperable.

## Requisitos funcionales

1. El detalle exitoso debe mostrar dos tabs: `Detalle del Proyecto` y `Consultar con IA`.
2. `Detalle del Proyecto` debe estar seleccionado inicialmente y conservar la franja roja, la tarjeta y toda la informacion definida en `project-detail.md`.
3. Debajo de la tarjeta existente debe mostrarse una tarjeta `Resumen generado por IA` con contenido demostrativo que informe que la generacion real se integrara posteriormente.
4. Mostrar el resumen demostrativo no debe realizar solicitudes adicionales al backend.
5. Al activar `Consultar con IA`, debe mostrarse el historial de la sesion, un campo con el placeholder `Escribe tu pregunta sobre el proyecto...` y una accion accesible `Enviar pregunta`.
6. El tab de consulta no debe realizar una solicitud hasta que el usuario envie una pregunta valida.
7. Una pregunta compuesta solo por espacios no debe enviarse y la accion debe permanecer deshabilitada.
8. Al enviar una pregunta valida, la UI debe quitar espacios al inicio y al final, agregarla al historial y realizar una solicitud al endpoint usando el ID de la ruta.
9. Mientras una consulta esta pendiente, debe comunicar `Consultando...` y bloquear nuevos envios para evitar solicitudes concurrentes.
10. Una respuesta exitosa debe agregarse despues de su pregunta y presentarse como texto, conservando saltos de linea sin interpretar HTML o Markdown.
11. Las preguntas y respuestas deben conservarse al alternar entre ambos tabs mientras la pagina siga abierta.
12. Ante un error debe mostrarse `No fue posible obtener una respuesta.` asociado a la pregunta fallida y una accion `Reintentar`.
13. `Reintentar` debe repetir la pregunta fallida una sola vez por activacion, sin duplicar su mensaje en el historial y bloqueando envios concurrentes durante la nueva solicitud.
14. Un reintento exitoso debe reemplazar el error por la respuesta recibida.
15. El historial no debe persistir al abandonar o recargar la pagina ni enviarse como contexto en solicitudes posteriores.
16. Los estados de carga, no encontrado y error del detalle deben conservar el comportamiento definido por `project-detail.md`; los tabs solo aparecen para un proyecto cargado exitosamente.

## Criterios visuales y de accesibilidad

- Integrar los tabs en la columna central actual y conservar la identidad roja de la aplicacion.
- Mantener la tarjeta de detalle existente sin reemplazarla por la composicion de la referencia.
- Presentar el resumen como una tarjeta oscura separada debajo del detalle.
- Diferenciar visualmente preguntas, respuestas y errores sin depender solo del color.
- Los tabs, campo, envio y reintento deben ser operables por teclado y tener nombres y foco visibles.
- Los cambios de estado del chat deben usar regiones anunciables apropiadas.
- La composicion debe funcionar sin desplazamiento horizontal desde 320 px.
- No usar las referencias visuales como assets de produccion ni agregar librerias de iconos o Markdown.

## Criterios de aceptacion

1. Dado un proyecto cargado, se muestran dos tabs accesibles y el detalle esta seleccionado inicialmente.
2. Dado el tab de detalle, se conserva todo su contenido verificado y se agrega debajo el resumen demostrativo sin solicitudes adicionales.
3. Dado el tab de consulta sin interaccion, no se llama al endpoint de IA.
4. Dada una pregunta valida, se envia una unica solicitud autenticada con el ID, el metodo y el cuerpo definidos por el contrato.
5. Dada una pregunta vacia o una consulta pendiente, no puede iniciarse un nuevo envio.
6. Dada una respuesta exitosa, la pregunta y respuesta se muestran en orden y se conservan al cambiar de tab.
7. Dadas varias consultas, el historial conserva el orden de cada pregunta y su respuesta durante la permanencia en la pagina.
8. Dado un fallo, se muestra el error aprobado; un reintento exitoso reemplaza ese error sin duplicar la pregunta.
9. Las respuestas se presentan como texto seguro y conservan sus saltos de linea.
10. Los tabs y el chat son operables por teclado, anuncian sus estados y funcionan desde 320 px sin desplazamiento horizontal.
11. Los flujos de carga, no encontrado, error, reintento, regreso y autenticacion del detalle no sufren regresiones.

## Verificacion

- Pruebas enfocadas de caso de uso, adaptador HTTP, chat y detalle: 13 pruebas aprobadas en 4 archivos.
- Pruebas visibles del chat para estado inicial, validacion, carga, exito, historial, bloqueo concurrente, error y reintento: aprobadas.
- Prueba de regresion del envio nativo del formulario sin recarga de pagina: aprobada.
- Pruebas visibles del detalle para tabs, resumen demostrativo, preservacion del contenido y estados preexistentes: aprobadas.
- Prettier de los archivos modificados: aprobado.
- Prettier completo: bloqueado por 47 archivos preexistentes fuera del alcance.
- Build de produccion sin warnings: aprobado.
- Suite Angular completa sin watch: 55 pruebas aprobadas en 18 archivos.
