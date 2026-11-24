# Etapa 4 — cuarzo, máquina y faro

Escena: Assets/Scenes/SampleScene_Android_04.unity.
Menú: El Último Faro > Etapa 4 > Abrir máquina y faro Android.
Objetivo: Samsung A56, Android 16, controles táctiles horizontales.

## Cambios de esta etapa

1. Caminar un 35 % más rápido: de 0,6667 a 0,9 unidades por segundo. Se mantiene la altura reducida del personaje y la velocidad de navegación.
2. Flotación: el bote consulta quince puntos distribuidos sobre el casco y toma la cresta más alta con margen. Sigue suavemente los descensos y corrige los ascensos antes de que atraviesen el interior. El mar conserva su geometría estática; animarlo no era necesario para corregir la flotación.
   La prueba con la velocidad nueva detectó huecos entre las tablas y al terminar la rampa. Una superficie física continua y una prolongación corta de la rampa los cubren solamente en esta escena; no cambian las mallas visibles. La flotación también comprueba suelo bajo el casco para impedir que una cresta permita navegar sobre una costa baja.
3. Inserción contextual del cuarzo original: al acercarse y mirar la máquina, el botón de recogida pasa a INSERTAR CUARZO. Se valida la instancia exacta llamada cuarzo, su estado recogido, la distancia y la visibilidad. El cristal se centra y orienta en QuartzSnapPoint, se ajusta al receptáculo y queda bloqueado.
4. Arranque gradual: QuartzMachineController incrementa la energía en 2,5 segundos. Flywheel y sus radios giran juntos; MainGear y SecondaryGear giran en sentidos opuestos. EnergyCore pulsa suavemente. Se encienden el indicador y el núcleo, y aumenta el volumen/tono del sonido mecánico.
5. Palanca condicionada: antes de insertar el cuarzo aparece Faro sin energía; durante el arranque no se puede encender el faro. Al alcanzar energía completa, el botón permite accionar la palanca con animación y sonido.
6. Faro: LighthouseBlink activa los elementos existentes LampGlow y LightBeam, junto con una luz nueva, con transiciones suaves y rotación lenta del haz. Tiempos, intensidad, velocidad y duración de transición son ajustables en el Inspector.
7. Confirmación: el estado superior muestra la etapa actual; la primera activación confirma que se devolvió la luz. Se puede apagar y encender nuevamente el faro sin perder el cuarzo ni la energía de la máquina.

## Secuencia integrada

Recoger el farol y encenderlo → embarcar → navegar libremente a la isla secundaria → desembarcar y recoger cuarzo → volver al muelle → subir caminando al faro → mirar la maquinaria y pulsar INSERTAR CUARZO → esperar el arranque → pulsar ENCENDER FARO.

Para pruebas rápidas siguen disponibles los controles PRUEBA. No es necesario usarlos para completar el viaje. E en el Editor realiza la acción contextual; F controla el farol; B embarca/desembarca.

## Conservación de modelos

No se editan los GLB, sus materiales compartidos ni los nombres de las piezas. Las instancias de maquinaria y faro se desempaquetan únicamente en la escena nueva para añadir pivotes centrados; así se evita que las mallas con vértices desplazados giren alrededor del origen común del archivo. Se desactivan colliders de las piezas móviles, manteniendo las colisiones del cuerpo y del entorno. Los efectos de emisión usan MaterialPropertyBlock.

Los scripts originales QuartzMachineController.cs, QuartzSocketTrigger.cs, MachineLever.cs y LighthouseBlink.cs se reutilizan y amplían. Los sistemas de inventario y navegación se amplían para admitir el cristal bloqueado y la flotación. Las escenas anteriores y los modelos importados se conservan. Los scripts compartidos sí reciben estas mejoras.

Los sonidos son generados proceduralmente por código, sin descargar recursos ni importar paquetes. Se pueden sustituir posteriormente por clips definitivos.

## Retroalimentación solicitada en el A56

- ¿La velocidad al caminar resulta cómoda sin perder precisión cerca del muelle?
- ¿El interior del bote y la cámara permanecen fuera del agua durante el recorrido?
- ¿Aparece INSERTAR CUARZO al acercarte y mirar la maquinaria con el cristal recogido?
- ¿Se aprecian el arranque suave, los engranajes, las luces y el sonido?
- ¿La palanca enciende/apaga el faro y se ve la rotación y el parpadeo desde la isla?

## Alcance pendiente

Esta etapa integra la misión del faro. Quedan para bloques posteriores la puerta, las animaciones ambientales de gaviota/vegetación/mar, el ambiente sonoro definitivo, la presentación final y la revisión completa de la pauta. El progreso se conserva durante la partida; al cerrar y abrir la aplicación la misión comienza de nuevo. Los teletransportes siguen visibles para pruebas.

## Evidencia técnica

La asistencia de IA utilizada es Codex, conforme a la aclaración del profesor comunicada por el usuario. Los scripts de verificación en Editor documentan las comprobaciones reproducibles; no se incluyen en el APK ni añaden navegación automática al juego. La revisión en un teléfono físico corresponde a la prueba del usuario.

## Compilar nuevamente

En Unity usa El Último Faro > Etapa 4 > Compilar APK Android y elige el destino. Esta acción selecciona la escena 04 y configura la compilación Android 0.4.0, código 4. Usa Unity 6000.6.2f1 con Android Build Support.

## Verificación

La prueba automática de Play Mode aprobó tres subidas y bajadas del muelle, navegación con controles táctiles, mirada independiente, ida y vuelta llevando cuarzo y farol, colisiones, límites, pérdida de foco, teletransportes de prueba y bloqueo del cuarzo. También verificó energía intermedia durante el arranque, habilitación de la palanca al finalizar, sonido, luces, parpadeo suave, rotación del haz y apagado/reactivación. Se comprobaron 1.385 posiciones de flotación durante el recorrido. La revisión visual enfocada comprobó el cristal en su receptáculo y permitió reducir la intensidad de las luces añadidas.

La automatización usa puntos de observación de prueba para revisar la máquina; no sustituye la confirmación del recorrido completo en el A56. La apariencia del mar dentro del bote y el sonido final requieren esa revisión física.
