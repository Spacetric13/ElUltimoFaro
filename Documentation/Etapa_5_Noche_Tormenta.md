# Etapa 5 — noche permanente, lluvia y tormenta

Escena: `Assets/Scenes/SampleScene_Android_05.unity`.
Menú: **El Último Faro > Etapa 5 > Abrir noche y tormenta Android**.
Dispositivo objetivo: Samsung A56 / Android 16, pantalla horizontal.

El usuario validó la secuencia de la etapa 4 en el teléfono. Esta etapa conserva esa misión y añade el clima acordado. Se trabajó por bloques: noche y visibilidad; lluvia, audio y agua; transición a tormenta y recompensa al encender el faro.

## Funcionalidades y conexiones

1. **Noche permanente.** La escena usa un cielo oscuro nuevo (`NightSky.shader`), iluminación ambiental fría, niebla ligera y la luz direccional existente como iluminación nocturna. No hay ciclo de día. No se reemplazan los modelos ni se cambia URP.
2. **Orientación nocturna.** Permanecen las luces cálida del muelle y fría de la isla secundaria. Se ajustan sus intensidades únicamente en esta escena. El farol original sigue iluminando los objetos cercanos; se conserva la recogida y el interruptor.
3. **Lluvia fuerte desde el inicio.** `RainField`, en `Clima_Etapa5/Lluvia_local`, sigue al jugador tanto a pie como en el bote. Las partículas se simulan en espacio mundial, por lo que no giran con la cámara. Se limpian al usar los teletransportes de prueba. El área local y el límite de 1.600 partículas evitan poblar todo el mapa con lluvia. No hay colisión física individual de gotas.
4. **Mar low poly animado.** `LowPolyWater` y `LowPolySea.shader` se aplican a `mar_low_poly`. Se conservan exactamente sus 7.372 vértices y 2.601 triángulos. No se subdivide la malla, no se suavizan las caras ni se incorporan normales de agua realista. En la revisión 0.5.1, los vértices se animan en una malla privada durante la partida. El desplazamiento máximo por sentido es 0,12 unidades con lluvia, 0,18 en tormenta y 0,05 al calmarse; dos ondas de fases diferentes cambian la pendiente de las caras. Las posiciones originales se guardan en LowPolyWaterSurfaceData para que también funcione cuando Android no permite leer la malla importada desde CPU. El modelo importado permanece intacto.
5. **Flotación coherente.** `BoatController` consulta el collider original y combina las alturas animadas de los tres vértices de la cara alcanzada mediante coordenadas baricéntricas. Corresponde exactamente a los vértices visibles de la malla animada. El shader conserva la iluminación de caras planas y no añade un segundo desplazamiento. Así no es necesario recalcular un MeshCollider cada frame. Se conserva el margen bajo el casco y el bote sigue las pequeñas ondulaciones incluso cuando el jugador está en tierra. El viento no desvía el control de navegación.
6. **Tormenta al recoger el cuarzo.** `WeatherDirector` observa el estado del objeto original `cuarzo`. La primera recogida inicia una transición de aproximadamente 8 segundos: aumenta la cantidad de lluvia, la inclinación por viento, el ambiente sonoro y ligeramente la ondulación. Insertarlo después no reinicia la tormenta.
7. **Relámpagos ambientales.** Después de recoger el cristal, el primer relámpago se programa a los 7 segundos. Los siguientes tienen intervalos aleatorios de 9 a 16 segundos. Un destello de unos 0,65 segundos ilumina la escena mediante la luz existente, el ambiente y el cielo. Un trazo de luz lejano representa el rayo; según la dirección puede quedar fuera de la vista. No hay daño, impactos físicos ni sacudidas de cámara.
8. **Truenos con retraso.** `WeatherAudio` reproduce uno de tres truenos entre 1,2 y 2,8 segundos después del relámpago. La fuente es 3D, situada hacia el rayo, con atenuación por distancia. Los truenos pendientes se cancelan al pausar o completar la misión.
9. **Ambiente sonoro.** Se mezclan lluvia, viento y oleaje con volúmenes distintos. El mar se oye más cerca de la costa y al navegar; se reduce en las zonas altas de la isla. Los sonidos mecánicos, de recogida y del farol se conservan. Los audios nuevos se sintetizaron localmente mediante ruido filtrado y envolventes; no son grabaciones de campo ni recursos descargados. Los WAV fuente están incluidos y se comprimen para Android.
10. **Recompensa al encender el faro.** La primera activación inicia una disminución gradual del clima durante 12 segundos: cesan los nuevos relámpagos y truenos pendientes, bajan el viento y el oleaje, y queda lluvia suave. La noche permanece. Apagar y volver a encender el faro no devuelve la tormenta.
11. **Continuidad de controles y misión.** Se mantienen velocidad de caminar, altura, navegación libre, acceso al muelle, inventario, inserción del cuarzo, arranque, palanca y faro de la etapa 4. Los botones de prueba siguen disponibles. El progreso se conserva durante la partida; al reiniciar la aplicación vuelve la lluvia fuerte inicial y comienza otra vez la misión.

## Parámetros en el Inspector

- `WeatherDirector`: duración de entrada a tormenta y calma, intervalo de relámpagos, retraso de truenos e intensidad del destello.
- `RainField`: emisión de lluvia fuerte, tormenta y lluvia suave.
- `LowPolyWater`: amplitudes pequeñas del mar por estado.
- `WeatherAudio`: clips y fuentes de lluvia, viento, mar y truenos.
- `LighthouseBlink`: conserva los controles de tiempos, intensidad y giro de la etapa 4.

