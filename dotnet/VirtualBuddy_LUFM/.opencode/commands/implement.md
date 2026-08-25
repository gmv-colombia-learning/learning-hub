---
description: Implementar exclusivamente una spec READY
---

Implementa la siguiente spec siguiendo estrictamente el workflow IMPLEMENT de SDD:

$ARGUMENTS

Antes de escribir código:

1. comprobar que la spec existe;
2. comprobar que su status es READY;
3. leer `spec_custom/agents_dev_rules.md`;
4. leer `spec_custom/dev_rules.md`;
5. leer el contexto arquitectónico relevante;
6. inspeccionar código relacionado;
7. analizar impacto;
8. identificar posibles regresiones;
9. presentar un plan.

No ampliar el scope.

Si se necesita modificar un flujo no contemplado por la spec,
detener ese cambio e informar al usuario antes de continuar.

Después de implementar:

- ejecutar tests;
- crear tests necesarios;
- verificar Acceptance Criteria;
- informar qué archivos fueron modificados;
- informar cualquier desviación;
- actualizar estado según corresponda.
