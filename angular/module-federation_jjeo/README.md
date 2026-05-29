# Angular MFE Demo

Proyecto Angular basado en una arquitectura de **Module Federation**. El objetivo es tener una
aplicación contenedora (**shell**) que orqueste la navegación y consuma distintos
microfrontends independientes.

Actualmente el workspace contiene:

- `shell`: aplicación principal o host.
- `mfe-country-app`: primer microfrontend del proyecto.

La integración de federation se apoya en `@angular-architects/native-federation`, por lo que la
idea es que cada microfrontend pueda evolucionar, compilarse y desplegarse de forma independiente,
mientras el shell actúa como punto de entrada para el usuario.

## Requisitos

- Node.js compatible con Angular 20.
- Angular CLI 20.
- Dependencias instaladas con `npm install`.

## Estructura

```text
projects/
  shell/            # Aplicación contenedora
  mfe-country-app/  # Microfrontend de países
```

## Desarrollo local

Levantar el shell:

```bash
ng serve shell
```

Levantar el microfrontend:

```bash
ng serve mfe-country-app --port 4201
```

Por defecto, Angular usa `http://localhost:4200/` para la aplicación principal. Si se ejecutan
varias aplicaciones al mismo tiempo, asigna puertos distintos para evitar conflictos.

## Build

Compilar todo el workspace:

```bash
ng build
```

Compilar una aplicación específica:

```bash
ng build shell
ng build mfe-country-app
```

Los artefactos se generan en `dist/`.

## Tests

Ejecutar pruebas unitarias:

```bash
ng test
```

También se puede ejecutar una app específica:

```bash
ng test shell
ng test mfe-country-app
```

## Próximos pasos sugeridos

- Definir la configuración de federation para el shell y cada microfrontend.
- Exponer rutas o componentes desde los microfrontends.
- Consumir los remotes desde el shell mediante lazy loading.
- Documentar puertos, nombres de remotes y contratos compartidos cuando se agreguen nuevos MFEs.
