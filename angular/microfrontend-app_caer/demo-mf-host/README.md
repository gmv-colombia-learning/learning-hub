# demo-mf-host

Aplicacion host (shell) construida con Angular 21 para orquestar microfrontends remotos con Webpack clasico y Module Federation.

## Proposito

Este proyecto actua como contenedor principal de la aplicacion. Su responsabilidad es resolver rutas, cargar remotos de forma dinamica y componer la experiencia final sin acoplarse al ciclo de build de cada microfrontend.

La configuracion se centra en:

- Webpack clasico con `ngx-build-plus`.
- Module Federation con `@angular-architects/module-federation`.
- Integracion runtime del remote `mf-auth` mediante `remoteEntry.js`.

## Stack y componentes clave

- Angular 21
- Webpack clasico (archivo `webpack.config.js`)
- Module Federation (`withModuleFederationPlugin`)
- Builder extendido con `ngx-build-plus`

## Arquitectura Microfrontend

### Host (este proyecto: `demo-mf-host`)

- Puerto local: `4200`
- Remote registrado: `mf-auth`
- URL del remote: `http://localhost:4201/remoteEntry.js`
- Ruta integrada en host: `/mf-auth`

Configuracion en `webpack.config.js`:

```js
module.exports = withModuleFederationPlugin({
	remotes: {
		'mf-auth': 'mf-auth@http://localhost:4201/remoteEntry.js',
	},
	shared: {
		...shareAll({ singleton: true, strictVersion: true, requiredVersion: 'auto' }),
	},
});
```

Carga dinamica en routing (`src/app/app.routes.ts`):

```ts
{
	path: 'mf-auth',
	loadChildren: () =>
		loadRemoteModule({
			type: 'module',
			remoteEntry: 'http://localhost:4201/remoteEntry.js',
			exposedModule: './Module',
		}).then((m) => m.RemoteEntryModule),
}
```

### Remote consumido

El microfrontend remoto esperado es `mf-auth`, que expone `./Module` desde su `remoteEntry.js`.

## Webpack clasico: como esta configurado

Para mantener control explicito del bundling federado:

- `angular.json` usa `ngx-build-plus:browser` para `build`.
- `angular.json` usa `ngx-build-plus:dev-server` para `serve`.
- Se aplica Webpack custom via:
	- `extraWebpackConfig: webpack.config.js` (desarrollo)
	- `extraWebpackConfig: webpack.prod.config.js` (produccion)

## Requisitos

- Node.js (version LTS recomendada)
- npm

## Instalacion

Desde la carpeta del proyecto:

```bash
npm install
```

***TIP: (Cuando el proyecto está desde 0) Para instalar Module Federation y configurar el proyecto como host con Webpack clasico, ejecuta en la raíz del proyecto:***

```bash
npm install -D @angular-architects/module-federation ngx-build-plus
ng add @angular-architects/module-federation --project demo-mf-host --type host --port 4200 --stack webpack
```

## Ejecucion en desarrollo

### 1) Levantar solo el host

```bash
npm start
```

Disponible en: `http://localhost:4200/`

### 2) Levantar host + remote para integracion completa

En este proyecto:

```bash
npm run run:all
```

O de forma manual, en terminales separadas:

1. En `mf-auth`: `npm start` (puerto `4201`)
2. En `demo-mf-host`: `npm start` (puerto `4200`)

Luego abrir `http://localhost:4200/mf-auth`.

## Build

```bash
npm run build
```

Salida en: `dist/demo-mf-host`

## Tests

```bash
npm test
```

## Estructura relevante

- `webpack.config.js`: registro de remotos y dependencias compartidas.
- `webpack.prod.config.js`: reutiliza configuracion base para produccion.
- `src/app/app.routes.ts`: carga lazy del remote con `loadRemoteModule`.
- `angular.json`: builders (`ngx-build-plus`) y puerto `4200`.

## Notas

- Angular y librerias base se comparten como `singleton` para evitar multiples instancias en runtime.
- Si `mf-auth` no esta disponible en `4201`, la ruta `/mf-auth` del host no podra resolver el remoto.
