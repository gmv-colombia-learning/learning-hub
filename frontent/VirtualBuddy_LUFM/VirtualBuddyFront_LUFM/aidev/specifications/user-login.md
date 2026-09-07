# Spec: Inicio de sesion de usuario

## Estado

VERIFIED el 7 de septiembre de 2026. Aprobada mediante las decisiones del usuario durante la solicitud de implementacion, ajustada por instruccion explicita para no modificar el backend y actualizada para configurar el backend de development mediante proxy Angular.

## Intencion

Permitir que un usuario registrado se autentique contra la API de Virtual Buddy, conserve su sesion en el navegador y acceda a las rutas privadas.

## Alcance

- Pantalla de inicio de sesion responsive basada en la referencia `aidev/context/ui/login.png`.
- Autenticacion mediante correo y contrasena.
- Persistencia de la sesion en `localStorage` hasta cerrar sesion o vencer el JWT.
- Envio automatico del JWT como `Bearer` a las solicitudes dirigidas a la API configurada.
- Proteccion del dashboard para usuarios no autenticados.
- Cierre de sesion desde el area privada.
- Configuraciones frontend independientes para `local`, `dev`, `test` y `prod`.
- Proxies de desarrollo Angular para conectar con las APIs .NET de local y development sin modificar el backend.

## Fuera de alcance

- Registro de usuarios.
- Recuperacion o cambio de contrasena.
- Renovacion del token; el backend no expone refresh token.
- Roles y permisos.
- Definir las URL de API o los origenes frontend de `test` y `prod`, que aun no existen.
- Cambiar el contrato o los mensajes internos del backend.
- Configurar CORS en el backend; cualquier necesidad de despliegue se reportara como nota.

## Contrato verificado

### Solicitud

`POST {apiBaseUrl}/api/Auth/login`

```json
{
  "email": "user@example.com",
  "password": "secret"
}
```

### Respuesta exitosa

```json
{
  "token": "jwt",
  "email": "user@example.com",
  "fullName": "User Name"
}
```

### Errores relevantes

- `400`: contrasena incorrecta, emitida por el backend como `ProblemDetails`.
- `404`: correo no registrado, emitido por el backend como `ProblemDetails`.
- Otros errores HTTP o de red: fallo tecnico de autenticacion.

## Requisitos funcionales

1. La ruta `/login` debe mostrar campos obligatorios de correo y contrasena y un boton para iniciar sesion.
2. El correo debe validarse con formato de email antes de enviar la solicitud.
3. Un formulario invalido no debe enviar solicitudes y debe mostrar mensajes junto a los campos despues de su interaccion o del intento de envio.
4. Mientras se autentica, el boton debe permanecer deshabilitado e indicar el estado de carga; no se permiten envios concurrentes.
5. Una respuesta exitosa debe guardar `token`, `email` y `fullName` en `localStorage` y navegar al dashboard.
6. Los errores `400` y `404` deben mostrar el mismo mensaje: `Correo o contrasena incorrectos.`
7. Los demas fallos deben mostrar: `No fue posible iniciar sesion. Intenta nuevamente.`
8. Una visita a una ruta privada sin una sesion vigente debe redirigir a `/login` y conservar la URL solicitada como destino posterior.
9. Una visita a `/login` con una sesion vigente debe redirigir al dashboard.
10. Una sesion solo es vigente si tiene los datos esperados y el JWT contiene una expiracion futura; una sesion invalida o vencida debe eliminarse.
11. Las solicitudes a la URL base configurada deben incluir `Authorization: Bearer {token}` cuando exista una sesion vigente. No debe enviarse el token a otros origenes.
12. Al cerrar sesion deben eliminarse los datos persistidos y navegar a `/login`.

## Ambientes

- `local`: Angular usa `/backend` y su proxy reenvia a `http://localhost:5089`; es la configuracion usada por `npm start` y `npm run start:local`.
- `dev`: Angular usa `/backend` y su proxy reenvia a `http://localhost:5090`; es la configuracion usada por `npm run start:dev`.
- `test`: URL de API pendiente.
- `prod`: URL de API pendiente y configuracion de produccion por defecto.
- Deben existir configuraciones de build y serve nombradas `local`, `dev`, `test` y `production`.
- Si una URL pendiente no esta configurada, el intento de login debe fallar con el mensaje tecnico definido, sin usar implicitamente otra API.

## Integracion local

- El frontend debe incluir proxies usados unicamente por `ng serve` en los ambientes local y development.
- En local, las solicitudes con prefijo `/backend` deben reenviarse a `http://localhost:5089` eliminando ese prefijo.
- En development, las solicitudes con prefijo `/backend` deben reenviarse a `http://localhost:5090` eliminando ese prefijo.
- El backend no se modifica dentro de esta spec.

## Criterios visuales y de accesibilidad

- Mantener la composicion de tarjeta centrada, identificador visual, titulo, subtitulo, campos y boton de la referencia.
- Adaptar el texto al contrato real: usar `Correo electronico`, no `Usuario`.
- No mostrar el tip de credenciales administrativas de la referencia.
- Usar labels asociados, tipos de input correctos, autocomplete, foco visible, mensajes anunciables y navegacion completa por teclado.
- La pantalla debe funcionar sin desplazamiento horizontal desde 320 px y conservar una longitud de lectura adecuada en escritorio.

## Criterios de aceptacion

1. Dado un formulario valido y una respuesta exitosa, la sesion se persiste y se abre el dashboard.
2. Dados datos invalidos, no se invoca la API y se informa cada error aplicable.
3. Dadas credenciales rechazadas con `400` o `404`, se muestra un mensaje unico sin revelar si existe el correo.
4. Dado un fallo de red o configuracion, se muestra el mensaje tecnico y el formulario puede reintentarse.
5. Dado un usuario anonimo, el dashboard redirige al login y, tras autenticarse, recupera el destino solicitado.
6. Dado un JWT vencido o malformado, la sesion se elimina y se trata al usuario como anonimo.
7. Dada una sesion vigente, el login redirige al dashboard y las solicitudes de la API reciben el Bearer token.
8. Dado el cierre de sesion, se elimina la sesion y las rutas privadas vuelven a estar protegidas.
9. Los builds de los cuatro ambientes compilan y `local` y `dev` usan las APIs .NET confirmadas.
10. `npm start` y `npm run start:local` reenvian `/backend` a `http://localhost:5089`, mientras `npm run start:dev` lo reenvia a `http://localhost:5090`, sin requerir cambios CORS en .NET.

## Nota de despliegue

Cuando existan los hosts de `dev`, `test` y `prod`, la infraestructura que sirva el frontend debe enrutar la API bajo el mismo origen o el backend debera habilitar CORS para los origenes concretos. Ese cambio no forma parte de esta implementacion.

## Verificacion

- Prettier completo: aprobado.
- Builds `local`, `dev`, `test` y produccion: aprobados.
- Suite Angular: 18 pruebas aprobadas.
- Servidor local con configuracion de proxy: inicia correctamente.