## Prueba recomendada en el A56

1. Instala `ElUltimoFaro_05_1_Mar_Animado.apk` sobre la versión anterior.
2. Comprueba que comienza de noche con lluvia fuerte, sin rayos antes del cuarzo.
3. Recoge y enciende el farol. Recorre el muelle caminando; observa si distingues los obstáculos y las luces de orientación.
4. Navega hacia la isla secundaria. Revisa que el agua conserve sus caras triangulares y que no atraviese el interior del bote.
5. Recoge el cuarzo y espera unos segundos: debe aumentar la tormenta, producirse un destello y luego escucharse el trueno.
6. Regresa navegando, inserta el cristal, espera el arranque y acciona la palanca.
7. Durante los siguientes 12 segundos deben disminuir lluvia, viento y oleaje. Debe seguir siendo de noche con el faro funcionando.
8. Apaga y enciende el faro: la tormenta no debe reiniciarse. Prueba también enviar la app a segundo plano y volver.

La retroalimentación principal es: visibilidad del terreno y muelles; claridad del cambio a tormenta; sonido sin tapar las interacciones; agua low poly y flotación; fluidez y temperatura del teléfono durante una vuelta completa.

## Corrección del mar tras la prueba en Android

El usuario validó lluvia, tormenta y calma en el A56, pero indicó que el mar parecía estático. La primera versión tenía amplitudes muy pequeñas y ondas amplias. La revisión 0.5.1 mueve directamente los vértices en vertical con mayor amplitud y variación entre caras. Se mantienen triángulos, controles y secuencia climática. La percepción del movimiento y el rendimiento de esta revisión aún deben comprobarse en el teléfono.

## Verificaciones realizadas

- Noche: comprobación visual del escenario sin farol y con farol; se aumentó la luz ambiental tras la primera captura para mantener legible el terreno.
- Recorrido completo en Play Mode: tres subidas/bajadas del muelle; controles táctiles y mirada independiente; navegación a la segunda isla; recogida; regreso durante tormenta; desembarque; inserción; arranque; palanca; faro. Se comprobaron 1.385 pasos de flotación durante la navegación y la oscilación del bote detenido.
- Clima: no hay relámpagos antes de recoger; transición gradual; partículas dentro del límite; destello y trueno con retraso; calma al completar; noche y lluvia suave al finalizar; no se reinicia la tormenta al alternar la palanca.
- Revisión visual enfocada final: lluvia, rayo habilitado durante el destello, maquinaria bajo lluvia y faro durante la calma. Se corrigió el encendido del trazo del rayo y se limitó el tamaño de las gotas cercanas.
- Estas son pruebas de Editor, no una medición de FPS ni una prueba física del A56. La apariencia, fluidez y mezcla sonora deben validarse en el teléfono.

## Problemas técnicos encontrados y soluciones

- La primera revisión visual terminó por falta de recursos temporales. Se eliminaron únicamente cachés regenerables de compilación Android de la copia de trabajo y se repitió la prueba correctamente. Se preservaron proyecto original, APK previas y respaldos.
- La compilación solicita dos trabajos para IL2CPP y omite información de depuración nativa que no usa esta APK, reduciendo el espacio temporal. El menú restaura los argumentos globales previos al terminar. La solicitud de concurrencia no limitó todas las tareas nativas de Bee.
- Unity terminó la compilación de código y shaders, pero su primer empaquetado Android falló por falta de espacio. Con Unity cerrado se completó el proyecto Gradle generado, usando modo offline, dos tareas y un máximo de 1,5 GB de memoria Java. El resultado fue BUILD SUCCESSFUL. Se verificaron la firma de la APK, versión 0.5.0/código 5, ARM64 y compatibilidad de firma con la entrega anterior. El informe inicial fallido de BuildPipeline se conserva como evidencia del problema; no representa el resultado del empaquetado recuperado.
- Se evita recalcular la colisión del mar en cada frame: una tabla de vértices original permite reproducir matemáticamente la superficie animada para la flotación.
- Los scripts nuevos no requieren cambiar los cuatro scripts de la maquinaria que ya funcionaban. El único script anterior de ejecución ampliado es BoatController, con referencia opcional al agua animada; las escenas anteriores siguen usando su comportamiento previo.

## Conservación y siguiente etapa

Se conserva `SampleScene`, las escenas Android 01–04, todos los GLB/FBX y los materiales compartidos anteriores. El cielo, el agua animada y las gotas tienen materiales nuevos propios de la escena 05. No se importaron paquetes ni se añadieron reflejos en tiempo real o física oceánica compleja.

La etapa 6 mantiene lo acordado: puerta interactiva del faro, vuelo y sonido de la gaviota, y movimiento de vegetación que responda al viento. Esos elementos aún no se animan en esta entrega.

## Compilar otra APK

Usa Unity 6000.6.2f1 con Android Build Support. Menú **El Último Faro > Etapa 5 > Compilar APK Android**. Selecciona un destino; el menú configura la escena 05, Android ARM64/IL2CPP y versión 0.5.1/código 6.

## Evidencia de IA

Se utilizó Codex conforme a la autorización comunicada por el usuario. Los scripts, WAV generados, escenas, capturas y verificadores son evidencia del trabajo real. No se atribuye este trabajo a Antigravity ni OpenCode. Los verificadores se encuentran en la carpeta Editor y no forman parte del juego ni añaden piloto automático al APK.
