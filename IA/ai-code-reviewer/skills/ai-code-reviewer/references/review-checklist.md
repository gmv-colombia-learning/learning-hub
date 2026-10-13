# Review Checklist

## 1. Requisitos y comportamiento

- Mapear cada criterio de aceptación relevante a evidencia en el código o tests.
- Detectar criterios sin implementación o implementación sin requisito claro.
- Revisar happy path, errores, límites, nulos, vacíos, duplicados, reintentos y estados parciales según aplique.
- Confirmar que el cambio no altere contratos públicos sin intención documentada.
- Identificar solapamientos o conflictos con comportamiento existente.

## 2. Arquitectura y diseño

- Respetar límites de módulos/capas y dirección de dependencias.
- Evitar acoplamiento innecesario y responsabilidades mezcladas.
- Aplicar SOLID donde aporte claridad; no forzar patrones sin necesidad.
- Revisar consistencia con ADR, README y convenciones del repositorio.
- Evitar duplicación de lógica de negocio.
- Verificar que el cambio esté ubicado en el componente correcto.

## 3. Calidad y mantenibilidad

- Nombres claros y consistentes.
- Complejidad accidental mínima: condiciones anidadas, ramas, ciclos y flujo difíciles de seguir.
- Funciones, clases y módulos con tamaño y responsabilidades entendibles.
- Duplicación real de reglas o lógica; distinguirla de repetición intencional para mantener límites claros.
- Acoplamiento, dependencias circulares y abstracciones que aumentan el costo de cambio.
- Código muerto, flags temporales y TODO sin justificación.
- Valores mágicos o configuraciones repetidas que deberían tener una fuente clara.
- Nombres engañosos, APIs confusas y comentarios que contradicen el comportamiento.
- Testabilidad: dependencias ocultas, efectos secundarios difíciles de aislar y límites de módulo borrosos.
- Manejo de errores explícito y observable.
- Logs útiles sin exponer información sensible.
- Comentarios explican intención, no repiten el código.

Para reportar un code smell, señalar el símbolo y líneas afectadas, describir el costo concreto (por ejemplo, cambios duplicados, alta probabilidad de regresión o dificultad para probar) y proponer una mejora acotada. No reportar gustos de formato como defectos si ya existe un formatter/linter que los cubre. No elevar un smell a `P1` solo por ser una mala práctica; usar `P2` o `P3` según el impacto demostrable.

## 4. Seguridad y privacidad

- Validación de entradas y autorización.
- Inyección SQL/command/template/LDAP según el stack y el flujo de datos.
- XSS, CSRF, CORS, SSRF y path traversal cuando aplique.
- Deserialización insegura, validación de firmas y manejo de archivos subidos cuando aplique.
- Exposición de secretos o credenciales.
- Datos personales o de cliente en logs, errores o telemetría.
- Control de acceso por objeto/recurso y escalamiento de privilegios.
- Uso de criptografía insegura o gestión incorrecta de claves/tokens.
- Dependencias nuevas con riesgo o licencia no permitida.
- Controles de acceso consistentes con la operación.

Distinguir entre vulnerabilidad confirmada y patrón sospechoso que requiere validación. Cuando haya un analizador local configurado, registrar nombre, alcance y resultado; si no, describirlo como revisión manual asistida por IA. La ausencia de hallazgos de IA no demuestra ausencia de vulnerabilidades.

## 5. Datos y consistencia

- Cambios de esquema compatibles y reversibles cuando aplique.
- Transacciones correctas.
- Idempotencia en operaciones repetibles.
- Concurrencia y race conditions.
- Conversión de tipos, timezone, precisión y formatos.
- Compatibilidad hacia atrás cuando sea necesaria.

## 6. Tests

No aceptar “hay tests” como sinónimo de cobertura útil.

Revisar:
- Casos que prueban criterios de aceptación.
- Casos negativos y edge cases.
- Tests que fallarían antes del cambio.
- Aserciones significativas.
- Ausencia de tests frágiles o excesivamente mockeados.
- Cobertura de rutas críticas, no solo porcentaje global.
- E2E/integración cuando el riesgo cruce componentes.

