# Actualización: etapa 6, entorno vivo

La versión 0.6.0 se documenta en [Etapa_6_Entorno_Vivo.md](Etapa_6_Entorno_Vivo.md). Añade puerta operable, acceso e interior sencillo autorizado por el usuario, refugio de lluvia, vuelo y voces de las dos gaviotas existentes y viento en once árboles y nueve grupos de flores. El clima y la misión validados en la etapa 5 se conservan.

---

# Corrección 0.5.1: oleaje visible

El usuario validó el clima de la etapa 5 en Android y pidió un movimiento visible de los triángulos del mar. Se anima una copia de la geometría original y se conserva la flotación sobre la misma superficie. Ver [Correccion_Mar_0_5_1.md](Correccion_Mar_0_5_1.md). La etapa 6 sigue pendiente.

---

﻿# Actualización: etapa 5, noche y tormenta

La versión actual se documenta en [Etapa_5_Noche_Tormenta.md](Etapa_5_Noche_Tormenta.md). Añade noche permanente, lluvia fuerte, mar con sus caras low poly originales, sonido ambiental, tormenta al recoger el cuarzo y calma gradual al encender el faro.

El usuario confirmó que la secuencia de la etapa 4 funcionaba a la perfección en su A56. Puerta, gaviotas y vegetación quedan para la etapa 6. Se conserva a continuación el historial.

---

# Actualización: etapa 4, máquina y faro

La versión actual se describe en [Etapa_4_Maquina_Faro.md](Etapa_4_Maquina_Faro.md). Incluye caminar un 35 % más rápido, flotación sobre varios puntos del casco, continuidad física del muelle, inserción del cuarzo, arranque gradual de la maquinaria, palanca condicionada y haz del faro con rotación y parpadeo suave.

La etapa 3 fue probada por el usuario en su A56: confirmó la vuelta completa sin teletransportes. Esta actualización responde a su petición de mayor velocidad y al cruce del bote con el mar.

Se conserva a continuación el historial.

---

# Actualización: etapa 3, navegación libre

La versión actual se describe en [Etapa_3_Navegacion.md](Etapa_3_Navegacion.md). Añade acceso suavizado al muelle, embarque, conducción libre, desembarque en ambas islas, límites discretos, luces de orientación y viaje con cuarzo/farol. La maquinaria queda para la etapa siguiente.

Se conserva abajo el historial de las entregas anteriores.

---

# Actualización: etapa 2, cuarzo y farol

La versión actual se documenta en [Etapa_2_Cuarzo_Farol.md](Etapa_2_Cuarzo_Farol.md).
El usuario confirmó en su Samsung A56 el funcionamiento del primer APK, los dos dedos y la apariencia correcta del mapa. La altura excesiva se corrige en la segunda escena: cámara 0,5167 y cápsula 0,5667 unidades. Se agregan recogida de cuarzo/farol, luz con interruptor, sonidos, inventario y accesos temporales de prueba.

El registro siguiente corresponde al primer prototipo y se conserva como evidencia del proceso; sus cifras de altura, velocidad y pendientes no describen la nueva escena.

---

# El Último Faro — avance incremental

## Estado de esta entrega: etapa 1, recorrido Android

Fecha: 5 de octubre de 2026. Unity 6000.6.2f1, URP 17.6.0 e Input System 1.20.0.
Dispositivo objetivo: Samsung A56, Android 16. Esta versión es para pantalla táctil, no para Quest ni Cardboard.
Se conserva SampleScene original; el recorrido nuevo está en SampleScene_Android_01. Se conservan los modelos, sus nombres, la maquinaria colocada por el usuario y los materiales existentes.

## Funcionalidades implementadas

