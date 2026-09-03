# Ambientes de ejecucion

La API dispone inicialmente de dos ambientes:

- `Local`: usa una instancia PostgreSQL instalada y administrada localmente.
- `Development`: usa PostgreSQL del proyecto de desarrollo en Supabase.

## Configurar las conexiones

Cada ambiente contiene su conexion completa en el archivo correspondiente:

- `appsettings.Local.json`: `ConnectionStrings:Local`.
- `appsettings.Development.json`: `ConnectionStrings:Development`.

Cada perfil selecciona automaticamente la cadena con su mismo nombre. `ConnectionStrings:DefaultConnection` permanece disponible como fallback.

## Ejecutar Local

```powershell
dotnet run --launch-profile Local
```

La API queda disponible en `http://localhost:5089` con `ASPNETCORE_ENVIRONMENT=Local`.

## Ejecutar Development

```powershell
dotnet run --launch-profile Development
```

La API queda disponible en `http://localhost:5090` con `ASPNETCORE_ENVIRONMENT=Development` y utiliza la conexion configurada para Supabase.
