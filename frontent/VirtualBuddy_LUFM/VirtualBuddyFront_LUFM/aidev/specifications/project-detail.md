# Spec: Detalle de proyecto

## Estado

VERIFIED el 7 de septiembre de 2026. Aprobada mediante decisiones explicitas del usuario durante la solicitud de implementacion.

## Intencion

Permitir que un usuario autenticado abra un proyecto desde el Dashboard y consulte su informacion completa para obtener contexto de incorporacion y trabajo.

## Alcance

- Convertir cada tarjeta del Dashboard en un enlace accesible al detalle del proyecto.
- Incorporar la ruta protegida `/projects/:id` dentro del layout privado existente.
- Consultar el proyecto mediante `GET {apiBaseUrl}/api/project/{id}`.
- Mostrar nombre, descripcion, estado, tecnologias, tiempo de desarrollo, equipo y, cuando exista, informacion de arquitectura.
- Representar estados de carga, exito, datos parciales, proyecto no encontrado y error recuperable.
- Permitir volver al Dashboard y reintentar una consulta fallida.
- Basar la presentacion visual en `aidev/context/ui/DetailProject.png`, conservando el encabezado privado actual.

Esta spec reemplaza exclusivamente las restricciones de `project-home-list.md` que definian las tarjetas como no interactivas y excluian la navegacion al detalle. El resto de esa spec permanece vigente.

## Fuera de alcance

- Implementar o mostrar el bloque `Consulta con IA`, su mensaje de respuesta o una navegacion simulada.
- Gestionar documentos.
- Crear, editar o eliminar proyectos.
- Modificar miembros, tecnologias o arquitectura.
- Mostrar el acronimo o la imagen del proyecto en esta pantalla.
- Introducir roles o permisos nuevos.
- Modificar el backend .NET.

## Contrato verificado

### Fuente

El contrato fue validado en modo solo lectura en `C:\Users\EXLUFM\source\repos\CareerPlan\VirtualBuddy_LUFM\dotnet\VirtualBuddy_LUFM`.

### Solicitud

`GET {apiBaseUrl}/api/project/{id}`

- Requiere `Authorization: Bearer {token}`.
- `id` es el identificador `Guid` recibido previamente en `GET /api/project`.

### Respuesta exitosa

La respuesta es un objeto JSON directo, sin envelope:

```ts
interface ProjectDto {
  id: string;
  name: string;
  acronym: string | null;
  description: string;
  developmentTime: string;
  status: 0 | 1 | 2 | 3 | 4;
  urlImage: string | null;
  architectureInfo: string | null;
  technologies: Array<{ id: string; name: string }>;
  members: Array<{ userId: string; fullName: string; role: string }>;
}
```

- `developmentTime` serializa un `DateTime` y representa la fecha de inicio desde la cual se calcula el tiempo transcurrido.
- El orden de `technologies` y `members` no esta garantizado; la UI conserva el orden recibido.

### Estados

- `0`: `Desconocido`.
- `1`: `Activo`.
- `2`: `Inactivo`.
- `3`: `En revision`.
- `4`: `Completado`.

### Errores relevantes

- `401`: sesion ausente, invalida, vencida o invalidada en el backend; el cuerpo no esta garantizado.
- `404`: no existe un proyecto para el identificador solicitado; responde `ProblemDetails`.
- Otros fallos HTTP, de red o identificadores no aceptados por el endpoint representan una consulta fallida recuperable.

## Requisitos funcionales

