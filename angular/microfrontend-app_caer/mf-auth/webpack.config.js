const { shareAll, withModuleFederationPlugin } = require('@angular-architects/module-federation/webpack');

module.exports = withModuleFederationPlugin({
  name: 'mf-auth', // ← nombre único del remote

  exposes: {
    // Alias : ruta real del archivo a exponer
    './Module': './src/app/remote-entry/remote-entry-module.ts',
    // Si usas standalone components en Angular 21:
    // './Component': './src/app/remote-entry/entry.component.ts',
  },

  shared: {
    ...shareAll({
      singleton: true,
      strictVersion: true,
      requiredVersion: 'auto'
    }),
  },
});
