# Etapa 6 — entorno vivo

Versión Android 0.6.0, código 7. Unity 6000.6.2f1, URP, ARM64/IL2CPP, Android mínimo 26 y objetivo 36. Dispositivo de prueba del usuario: Samsung A56 con Android 16.

Escena: Assets/Scenes/SampleScene_Android_06.unity.
Menú: **El Último Faro > Etapa 6 > Abrir entorno vivo Android**.

El usuario validó la etapa 5 y la corrección del mar 0.5.1. Autorizó esta etapa y, tras revisar el modelo, eligió habilitar un pequeño acceso con un interior sencillo en el faro.

## Funcionalidades

1. **Puerta original interactiva.** LighthouseDoor anima Door alrededor de Bisagra_puerta. El botón contextual permite ABRIR PUERTA / CERRAR PUERTA cuando se mira desde cerca. La apertura es gradual, limitada a 100 grados. Puede operarse desde fuera y desde dentro sin depender del cuarzo. Sonido 3D de madera durante el giro y cierre al llegar al marco. Si hay un obstáculo en el arco de giro se detiene y muestra un mensaje; se puede reintentar al apartarse.
2. **Entrada e interior.** El modelo tenía una puerta decorativa y un marco macizo frente a la pared de la torre. En la escena 06 se utiliza una copia de la malla de Tower_Red_Lower con un hueco localizado, un marco abierto y la hoja ajustada a la altura del personaje. Se separó la ventana frontal del nuevo dintel y se dejó holgura sobre el escalón. El GLB fuente no cambia. Interior_sencillo contiene suelo, paredes, techo y un umbral con colisiones. Los escalones conservan su geometría visible y usan una superficie de colisión continua para caminar. EntranceTraversal reduce la elevación automática del CharacterController solo bajo el dintel y restaura el valor exterior al salir; no cambia altura ni velocidad. El farol portátil sirve para iluminarlo. No se añade una misión ni una subida a la parte alta del faro.
3. **Refugio de lluvia.** RainShelter elimina las gotas que entran bajo el techo, detecta al jugador dentro de Volumen_refugio y solicita a WeatherAudio una atenuación gradual de lluvia, viento y mar. Al salir vuelve el sonido exterior. La tormenta mantiene su estado global; entrar no reinicia ni completa la misión.
4. **Vuelo de las gaviotas existentes.** SeagullFlight utiliza los dos objetos existentes Gaviota_en_vuelo y Gaviota_en_vuelo (1). Uno recorre el contorno de la isla principal y otro el entorno de la isla secundaria. Tienen orientación tangente al recorrido, variación moderada de altura e inclinación suave. Se aprovechan los pivotes de alas ya incluidos para aleteos cortos intercalados con planeo. No se reemplazan modelos ni se añade un rig.
5. **Sonido de gaviotas.** AudioSource 3D con distancia máxima de 19 unidades. Se alternan dos llamadas con pequeñas variaciones de tono. Los intervalos son de 15–26 segundos y aumentan durante la tormenta, cuando también baja el volumen. No se reproducen llamadas en cada cuadro ni desde toda la isla al mismo volumen. El vuelo y la voz se detienen al pausar la aplicación.
6. **Vegetación con viento.** VegetationSway actúa sobre once árboles y nueve grupos de flores. Deforma progresivamente los vértices superiores; el 18 % inferior queda fijo. Las raíces se conservan sin animación y los troncos mantienen sus colisiones. Cada planta usa una fase distinta. La fuerza sube con la tormenta del cuarzo y baja tras encender el faro. Los árboles completos no giran como bloques rígidos.
7. **Preservación visual y coste.** WindMeshData almacena los atributos originales para Android, donde algunas mallas importadas no permiten lectura desde CPU. Se comparten catorce conjuntos de datos entre las plantas y se crean copias de malla durante la partida. Se conservan materiales, submallas y triangulación. Las posiciones se actualizan como máximo a 30 Hz con arrays reutilizados. La revisión de fluidez física del A56 sigue siendo necesaria.
8. **Continuidad de la misión.** Se mantienen escala, velocidad, muelles, navegación libre, mar low poly animado, farol, cuarzo, inserción, arranque, palanca, haz del faro y la secuencia lluvia → tormenta → calma. La interacción de puerta y máquina se elige según la dirección de la mirada; los objetos recogibles conservan su prioridad.

## Parámetros

- LighthouseDoor: alcance, ángulo máximo y duración aproximada de apertura.
- SeagullFlight: FlightSpeed, CircleRadius, HeightVariation y fase; referencias de pivotes, clips y clima.
- VegetationSway: WindStrength, WindSpeed, Variation, fase y desplazamiento máximo. El máximo base es 0,09 unidades en árboles y 0,015 en flores, modulado por el clima.
- RainShelter: volumen interior, jugador y WeatherAudio. No modifica el progreso climático.

## Prueba recomendada en el A56

1. Instala ElUltimoFaro_06_Entorno_Vivo.apk sobre la anterior.
2. Recoge y enciende el farol. Acércate a la puerta y mírala; usa ABRIR PUERTA. Deja libre el espacio por donde gira.
3. Entra caminando. Comprueba el suelo, las paredes, la ausencia de lluvia bajo el techo y el sonido exterior más amortiguado.
4. Cierra y vuelve a abrir desde dentro. Sal caminando. Si bloqueas el giro, apártate y vuelve a pulsar.
5. Observa las gaviotas y escucha sus llamadas ocasionales. Comprueba que la vegetación se mueva sin desplazar las bases.
6. Haz una vuelta completa sin teletransportes: bote, isla secundaria, cuarzo, regreso y encendido del faro. Compara el viento antes, durante y después de la tormenta.
7. Envía la aplicación a segundo plano y vuelve. Revisa controles, puerta, clima y sonidos.

La retroalimentación principal es: facilidad de entrada/salida, elección correcta del botón entre puerta y máquina, visibilidad del interior con el farol, naturalidad del vuelo/viento, mezcla sonora y fluidez del recorrido.

## Verificación y problemas resueltos

Los verificadores FaroEnvironmentVerification y FaroStageSixVerification ejercitan el acceso, obstáculos, refugio, vuelo, vegetación y la misión completa. Los resultados definitivos se entregan en Verificacion_Etapa_6.txt. Las capturas y comprobaciones son del Editor; no se presenta una medición de FPS del teléfono.

El acceso requirió adaptar un modelo originalmente cerrado. Se generan mallas propias de esta escena, conservando el archivo GLB original. Se corrigió la interferencia de la ventana con el dintel y del borde de la hoja con el escalón. Las pruebas en Editor oculto simulan el foco de aplicación para que no se confunda una pausa legítima con una animación bloqueada.

Los sonidos de madera y gaviotas se sintetizaron localmente; son efectos estilizados, no grabaciones reales ni recursos descargados. Se incluyen los WAV fuente y sus importaciones comprimidas. No se importaron paquetes.

## Conservación y evidencia

Se conservan SampleScene y todas las escenas Android 01–05, modelos originales, materiales compartidos anteriores y parámetros de la misión. Los cambios existentes se limitan a conexiones opcionales en MobileInteraction, una consulta de ángulo en MachineInteraction y la atenuación interior en WeatherAudio; las escenas anteriores sin esas referencias mantienen su comportamiento.

La fuente, los verificadores, los WAV, el respaldo previo y los informes documentan el trabajo realizado con Codex, conforme a la autorización del usuario. No se atribuye el proceso a otras herramientas.

Para compilar: **El Último Faro > Etapa 6 > Compilar APK Android**.
