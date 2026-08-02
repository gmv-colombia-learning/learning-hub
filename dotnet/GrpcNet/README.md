# Ejemplo básico de gRPC en .NET

Este repositorio contiene dos aplicaciones de consola independientes:

* **Servidor**
* **Cliente**

La idea del proyecto es mostrar de forma simple cómo funciona la comunicación entre ambas aplicaciones usando **gRPC**.

## Cómo ejecutar

1. Abrir la solución en Visual Studio.
2. Ejecutar primero el proyecto **Servidor** en modo **Debug**.
3. Luego ejecutar el proyecto **Cliente** también en modo **Debug**.
4. Al correr ambos programas, las peticiones y respuestas se mostrarán en las consolas.

## Qué revisar en el código

El objetivo es que, después de ejecutar el ejemplo, puedas identificar en el código:

* cómo se define el servicio gRPC;
* cómo el cliente llama al servidor;
* cómo funcionan los distintos tipos de comunicación;
* cómo se envían y reciben mensajes en consola.

## Tipos de comunicación incluidos

Este ejemplo usa diferentes formas de comunicación gRPC.

## Objetivo del ejemplo

Este proyecto es un caso básico de aprendizaje. Sirve para entender el flujo de una comunicación gRPC entre dos procesos independientes.