1. Cada tarjeta del Dashboard debe ser un unico enlace operable por teclado a `/projects/{project.id}` sin agregar controles interactivos anidados.
2. Al abrir `/projects/:id` con una sesion vigente, la pantalla debe solicitar una vez `GET {apiBaseUrl}/api/project/{id}` mediante la configuracion HTTP y autenticacion existentes.
3. Mientras la solicitud esta pendiente, la pantalla debe comunicar `Cargando proyecto...`.
4. Al recibir el proyecto, debe mostrar su `name`, `description` y la etiqueta en espanol correspondiente a `status`.
5. Debe mostrar todas las tecnologias, conservando el orden recibido. Si no hay tecnologias, debe mostrar `No hay tecnologias registradas.`.
6. Debe mostrar todos los miembros con nombre y rol, conservando el orden recibido y sin duplicar a la persona que tenga un rol de liderazgo. Si no hay miembros, debe mostrar `No hay miembros registrados.`.
7. El tiempo de desarrollo se calcula en meses calendario completos desde `developmentTime` hasta la fecha local actual.
8. Una duracion de cero meses completos debe mostrarse como `Menos de 1 mes`; una duracion de un mes como `1 mes`; y las demas como `{n} meses`.
9. Si `developmentTime` no es una fecha valida o esta en el futuro, debe mostrar `No disponible`.
10. Si `architectureInfo` tiene contenido no vacio, debe mostrar una seccion `Arquitectura`; si es nulo, vacio o contiene solo espacios, debe omitir esa seccion.
11. Ante un `404`, debe mostrar `Proyecto no encontrado.` y un enlace `Volver al Dashboard`, sin ofrecer reintento.
12. Ante cualquier otro error de consulta, debe mostrar `No fue posible cargar el proyecto.` y un boton `Reintentar`.
13. `Reintentar` debe iniciar una nueva solicitud y volver a representar el estado de carga sin recargar la pagina.
14. La pantalla debe incluir un enlace `Volver al Dashboard` que navegue a `/`.
15. La ruta debe conservar el encabezado privado, la identidad visible cuando hay espacio, el cierre de sesion y la proteccion de autenticacion existentes.

## Criterios visuales y de accesibilidad

- Mantener de la referencia una columna central, enlace de regreso, franja roja superior con icono documental decorativo, tarjeta principal blanca, chip de estado y agrupacion clara de informacion.
- La informacion de tecnologias y tiempo debe reorganizarse en una columna cuando no exista ancho suficiente.
- Los miembros pueden incluir iniciales decorativas derivadas de su nombre, pero el nombre completo y el rol deben permanecer como texto.
- La seccion opcional de arquitectura debe integrarse en la tarjeta principal y respetar la misma jerarquia visual.
- Los estados de carga, no encontrado y error deben usar regiones anunciables apropiadas.
- Enlaces y botones deben tener foco visible y areas de interaccion suficientes.
- La pantalla debe funcionar sin desplazamiento horizontal desde 320 px.
- No usar las imagenes de referencia como assets de produccion ni agregar una libreria de iconos.

## Criterios de aceptacion

1. Dada una tarjeta del Dashboard, al activarla se navega a la URL del proyecto correspondiente.
2. Dada una URL directa protegida con un ID, se realiza una unica consulta autenticada al endpoint de detalle de ese ID.
3. Dada una consulta pendiente, se informa el estado de carga.
4. Dado un proyecto existente, se presentan sus datos, estado traducido, tecnologias, duracion y miembros en el orden recibido.
5. Dada informacion de arquitectura no vacia, se muestra; cuando no existe, no se renderiza su seccion.
6. Dadas tecnologias o miembros vacios, se muestran sus respectivos mensajes y la pantalla permanece utilizable.
7. Dadas fechas de inicio validas, se aplican singular, plural y el caso menor a un mes; una fecha invalida o futura muestra `No disponible`.
8. Dado un `404`, se presenta el estado de proyecto no encontrado con regreso al Dashboard y sin reintento.
9. Dado otro fallo, se presenta el error recuperable; un reintento exitoso reemplaza el error por el detalle sin recargar la pagina.
10. El detalle y la tarjeta enlazable son operables por teclado, tienen foco visible y funcionan desde 320 px sin desplazamiento horizontal.
11. El encabezado privado, la proteccion de ruta y el cierre de sesion conservan su comportamiento verificado.
12. La pantalla no muestra ni simula `Consulta con IA`.

## Verificacion

- Pruebas del caso de uso para delegacion por ID: aprobadas.
- Pruebas del adaptador HTTP para endpoint, mapeo completo, `404` y propagacion de errores: aprobadas.
- Pruebas de la tarjeta para semantica y URL de navegacion: aprobadas.
- Pruebas visibles del detalle para carga, contenido, colecciones vacias, arquitectura opcional, `404`, error y reintento: aprobadas.
- Pruebas puras de duracion para singular, plural, menos de un mes, fecha invalida y fecha futura: aprobadas.
- Prettier de los archivos modificados: aprobado.
- Prettier completo: bloqueado por 41 archivos preexistentes fuera del alcance.
- Build de produccion: aprobado.
- Suite Angular completa sin watch: 45 pruebas aprobadas en 15 archivos.
