# Manejo seguro de secretos en este repositorio

## Reglas básicas

- **Nunca subas claves ni secretos de servicios a git.**  
  Usa variables de entorno (.env), nunca valores directos en appsettings.* o código fuente.
- **El archivo `.env` debe existir localmente, pero estar en `.gitignore`.**
- Si necesitas agregar o modificar una clave (por ejemplo de Supabase), genera una nueva en el panel y actualiza sólo tu `.env` (¡no la pegues en código!).
- Si accidentalmente expones una clave, revócala y sigue el proceso de limpieza del historial del repo.

## Ejemplo de archivo `.env`
```env
SUPABASE_URL=https://xxx.supabase.co
SUPABASE_KEY=pon_tu_clave_aqui
SUPABASE_BUCKET_NAME=documents
```

## Cómo acceder a las variables en .NET

```csharp
DotNetEnv.Env.Load();
var key = Environment.GetEnvironmentVariable("SUPABASE_KEY");
```

## Recursos

- [DotNetEnv](https://github.com/tonerdo/dotnet-env)
- [Documentación oficial .NET: User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

---

Cualquier pregunta o incidente, consulta a tu líder técnico o usa la sección Security del repo para emergencias.
