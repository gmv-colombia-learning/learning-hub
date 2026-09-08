# Ambientes de ejecucion

La API dispone inicialmente de dos ambientes:

- `Local`: usa una instancia PostgreSQL instalada y administrada localmente.
- `Development`: usa Azure SQL Database para persistencia relacional y RAG.

Supabase se mantiene como almacenamiento de documentos e imagenes en ambos ambientes.

## Preparar HTTPS local

Los perfiles `Local` y `Development` publican exclusivamente HTTPS mediante el certificado de desarrollo de ASP.NET Core. Antes del primer arranque, verifique y confie el certificado en el equipo:

```powershell
dotnet dev-certs https --check --trust
```

Si no existe un certificado valido, genere y confie uno:

```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

No se almacenan certificados, claves privadas ni contrasenas de certificados en el repositorio.

## Configurar las conexiones

Cada ambiente contiene su conexion completa en el archivo correspondiente:

- `appsettings.Local.json`: `ConnectionStrings:Local`.
- `appsettings.Development.json`: `ConnectionStrings:Development`.

Cada perfil selecciona exclusivamente la cadena con su mismo nombre. No existe fallback entre proveedores.

La seccion `Ollama` debe declarar `EmbeddingDimension`. El valor debe coincidir con el modelo de embeddings y con la dimension `VECTOR(n)` de las migraciones. Cambiarlo requiere una migracion y reindexar los documentos.

## Aplicar migraciones

Las migraciones nunca se aplican durante el arranque. Deben ejecutarse explicitamente desde la raiz de la solucion.

PostgreSQL Local:

```powershell
dotnet ef database update --project VirtualBuddy.Infraestructure --startup-project VirtualBuddy.Api --context BuddyDBContext -- --provider PostgreSql
```

Azure SQL Development:

```powershell
dotnet ef database update --project VirtualBuddy.Migrations.SqlServer --startup-project VirtualBuddy.Api --context BuddyDBContext -- --provider SqlServer
```

No se deben intercambiar los proyectos ni los valores de `--provider`.

## Ejecutar Local

```powershell
dotnet run --launch-profile Local
```

La API queda disponible exclusivamente en `https://localhost:7089` con `ASPNETCORE_ENVIRONMENT=Local`.

## Ejecutar Development

```powershell
dotnet run --launch-profile Development
```

La API queda disponible exclusivamente en `https://localhost:7090` con `ASPNETCORE_ENVIRONMENT=Development` y utiliza Azure SQL para todos los datos, incluidos los embeddings. Si la conexion falla o existen migraciones pendientes, la API no inicia.

## Preparar Azure SQL

1. Crear una base Azure SQL vacia con SQL Authentication.
2. Autorizar en el firewall unicamente la IP que ejecutara migraciones y la API.
3. Mantener `Encrypt=True` y `TrustServerCertificate=False`.
4. Confirmar soporte para `VECTOR(n)` y `VECTOR_DISTANCE`.
5. Colocar la cadena ADO.NET en `ConnectionStrings:Development`.
6. Aplicar las migraciones SQL Server antes del primer arranque.
7. Verificar `__EFMigrationsHistory` y la tabla `KnowledgeChunks`.
8. Iniciar Development para ejecutar el seed y realizar pruebas funcionales.
