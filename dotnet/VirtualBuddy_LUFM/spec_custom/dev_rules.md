# Development Rules

## Architecture

- Aplicar SOLID.
- Favorecer alta cohesión.
- Mantener bajo acoplamiento.
- Respetar los límites de dominio existentes.
- No introducir dependencias entre dominios sin necesidad explícita.

## Domain

- Evitar entidades anémicas cuando exista comportamiento de dominio.
- Las invariantes deben protegerse cerca del dominio.
- No mover reglas de negocio a controllers.

## DTOs

- Utilizar DTOs para transporte de datos entre límites de aplicación.
- No exponer entidades de dominio directamente en APIs.
- Separar DTOs de entrada y salida cuando sus responsabilidades difieran.

## Controllers

- Controllers delgados.
- Responsabilidad:
  - recibir request;
  - validar formato básico;
  - delegar al caso de uso;
  - transformar respuesta.

- No implementar lógica de negocio en controllers.

## Application

- Los casos de uso coordinan operaciones.
- No introducir detalles de infraestructura dentro del dominio.

## Errors

- Utilizar el mecanismo estándar de errores del proyecto.
- No crear nuevos formatos de error si ya existe uno.
- Priorizar el manejo y la observabilidad de excepciones en toda FEATURE, BUG, MODULE, IMPLEMENT o cambio técnico.
- Toda excepción capturada debe propagarse hasta un límite que la registre, registrarse donde sea consumida o conservar su causa al traducirse. No se permiten excepciones silenciadas.
- No agregar `try/catch` indiscriminadamente. Si una capa no puede recuperar el proceso, aportar contexto necesario o traducir el error, debe permitir que la excepción alcance el límite responsable.
- Evitar registrar la misma excepción repetidamente en capas consecutivas. Registrar localmente cuando el error se consume, cuando se pierde contexto al traducirlo o cuando se necesita contexto operativo que el límite final no puede reconstruir.
- Los procesos tolerantes a fallos deben mantener el comportamiento definido por su spec, pero siempre registrar de forma segura las excepciones que consuman.
- Clasificar la severidad del log según el impacto: fallos esperados del cliente no deben ocultar los fallos operativos; los errores que impiden iniciar la aplicación deben registrarse como críticos y propagarse.
- Incluir contexto útil y correlación disponible en los logs, sin registrar credenciales, tokens, contraseñas, códigos, cuerpos completos, contenido documental ni otros datos sensibles.
- Al traducir una excepción técnica a un error seguro, preservar la causa como excepción interna o registrarla en el límite que conoce la integración, sin exponerla al cliente.

## Testing

Cada cambio debe considerar:

- happy path;
- validaciones;
- reglas de negocio;
- errores;
- edge cases;
- regresiones sobre comportamiento existente.
- resiliencia y tolerante a fallos
- observabilidad de cada excepción capturada, incluidos procesos secundarios, compensaciones e inicio de la aplicación.
- ausencia de excepciones silenciadas, logs duplicados y datos sensibles en errores o registros.

## Existing patterns

Antes de crear una nueva abstracción:
buscar primero patrones equivalentes existentes en el repositorio.

Favorecer consistencia del proyecto sobre preferencias personales del agente.

## 🧠 Enfoque en DDD (Domain-Driven Design)

El desarrollo se enfoca en modelar el dominio de forma explícita:

- **Entidades, Value Objects y Agregados**: Estructurar el dominio de manera coherente.
- **Lenguaje Ubicuo**: Alineación entre negocio y desarrollo.
- **Encapsulación**: La lógica de negocio reside en el dominio.

## ⚙️ Reglas Buenas Prácticas

Debe seguir estrictamente principios de ingeniería de software:

- **SOLID** y **Clean Code**.
- **Bajo acoplamiento y alta cohesión**.
- **Separación de responsabilidades**.
- **Uso de DTOs**: Toda respuesta en los endpoints deben ser DTO y no exponer la entidad directamente.
- **Enfoque Domain-Centric (DDD)**:
  - La lógica de negocio y las reglas de validación **DEBEN** residir en la capa de Dominio (Entidades, Value Objects o Servicios de Dominio).
  - Las transiciones de estado de las entidades deben realizarse mediante métodos explícitos en la entidad, no mediante setters públicos (Encapsulación).
  - El Dominio debe ser agnóstico a DTOs, frameworks y detalles de infraestructura.
- **Uso de Value Objects**:
  - Se deben utilizar Value Objects para tipos de datos que posean lógica de validación o reglas de negocio (ej. nombres, emails, descripciones).
  - Esto permite centralizar las validaciones y asegurar que las entidades siempre operen con datos válidos, reduciendo la carga de validación manual en los constructores de las entidades.
