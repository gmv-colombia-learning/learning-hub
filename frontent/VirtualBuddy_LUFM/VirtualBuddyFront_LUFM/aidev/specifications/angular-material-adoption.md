# Spec: Adopcion gradual de Angular Material

## Estado

APPROVED el 8 de septiembre de 2026 mediante decisiones explicitas del usuario durante la solicitud de implementacion.

## Intencion

Incorporar Angular Material como base visual del frontend y aplicarlo de forma gradual a controles interactivos existentes, preservando el comportamiento, la identidad roja y los criterios de accesibilidad ya verificados.

## Alcance

- Instalar Angular Material y Angular CDK en versiones compatibles con Angular 21.2.
- Configurar un tema global claro con color primario rojo, tipografia `Inter` con fuentes de sistema como respaldo y densidad estandar.
- Migrar los campos y el boton de envio del login a componentes y directivas de Angular Material.
- Aplicar botones Material al cierre de sesion y a las acciones de reintento de Home y detalle de proyecto.
- Ajustar estilos y pruebas unicamente donde la adopcion de Material lo requiera.

## Fuera de alcance

- Migrar las tarjetas de proyecto, etiquetas de estado, loaders, enlaces, encabezados o estructura general de las pantallas.
- Cambiar contratos HTTP, autenticacion, reglas de formulario, navegacion o estados de consulta.
- Introducir Angular Material Icons u otra libreria de iconos.
- Introducir dialogos, menus, snackbars, selects u otros componentes Material no requeridos por este alcance.
- Incorporar un tema oscuro o un selector de tema.
- Modificar el backend o los proxies existentes.

## Requisitos

1. Las dependencias de Angular Material y Angular CDK deben ser compatibles con el major 21 instalado y quedar registradas mediante npm y `package-lock.json`.
2. La aplicacion debe aplicar un tema Material global claro que mantenga el rojo como color primario y la familia tipografica actual.
3. El login debe usar campos Material con apariencia delineada para correo y contrasena, conservando labels, placeholders, tipos, autocomplete y formulario reactivo actuales.
4. Los errores de correo obligatorio, formato de correo y contrasena obligatoria deben conservar exactamente sus textos y mostrarse bajo el campo correspondiente.
5. El error general de autenticacion debe conservar su region anunciable y sus mensajes aprobados.
6. El boton de inicio de sesion debe usar un boton Material destacado, conservar el texto de carga y permanecer deshabilitado durante el envio.
7. El cierre de sesion debe usar un boton Material delineado sin cambiar su comportamiento ni la identidad visible del usuario.
8. Los botones `Reintentar` de Home y detalle deben usar botones Material destacados sin modificar los estados, solicitudes o comportamiento de reintento.
9. La implementacion debe usar imports standalone en cada componente y no debe introducir NgModules ni wrappers visuales sin una responsabilidad reutilizable.
10. La migracion debe conservar navegacion por teclado, foco visible, regiones anunciables y funcionamiento sin desplazamiento horizontal desde 320 px.
11. Las tarjetas, chips de estado, loaders, enlaces y composicion de las pantallas deben conservar su implementacion y comportamiento actuales.

## Criterios de aceptacion

1. La aplicacion compila con Angular Material y Angular CDK 21 configurados y sin agregar una libreria de iconos.
2. El login presenta controles Material y conserva todos los escenarios funcionales y mensajes definidos en `user-login.md`.
3. Durante una autenticacion pendiente, el boton Material de inicio permanece deshabilitado y no permite envios concurrentes.
4. El boton Material de cierre de sesion elimina la sesion y navega a `/login` como antes.
5. Cada boton Material `Reintentar` inicia una nueva consulta y permite recuperar el estado exitoso sin recargar la pagina.
6. Home y detalle conservan tarjetas, etiquetas, loaders, enlaces, contenido y semantica fuera de los botones migrados.
7. Las pantallas afectadas mantienen foco visible y se adaptan desde 320 px sin desplazamiento horizontal.
8. Las pruebas enfocadas, el build de produccion y la suite completa finalizan correctamente.

## Verificacion

- Angular Material y Angular CDK 21.2.7 instalados mediante npm.
- Tema Material M3 claro con paleta roja, tipografia configurada y densidad estandar: aprobado.
- Pruebas enfocadas de login, layout privado, Home y detalle: 16 pruebas aprobadas.
- Build de produccion: aprobado con bundle inicial de 269.13 kB, dentro del presupuesto configurado.
- Suite Angular completa sin watch: 47 pruebas aprobadas en 15 archivos.
- Prettier de todos los archivos modificados: aprobado.
- Prettier completo: bloqueado por 49 archivos preexistentes fuera del alcance.
