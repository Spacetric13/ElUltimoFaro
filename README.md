# El Último Faro

Experiencia de exploración en una isla nocturna, creada en Unity para Meta Quest. Recupera el cuarzo, navega con remos y devuelve la luz al faro. Incluye un hall de bienvenida con presentación audiovisual.

**Equipo:** Facundo Gomez, Nicolás López y Cristóbal Rodriguez.

## Abrir el proyecto

1. Instala Unity **6000.6.2f1** con Android Build Support, SDK, NDK y OpenJDK desde Unity Hub.
2. Agrega esta carpeta en Unity Hub. Contiene `Assets`, `Packages` y `ProjectSettings`.
3. Abre el proyecto y permite que Package Manager descargue las dependencias declaradas. No se incluye `Library`, porque Unity la reconstruye.
4. Para VR, abre `Assets/Scenes/Hall_Quest.unity`; carga `Isla_Quest` al comenzar. Para PC, abre `Hall_PC.unity`.

## Compilar

- Selecciona Android en los perfiles de compilación para Quest.
- Ejecuta **El Último Faro > Hall > Compilar Quest final**.
- El resultado se guarda en `Builds/Quest_Final/ElUltimoFaro_Quest_Final.apk`.
- Para Windows, selecciona Windows y utiliza **El Último Faro > Hall > Compilar prueba PC**. El resultado se guarda en `Builds/Hall_Prueba04/Juego_PC`.
- Los scripts de editor de esta copia utilizan rutas relativas para funcionar en otro equipo. No cambian la lógica del juego.

La compilación final utilizó ARM64, IL2CPP, OpenXR, paquete `com.elultimofaro.quest`, versión 1.0.0 (100). Si el entorno de red impide descargar dependencias, consulta `Documentation/COMPILACION.md`; no desactives la comprobación TLS.

## Controles

Quest: palanca izquierda para caminar, derecha para girar por pasos, A para activar, B para embarcar/desembarcar y X para el farol. Los agarres laterales sujetan objetos y remos. Para retroceder se mantienen también los gatillos mientras se empujan los remos.

PC: WASD, botón derecho y ratón para mirar, E para interactuar, B para el bote, Z/C para remar y Shift + Z/C para retroceder. 1/2 dejan cuarzo/farol; H oculta la ayuda; F11 alterna pantalla completa.

## Documentación

- `Documentation/Informe_Entrega_Examen.pdf`: funcionalidades, lógica, eficiencia y guion de demostración.
- `Documentation/inventario_escenas.json`: conteos estáticos de las escenas, no métricas de FPS.
- `Documentation/HISTORIAL_DESARROLLO.md`: etapas y correcciones documentadas.
- `Documentation/build-result.txt` y `preflight.txt`: validación del APK final.
- `Assets/ElUltimoFaro/Editor/FaroStaticAudit.cs`: método `FaroStaticAudit.Run` para repetir el inventario.

## Pruebas y uso de IA

El equipo probó versiones anteriores en Quest 3S durante las clases. La versión final recibió verificaciones de compilación y pruebas complementarias en PC/editor; falta repetir su ejecución física y medir rendimiento en el visor. El teaser final está grabado en PC.

Se utilizó ChatGPT/Codex para ayudar con programación, revisión y compilación. El equipo dirigió el diseño, trabajó en la escena y los modelos, probó el comportamiento y solicitó correcciones.

## Publicación

Página itch.io: **[completar enlace]**

Teaser: **[completar enlace]**

Los APK y ejecutables se distribuyen por itch.io o GitHub Releases, no dentro del historial de código. Los paquetes de Unity y Meta conservan sus condiciones de uso; este repositorio no les asigna una licencia nueva.