## 7. Dependencias y documentación

- Comparar README/documentación con manifests reales.
- Justificar dependencias nuevas.
- Evitar dependencias duplicadas o innecesarias.
- Verificar versiones y compatibilidad con el stack.
- Actualizar documentación cuando cambien configuración, API, operación o uso.

## 8. Selección de reglas por tecnología

Antes de aplicar reglas específicas:
- Detectar tecnologías, frameworks, runtime y versiones por subproyecto.
- Aplicar solo reglas compatibles con la evidencia detectada.
- Usar archivos de configuración y manifests para identificar convenciones/tooling.
- Si la tecnología no tiene una sección específica en este checklist, aplicar las secciones generales y complementar con prácticas propias del stack respaldadas por evidencia del repositorio.
- No trasladar patrones de Angular/.NET a otros stacks por analogía.

## 9. Referencias visuales opcionales para frontend

Cuando el cambio afecte una pantalla o comportamiento visual, preguntar si la persona tiene diseños de Figma que quiera compartir. La pregunta es opcional y no debe bloquear la revisión. Pedir que adjunte las pantallas relevantes como imágenes (PNG/JPG/WebP); no exigir acceso directo a Figma ni una integración.

Si recibe imágenes:
- Relacionarlas solo con las pantallas/estados tocados por el diff.
- Compararlas con una captura de la implementación solo si se puede ejecutar y capturar la UI en condiciones equivalentes.
- Sin captura renderizada, usarlas como especificación visual de referencia y no afirmar que la UI final fue verificada.
- Marcar los aspectos no verificables como `NOT VERIFIED`.

## 10. Angular

Cuando se detecte Angular:
- Revisar separación smart/presentational cuando la arquitectura la use.
- Evitar suscripciones manuales sin necesidad; revisar limpieza y ciclo de vida.
- Preferir flujos reactivos claros y evitar nested subscriptions.
- Revisar change detection y renders costosos en rutas calientes.
- Verificar tipado estricto; evitar `any` innecesario.
- Revisar guards/interceptors/resolvers y manejo de errores.
- Evitar lógica de negocio dispersa en componentes si pertenece a servicios/fachadas/dominio.
- Revisar accesibilidad básica y estados loading/error/empty.
- Revisar tests de componentes/servicios según riesgo.
- Confirmar compatibilidad con la versión de Angular indicada por `package.json`.

## 11. .NET

Cuando se detecte .NET:
- Revisar nullable reference types cuando estén habilitados.
- Propagar `CancellationToken` en operaciones async relevantes.
- Evitar `.Result`/`.Wait()` en código async.
- Revisar lifetime de dependencias en DI.
- Validar manejo de excepciones y códigos HTTP en APIs.
- Separar DTO, dominio y persistencia cuando la arquitectura lo requiera.
- Revisar consultas EF/LINQ por N+1, tracking innecesario y materialización temprana.
- Revisar transacciones e idempotencia.
- Evitar loggear secretos/PII.
- Confirmar target framework y paquetes desde `.csproj`, `Directory.Packages.props` o `global.json`.
- Revisar tests unitarios/integración según límites de la solución.

## 12. Señales específicas de código generado por IA

Buscar activamente:
- APIs o métodos inexistentes/alucinados.
- Imports/usings innecesarios o incorrectos.
- Repetición de patrones sin entender convenciones locales.
- Manejo superficial de errores.
- Código excesivamente genérico para una regla de negocio específica.
- Condiciones que solo satisfacen el happy path mostrado en el prompt.
- Tests que replican la implementación en vez de validar comportamiento.
- Comentarios grandilocuentes o documentación que no coincide con el código.
- Nuevas abstracciones innecesarias.
- Dependencias inventadas o versiones incompatibles.
- Falsos positivos de “todo correcto” derivados únicamente de compilación/tests.
