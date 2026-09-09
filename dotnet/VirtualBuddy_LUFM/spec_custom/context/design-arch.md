## 🏗️ Arquitectura y Estructura del Proyecto

El proyecto se construye bajo el enfoque de **Clean Architecture**, promoviendo una clara separación de responsabilidades y un diseño desacoplado.

### Capas del Proyecto:

- **VirtualBuddy.Api**: Capa de presentación. Contiene los controladores y la configuración de la API REST. Independencia de frameworks en la medida de lo posible.
- **VirtualBuddy.Application**: Capa de aplicación. Contiene los casos de uso (Use Cases), DTOs (Request/Response) y fachadas (Facades).
- **VirtualBuddy.Domain**: Núcleo del negocio. Contiene las entidades del dominio, objetos de valor (Value Objects), interfaces de repositorio y enums.
- **VirtualBuddy.Infraestructure**: Implementación de detalles técnicos. Contiene la persistencia con Entity Framework Core, PostgreSQL para `Local`, Azure SQL Database para `Development`, migraciones separadas por proveedor e identidad. **Incluye la implementación de RAG con Semantic Kernel, Ollama en `Local` y Azure OpenAI en `Development`.**
- **VirtualBuddy.Test**: Pruebas unitarias e integración para asegurar la calidad del código, mantenibilidad y testabilidad.

---

## 🤖 Capacidades de IA (RAG)

El proyecto utiliza un pipeline de **Retrieval-Augmented Generation** para proporcionar respuestas contextuales:

- **Orquestador:** Microsoft Semantic Kernel (usando `Microsoft.Extensions.AI`).
- **Proveedores de IA:** Ollama en `Local` (`llama3` para chat, `nomic-embed-text` para embeddings) y Azure OpenAI en `Development` (`gpt-4.1-mini` para chat, `text-embedding-3-small` para embeddings de 768 dimensiones).
- **Vector Store:** PostgreSQL en `Local` (similitud implementada en SQL sobre arreglos `real[]`) y Azure SQL Database en `Development` (tipo `VECTOR(n)` y `VECTOR_DISTANCE`).
- **Formatos Soportados:** PDF, Word (.docx), Excel (.xlsx), TXT.
- **Flujo:** Indexación automática al subir archivos -> Búsqueda semántica en chat -> Generación de respuesta aumentada con contexto.
