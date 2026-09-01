# Spec: Listado de proyectos en Home

## Estado

VERIFIED el 1 de septiembre de 2026. Aprobada mediante decisiones explicitas del usuario durante la solicitud de implementacion y actualizada para mostrar una imagen de respaldo cuando un proyecto no tenga una imagen disponible.

## Intencion

Permitir que un usuario autenticado consulte desde Home todos los proyectos registrados en Virtual Buddy y conozca su estado actual.

## Alcance

- Reemplazar el placeholder actual de Home por un listado responsive de proyectos basado visualmente en `aidev/context/ui/home.png`.
- Consultar todos los proyectos mediante el endpoint autenticado `GET {apiBaseUrl}/api/project`.
- Representar cada proyecto en una tarjeta con imagen, nombre, descripcion y estado.
- Representar estados de carga, exito, vacio y error.
- Permitir reintentar la consulta despues de un error.
- Conservar el encabezado privado existente, la identidad del usuario y el cierre de sesion.

## Fuera de alcance

- Filtrar, buscar, ordenar o paginar proyectos.
- Navegar al detalle de un proyecto o implementar su pantalla.
- Mostrar o implementar el acceso a Administracion.
- Crear, editar o eliminar proyectos.
- Mostrar tecnologias, miembros, tiempo de desarrollo o informacion de arquitectura.
- Introducir roles o permisos.
- Modificar el backend .NET.

## Contrato verificado

### Fuente

El contrato fue validado en modo solo lectura en `C:\Users\EXLUFM\source\repos\CareerPlan\VirtualBuddy_LUFM\dotnet\VirtualBuddy_LUFM`.

### Solicitud

`GET {apiBaseUrl}/api/project`

- Requiere `Authorization: Bearer {token}`.
- No acepta filtros, paginacion ni ordenamiento funcionales.

### Respuesta exitosa

La respuesta es un arreglo JSON directo, sin envelope:

```ts
interface ProjectDto {
  id: string;
  name: string;
  acronym: string | null;
  description: string;
  developmentTime: string;
  status: 0 | 1 | 2 | 3 | 4;
  urlImage: string;
  architectureInfo: string | null;
  technologies: Array<{ id: string; name: string }>;
  members: Array<{ userId: string; fullName: string; role: string }>;
}
```

- `200` con `[]` representa que no hay proyectos.
- El backend devuelve todos los proyectos, sin restringirlos al usuario o al estado activo.
- El orden de proyectos y colecciones anidadas no esta garantizado.

### Estados

- `0`: `Desconocido`.
- `1`: `Activo`.
- `2`: `Inactivo`.
- `3`: `En revision`.
- `4`: `Completado`.

### Errores relevantes

- `401`: sesion ausente, invalida, vencida o invalidada en el backend; el cuerpo no esta garantizado.
- `500` y fallos HTTP o de red: indisponibilidad de la consulta.
- Home presenta el mismo estado de error recuperable para cualquier fallo de esta consulta. Cambiar globalmente el tratamiento de una sesion invalidada esta fuera de alcance.

## Requisitos funcionales

1. Al abrir `/` con una sesion vigente, Home debe solicitar una vez `GET {apiBaseUrl}/api/project` mediante la configuracion HTTP y autenticacion existentes.
2. Mientras la solicitud esta pendiente, Home debe comunicar que los proyectos se estan cargando.
3. Al recibir proyectos, Home debe conservar el orden de la respuesta y mostrar una tarjeta por cada elemento.
4. Cada tarjeta debe mostrar `urlImage`, `name`, `description` y la etiqueta en espanol correspondiente a `status`.
5. Las tarjetas son informativas y no deben navegar ni simular acciones en este alcance.
6. El titulo debe referirse a `Proyectos`, no a `Proyectos Activos`, porque la API devuelve todos los estados.
7. Al recibir `[]`, Home debe mostrar `No hay proyectos disponibles.`.
8. Ante cualquier error de consulta, Home debe mostrar `No fue posible cargar los proyectos.` y un boton `Reintentar`.
9. `Reintentar` debe iniciar una nueva solicitud y volver a representar el estado de carga sin recargar la pagina.
10. La implementacion debe preservar el encabezado privado, la identidad visible cuando hay espacio, el cierre de sesion y la proteccion de la ruta.
11. Si `urlImage` esta vacio o la imagen indicada no puede cargarse, la tarjeta debe mostrar `/sin-imagen.png` en su lugar.

## Criterios visuales y de accesibilidad

- Mantener de la referencia la jerarquia de titulo, texto de apoyo y cuadricula de tarjetas con imagen superior, contenido y chip de estado.
- Adaptar la referencia para todos los proyectos y omitir el boton Administracion.
- Usar texto semantico y regiones anunciables para carga, vacio y error.
- El boton de reintento debe ser operable por teclado y tener foco visible.
- Las imagenes deben usar el nombre del proyecto como texto alternativo.
- La imagen de respaldo debe mostrarse centrada, con un tamano menor al area de imagen y sobre un fondo neutro sin color rojo.
- La cuadricula debe adaptarse sin desplazamiento horizontal desde 320 px: una columna en movil y varias cuando exista espacio suficiente.
- No usar la imagen de referencia como asset de produccion ni agregar una libreria de iconos.

## Criterios de aceptacion

1. Dada una sesion vigente, al abrir Home se realiza una unica consulta autenticada a `/api/project`.
2. Dada una consulta pendiente, se informa el estado de carga.
3. Dada una respuesta con proyectos de distintos estados, se muestran todos, en el orden recibido, con imagen, nombre, descripcion y estado traducido.
4. Dada una respuesta vacia, se muestra el mensaje aprobado y no se renderizan tarjetas.
5. Dado un fallo HTTP o de red, se muestra el mensaje aprobado y se ofrece reintentar.
6. Dado un error seguido de un reintento exitoso, se reemplaza el error por el listado sin recargar la pagina.
7. Las tarjetas no navegan y Home no muestra Administracion.
8. El encabezado privado y el cierre de sesion conservan su comportamiento verificado.
9. La pantalla funciona desde 320 px, sin desplazamiento horizontal y con estados accesibles.
10. Un proyecto sin imagen disponible muestra `/sin-imagen.png` centrada, contenida sobre un fondo neutro y no presenta una imagen rota.

## Verificacion

- Pruebas del adaptador HTTP para endpoint, mapeo y errores: aprobadas.
- Pruebas visibles de Home para carga, proyectos, vacio, error y reintento: aprobadas.
- Pruebas de las etiquetas de todos los estados: aprobadas.
- Pruebas de imagen de respaldo para URL vacia y error de carga: aprobadas.
- Prettier completo: aprobado.
- Build de produccion: aprobado.
- Suite Angular completa sin watch: 32 pruebas aprobadas en 12 archivos.
