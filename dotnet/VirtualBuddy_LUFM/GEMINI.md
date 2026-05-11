# GEMINI.md - Project Guidelines & Context

## 📌 Proyecto de Mentoría Virtual Inteligente

### 🧩 Descripción

Este proyecto consiste en un prototipo de aplicativo web para brindar mentoría virtual e inteligente sobre los proyectos que se están desarrollando en el área de desarrollo de software en GMV.

La solución busca centralizar conocimiento y ofrecer acompañamiento contextual, mejorando la productividad y reduciendo la curva de aprendizaje.

**Objetivos principales:**

- Mantener informado a todo el equipo.
- Facilitar la comprensión de los proyectos activos.
- **Apoyar especialmente a:**
  - Nuevos integrantes del equipo.
  - Empleados que cambian de proyecto.
  - Personas que requieren contexto rápido y preciso.

---

## 🎯 Objetivo del prototipo

Validar una solución que permita:

- Consultar información relevante de proyectos.
- Obtener orientación inteligente sobre el desarrollo.
- Facilitar la transferencia de conocimiento dentro del equipo.
- Reducir tiempos de onboarding y adaptación.

---

## 🏗️ Arquitectura y Estructura del Proyecto

El proyecto se construye bajo el enfoque de **Clean Architecture**, promoviendo una clara separación de responsabilidades y un diseño desacoplado.

### Capas del Proyecto:

- **VirtualBuddy.Api**: Capa de presentación. Contiene los controladores y la configuración de la API REST. Independencia de frameworks en la medida de lo posible.
- **VirtualBuddy.Application**: Capa de aplicación. Contiene los casos de uso (Use Cases), DTOs (Request/Response) y fachadas (Facades).
- **VirtualBuddy.Domain**: Núcleo del negocio. Contiene las entidades del dominio, objetos de valor (Value Objects), interfaces de repositorio y enums.
- **VirtualBuddy.Infraestructure**: Implementación de detalles técnicos. Contiene la persistencia (Entity Framework Core, PostgreSQL), migraciones e identidad.
- **VirtualBuddy.Test**: Pruebas unitarias e integración para asegurar la calidad del código, mantenibilidad y testabilidad.

---

## 🧠 Enfoque en DDD (Domain-Driven Design)

El desarrollo se enfoca en modelar el dominio de forma explícita:

- **Entidades, Value Objects y Agregados**: Estructurar el dominio de manera coherente.
- **Lenguaje Ubicuo**: Alineación entre negocio y desarrollo.
- **Encapsulación**: La lógica de negocio reside en el dominio.

## 🧠 Flujo de Trabajo: Planificación Interactiva

Para mantener el proyecto ágil y profesional sin sobreingeniería, seguiremos este flujo:

1. **Propuesta Técnica**: Para cualquier cambio no trivial, el agente presentará una propuesta breve en el chat detallando qué archivos se verán afectados y cuál es la lógica propuesta.
2. **Aprobación**: El usuario debe validar la propuesta antes de que el agente proceda.
3. **Ejecución y Validación**: Una vez aprobado, se implementa el código y se verifica mediante pruebas o revisión de logs/comportamiento.

---

## ⚙️ Reglas de Operación y Buenas Prácticas

Se siguen estrictamente principios de ingeniería de software:

- **SOLID** y **Clean Code**.
- **Bajo acoplamiento y alta cohesión**.
- **Separación de responsabilidades**.
- **Confirmación Obligatoria**: El agente **DEBE** preguntar y obtener confirmación del usuario antes de realizar cualquier cambio en el código o estructura del proyecto.
- **Uso de DTOs**: Toda respuesta en los endpoints deben ser DTO y no exponer la entidad directamente.
- **Enfoque Domain-Centric (DDD)**:
  - La lógica de negocio y las reglas de validación **DEBEN** residir en la capa de Dominio (Entidades, Value Objects o Servicios de Dominio).
  - Las transiciones de estado de las entidades deben realizarse mediante métodos explícitos en la entidad, no mediante setters públicos (Encapsulación).
  - El Dominio debe ser agnóstico a DTOs, frameworks y detalles de infraestructura.
- **Uso de Value Objects**:
  - Se deben utilizar Value Objects para tipos de datos que posean lógica de validación o reglas de negocio (ej. nombres, emails, descripciones).
  - Esto permite centralizar las validaciones y asegurar que las entidades siempre operen con datos válidos, reduciendo la carga de validación manual en los constructores de las entidades.
- **Modo Plan**: Para tareas complejas, el agente usará la herramienta `enter_plan_mode` para diseñar la solución antes de ejecutarla.

---

## 🚀 Estado del Proyecto

Iniciativa en etapa de planeación y desarrollo inicial, concebida como una base evolutiva con potencial de escalar e integrar servicios de IA y automatización del conocimiento.

## 🎨 Contexto Visual y UI (spec/ui)

Para asegurar que el desarrollo del backend soporte todas las funcionalidades de la interfaz, se han analizado los prototipos en `spec/ui`. Los puntos clave son:

### 1. Vistas Principales
- **Dashboard (`home.png`)**: Visualización de proyectos en cards con estados visuales (`Activo`, `Mantenimiento`, `Planificación`).
- **Detalle del Proyecto (`DetailProject.png`)**: Información extendida que incluye **Tecnologías** (tags), **Tiempo de Desarrollo** y un **Equipo de Trabajo** con roles específicos (ej. Project Lead, QA).
- **Administración (`panelAdmin.png`)**: Gestión centralizada de proyectos, acceso a edición y documentos.
- **Gestión de Documentos e IA (`GestionDocumentos&Consultaia.png`)**: Interfaz de carga de archivos y chat lateral para consultas contextuales sobre el proyecto.
- **Formularios (`formCrearProject.png`)**: Captura dinámica de datos, incluyendo la adición de múltiples tecnologías y miembros del equipo.

### 🔍 Observaciones Técnicas para el Desarrollo
- **Modelo de Datos Extendido**: El dominio debe evolucionar para soportar colecciones de **Tecnologías** y **Miembros del Equipo** (Entidades o Value Objects con Nombre y Rol).
- **Gestión de Estados**: Los estados deben ser consistentes entre el backend (`ProjectStatus`) y su representación visual.
- **Capacidad de IA**: El sistema debe estar preparado para vincular documentos específicos a un proyecto para alimentar el contexto de la IA.

## 🚀 Flujo para Desarrollo de Features y Componentes

Para la implementación de nuevas funcionalidades, utilizaremos como referencia los recursos en `spec\ui` (prototipos visuales) y `instructions` (especificaciones técnicas en formato `.md`). El proceso será el siguiente:

1. **Indicación del Recurso**: El usuario indicará el nombre del archivo `.md` en la carpeta `instructions` y, si aplica, la imagen en `spec\ui` a validar.
2. **Análisis y Diagnóstico**: El agente leerá y analizará exhaustivamente el archivo de instrucciones para comprender los objetivos, reglas de negocio y requisitos técnicos.
3. **Propuesta Técnica**: El agente presentará una propuesta detallada del camino a tomar, detallando los cambios en cada capa (Domain, Application, Api, etc.) para aprobación del usuario.
4. **Ejecución**: Una vez aprobado el plan, se procederá con la implementación siguiendo los estándares de Clean Architecture y DDD.