1. Movimiento en primera persona. GameObject Player_Android; MobilePlayerController. Se activa con el control inferior izquierdo; en el Editor también admite WASD. Usa CharacterController, gravedad, límite de pendiente y velocidad de 2 m/s. No incluye sonido de pasos en esta etapa.
2. Mirada táctil. Main Camera, TouchLookArea y MobilePlayerController. Arrastrar en la zona derecha orienta la cámara; altura de ojos 1,55 m y límite vertical de 65 grados. En el Editor se admite arrastrar con botón derecho. Movimiento y mirada aceptan dedos independientes.
3. Colisiones del recorrido. MeshCollider en las cuatro superficies de isla, geometría sólida del faro y maquinaria, troncos y parte de la arquitectura original. No se añadió collider al agua, haz de luz ni vidrio del faro. Bote y portátiles esperan sus etapas específicas.
4. Protección de borde y recuperación. MobilePlayerController verifica suelo por delante antes de avanzar. Si el jugador pierde apoyo prolongadamente o cae, vuelve al punto inicial. El botón «Volver al inicio» permite recuperación manual y reinicia los controles.
5. Interfaz adaptable. Android_Controls, MobileSafeArea, TouchMovePad y TouchLookArea. La interfaz respeta el área segura y orientación horizontal; los controles se limpian al perder foco. Los textos indican que es una prueba de recorrido.

Estas cinco entradas describen la base técnica. No se presentan como el pack final de diez interacciones del certamen.

## Objetos y próximas etapas

- Mar: mar_low_poly.
- Isla principal: polySurface32, polySurface32.001, polySurface32.002.
- Isla secundaria: polySurface32.003.
- Bote: pCube22 y pCube23. Pendiente embarque, conducción libre, límites y desembarque.
- Cuarzo: cuarzo. Emisión original conservada; pendiente recogida, transporte y recuperación.
- Faro: El_Ultimo_Faro_LowPoly. Se conservan LampGlow, LightBeam, Lantern_Glass, Lantern_Roof, Door, Tower_Red_Lower y Tower_White_Upper.
- Maquinaria: Maquinaria_Antigua_Cuarzo. Se conservan QuartzMachineController, QuartzSocketTrigger y MachineLever sin alterar su lógica en este bloque.
- Gaviotas: Gaviota_en_vuelo y Gaviota_en_vuelo (1). Pendiente vuelo.
- Farol: Farol_de_mano. Pendiente agarre, luz e interruptor.
- Vegetación y mar: pendientes movimiento ambiental sutil.
- Socket, generador, palanca, parpadeo, giro de haz y puerta: pendientes conexión e interacción.
- Sonidos y feedback de misión: pendientes.

Secuencia final prevista: explorar → navegar → recoger cuarzo → regresar → insertarlo → energizar maquinaria → accionar palanca → encender faro. No está implementada en esta versión.

## Problemas técnicos encontrados y soluciones

- No existía un controlador móvil ni colliders de recorrido: se agregó una base separada de scripts pequeños.
- El script del faro no estaba conectado y la maquinaria no tenía integración de misión: se conserva para el siguiente bloque, sin afirmar que ya funcione.
- El análisis inicial detectó un shader Built-in en Arbol_Wind, pero la inspección de renderers en Unity confirmó que la escena actual utiliza materiales URP/glTF. No se cambiaron materiales.
- El bote mide aproximadamente 0,56 m de ancho y 1,18 m de largo; la puerta del faro mide aproximadamente 0,52 m de alto. La maquinaria también es pequeña. La escala de interacción deberá ajustarse de manera localizada en sus etapas, no mediante un reescalado global del escenario.
- La primera copia de trabajo tenía una ruta demasiado larga para algunas herramientas de Unity en Windows. Se usó una copia en Documents/Codex/work/FaroAndroid.
- La ejecución restringida no conectaba con Unity Licensing Client. La validación se ejecutó con acceso al servicio de licencia local, sin cambiar la licencia.
- La prueba automatizada inicial de teclado sintético no produjo movimiento en el Editor sin foco. La prueba posterior utilizó los eventos del control táctil, que sí produjeron desplazamiento. No se considera una prueba física en teléfono.
- Se observó una excepción del índice de búsqueda del Editor en la copia de trabajo. Es una incidencia de UnityEditor.Search, no un error de compilación del código del juego; queda registrada.
- Persisten referencias heredadas sin resolver en perfiles/recursos de depuración y demos. No se aplicó una limpieza masiva de assets ajenos al recorrido.

