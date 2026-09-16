# mf-auth

Microfrontend remoto (remote) construido con Angular 21, Webpack clasico y Module Federation.

## Proposito

Este proyecto implementa el dominio de autenticacion como una pieza desacoplada que puede ser consumida por un host (`demo-mf-host`) sin recompilar el host cada vez.

La meta es mostrar una configuracion de microfrontend basada en:

- Webpack clasico mediante `ngx-build-plus` + `extraWebpackConfig`.
- Module Federation mediante `@angular-architects/module-federation`.
- Exposicion de un modulo remoto reutilizable (`./Module`) con `remoteEntry.js`.

## Stack y componentes clave

- Angular 21
- Webpack clasico (archivo `webpack.config.js`)
- Module Federation (`withModuleFederationPlugin`)
- Builder de Angular extendido con `ngx-build-plus`

## Arquitectura Microfrontend

### Remote (este proyecto: `mf-auth`)

- Nombre del remote: `mf-auth`
- Puerto local: `4201`
- URL del manifest remoto: `http://localhost:4201/remoteEntry.js`
- Modulo expuesto: `./Module`
- Archivo real expuesto: `./src/app/remote-entry/remote-entry-module.ts`

Configuracion en `webpack.config.js`:

```js
module.exports = withModuleFederationPlugin({
	name: 'mf-auth',
	exposes: {
		'./Module': './src/app/remote-entry/remote-entry-module.ts',
	},
	shared: {
		...shareAll({ singleton: true, strictVersion: true, requiredVersion: 'auto' }),
	},
});
```

### Host (`demo-mf-host`)

El host carga dinamicamente el remote con `loadRemoteModule` en su routing:

```ts
loadRemoteModule({
	type: 'module',
	remoteEntry: 'http://localhost:4201/remoteEntry.js',
	exposedModule: './Module',
}).then((m) => m.RemoteEntryModule);
```

Ruta de acceso en el host: `/mf-auth`.

## Webpack clasico: como esta configurado

Este proyecto no usa el flujo por defecto del builder moderno para bundling federado. En su lugar:

- `angular.json` usa `ngx-build-plus:browser` para `build`.
- `angular.json` usa `ngx-build-plus:dev-server` para `serve`.
- Se inyecta Webpack personalizado con:
	- `extraWebpackConfig: webpack.config.js` (desarrollo)
	- `extraWebpackConfig: webpack.prod.config.js` (produccion)

Esta estrategia permite mantener una configuracion explicita y controlada de federation sobre Webpack.

## Requisitos

- Node.js (version LTS recomendada)
- npm

## Instalacion

Desde la carpeta del proyecto:

```bash
npm install
```

***TIP: (Cuando el proyecto está desde 0) Para instalar Module Federation y configurar el proyecto como remote con Webpack clasico, ejecuta en la raíz del proyecto:***

```bash
npm install -D @angular-architects/module-federation ngx-build-plus
ng add @angular-architects/module-federation --project mf-auth --type remote --port 4201 --stack webpack
```

## Ejecucion en desarrollo

### 1) Levantar solo el remote (`mf-auth`)

```bash
npm start
```

Disponible en: `http://localhost:4201/`

### 2) Levantar host + remote para ver integracion

En este proyecto:

```bash
npm run run:all
```

O de forma manual, en terminales separadas:

1. En `mf-auth`: `npm start`
2. En `demo-mf-host`: `npm start`

Luego abrir el host en `http://localhost:4200/` y navegar a `/mf-auth`.

## Build

```bash
npm run build
```

Salida en: `dist/mf-auth`

## Tests

```bash
npm test
```

## Estructura relevante

- `webpack.config.js`: definicion de Module Federation (name, exposes, shared).
- `webpack.prod.config.js`: reutiliza la configuracion base para produccion.
- `src/app/remote-entry/remote-entry-module.ts`: modulo expuesto al host.
- `angular.json`: builders y puertos (`4201`) para el remote.

## Notas

- Se habilita `Access-Control-Allow-Origin: *` en `serve` para facilitar carga remota en entorno local.
- Dependencias Angular compartidas se configuran como `singleton` para evitar duplicados en runtime.
