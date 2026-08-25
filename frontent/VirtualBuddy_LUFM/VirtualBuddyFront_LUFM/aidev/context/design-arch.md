# Diseño y arquitectura frontend

## Propósito

El frontend de Virtual Buddy se organiza como un monolito modular Angular orientado al dominio. La arquitectura busca aislar reglas de negocio, permitir que cada capacidad evolucione de forma independiente y evitar que Angular, HTTP o la UI definan el modelo del negocio.

Esta arquitectura establece el diseño técnico; las funcionalidades, permisos, estados y validaciones solo existen cuando una spec los define.

## Dominios identificados

El contexto y la estructura de especificaciones identifican inicialmente:

- `project`: consulta y administración de proyectos y sus equipos.
- `document`: gestión documental y consultas asistidas sobre un proyecto.
- `user`: identidad, sesión y acceso.

`dashboard` es una composición de información para navegación y consulta; no debe convertirse automáticamente en un dominio con reglas propias. Estos límites pueden cambiar únicamente mediante una spec de módulo aprobada.

## Estructura objetivo

```text
src/app/
  core/                       # Capacidades técnicas globales
  shared/                     # UI y utilidades sin reglas de dominio
  features/
    <dominio>/
      domain/                 # Modelo y reglas puras del negocio
      application/            # Casos de uso y puertos
      infrastructure/         # HTTP, storage, DTOs y mappers
      ui/
        pages/                # Pantallas y composición del flujo
        components/           # Componentes presentacionales del dominio
        state/                # Estado de presentación cuando sea necesario
      <dominio>.routes.ts
```

Las carpetas son límites, no una cuota de archivos. Una feature simple puede comenzar con `ui/` y crecer cuando una spec introduzca reglas, casos de uso o integraciones reales.

## Dependencias

```text
UI --------------> Application --------------> Domain
                          ^                       ^
                          |                       |
                    Infrastructure --------------+
```

- `domain` es TypeScript puro y no conoce Angular, RxJS, DTOs ni infraestructura.
- `application` expresa intenciones mediante casos de uso y contratos mediante puertos.
- `infrastructure` adapta HTTP, almacenamiento y APIs externas a esos contratos.
- `ui` transforma interacción y estado de presentación en llamadas a casos de uso.
- La composición de dependencias se realiza con providers e Injection Tokens en el límite de la feature o ruta.

## Flujo de ejecución

1. Una ruta lazy carga la feature y sus providers.
2. Una page recibe la interacción del usuario y delega una intención a aplicación.
3. El caso de uso aplica o coordina reglas de dominio y consume un puerto.
4. Un adaptador de infraestructura implementa el puerto, mapea DTOs y controla detalles técnicos.
5. La UI representa un View Model explícito con estados de carga, éxito, vacío y error definidos por la spec.

## Estado

- La presentación sigue una estrategia Signals-first: el template consume Signals y las derivaciones se expresan con `computed`.
- RxJS representa fuentes y transformaciones asíncronas: HTTP, router, WebSockets, debounce, cancelación, combinación y reintentos.
- Los Observables se convierten con `toSignal` en el límite de presentación o en `ui/state/`; no se eliminan cuando modelan mejor la asincronía.
- Los componentes no realizan suscripciones manuales por defecto. Una suscripción inevitable se encapsula, se justifica y se cancela con `takeUntilDestroyed`.
- `effect` se reserva para efectos secundarios externos y no se utiliza para sincronizar estado derivable entre Signals.
- El estado local permanece en el componente mientras no necesite coordinación.
- El estado de feature puede extraerse a `ui/state/` cuando varias piezas del mismo flujo lo comparten.
- No se adopta un store global hasta que una necesidad transversal demostrable lo justifique.

```text
HTTP / Router / WebSocket -> Observable -> Application / Infrastructure
                                             |
                                          toSignal
                                             |
                                      UI basada en Signals
```

## Evolución tecnológica

- Se prefieren APIs modernas declaradas estables por la versión instalada de Angular.
- Una API experimental se evalúa por estabilidad, soporte y posibilidad de reemplazo antes de incorporarse.
- No se agrega una dependencia externa cuando las capacidades nativas resuelven el caso con claridad.
- La modernización es incremental y asociada al slice afectado; no justifica refactors fuera de una spec.

## Integración entre dominios

- Cada feature expone el contrato mínimo necesario; sus carpetas internas no son API pública.
- Las vistas que combinan dominios coordinan datos sin trasladar reglas de un dominio a otro.
- `shared/` no puede depender de `features/`; `core/` tampoco contiene reglas específicas de negocio.
- Los ciclos o importaciones directas entre internals de features indican un límite incorrecto y deben resolverse antes de implementar.

## Diseño visual

Las imágenes en `aidev/context/ui/` describen la referencia visual del prototipo: login, dashboard, administración, creación y detalle de proyectos, y gestión documental con consulta IA. Son contexto de diseño, no archivos servidos por la aplicación. Una spec debe definir qué pantalla o comportamiento se implementa y sus criterios responsive, de interacción y accesibilidad.

## Estrategia de evolución

El código actual es un punto de partida y puede no cumplir todavía toda la estructura objetivo. La adopción es incremental por spec: se crea o ajusta únicamente el slice afectado, se preserva el comportamiento fuera del alcance y se evita una reestructuración global sin una spec de arquitectura aprobada.