## Uso de IA y revisión técnica

Se utilizó Codex para analizar archivos, preparar la base Android y ejecutar verificaciones de Unity. El usuario aclaró que el profesor permite cualquier IA. No se afirma haber utilizado Antigravity ni OpenCode.
El equipo debe revisar los scripts, explicar la lógica y registrar su propia prueba del APK. No se inventa evidencia de revisión humana ni de ejecución en un dispositivo.

## Compilación

Android ARM64, IL2CPP, mínimo API 26, objetivo API 36, OpenGL ES 3, orientación horizontal. Identificador: com.elultimofaro.experiencia. Versión 0.1.0.
Se utiliza el perfil URP Mobile existente para Android. No se agregaron paquetes XR.
FaroAndroidPreparation.BuildAndroid permite repetir la compilación desde línea de comandos; FARO_APK_PATH define la salida.

## Prueba de aceptación en el A56

1. Instalar y abrir el APK en horizontal.
2. Mover el control izquierdo y arrastrar simultáneamente a la derecha.
3. Recorrer la isla, acercarse a troncos y al faro y comprobar que no se atraviesan.
4. Acercarse despacio al borde: el personaje debe detenerse sin caer al mar.
5. Pulsar Volver al inicio y comprobar la recuperación.
6. Poner la aplicación en segundo plano y regresar: no debe quedar movimiento retenido.
7. Informar si aparecen objetos rosados, pantalla negra, controles ocultos, calentamiento o lentitud.

El rendimiento, la instalación, el tacto real y las particularidades del Samsung A56 requieren esta prueba física.

## Verificación automática

La escena de recorrido superó en Play Mode las comprobaciones de suelo inicial, altura de cámara, colliders de las cuatro islas, conservación de cuarzo y maquinaria, independencia de dedos, liberación del joystick, movimiento táctil, retorno manual y recuperación fuera del terreno. La prueba usa eventos simulados en Unity; no sustituye una prueba en el A56.

La primera compilación Android agotó espacio al copiar símbolos. El reintento incremental superó ese paso. Gradle encontró después un problema de confianza HTTPS del JDK; la conexión se validó usando Windows-ROOT únicamente en el proceso de compilación, sin cambiar certificados del sistema ni desactivar HTTPS.

## Resultado de compilación de esta entrega

APK generado: ElUltimoFaro_01_Recorrido.apk, 30.804.080 bytes (~30,8 MB decimales).
El empaquetado final del proyecto Gradle generado por Unity terminó con BUILD SUCCESSFUL (89 tareas).
La firma APK v2 fue verificada con apksigner. aapt confirmó ARM64, mínimo API 26, objetivo API 36 y versión 0.1.0.
El informe inicial de Unity terminó en fallo por el certificado de Gradle; el empaquetado posterior se completó directamente con Gradle usando la confianza de Windows para el cliente y el proceso aislado. No se oculta ese antecedente ni se presenta como una prueba en celular.

## Abrir en el proyecto original

Menú El Último Faro > Etapa 1 > Abrir recorrido Android, o abrir Assets/Scenes/SampleScene_Android_01.unity.
SampleScene original permanece intacta. Se añadió la capa 8 FaroGround y la carpeta Assets/ElUltimoFaro; no se modificaron materiales, modelos ni scripts de la maquinaria.
El ZIP de código incluye el proyecto con los ajustes Android de esta compilación. En el proyecto original abierto se conservó su configuración de compilación previa para no alterar la sesión del usuario; FaroAndroidPreparation.BuildAndroid configura Android y selecciona la escena de recorrido al compilar por CLI.

## Observación visual pendiente

La captura con el perfil móvil muestra zonas del faro muy oscuras y una apariencia distinta de la vista inicial del Editor. Se verificó que los colores base rojo, verde y suelo siguen presentes en los materiales importados. No se alteraron sus colores para compensar una captura sin dispositivo. Iluminación y aspecto final se deben comprobar en el A56 antes de dar por cerrada la presentación visual.
