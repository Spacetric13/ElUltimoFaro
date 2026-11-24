# Etapa 2 — escala del jugador, cuarzo y farol

Escena: Assets/Scenes/SampleScene_Android_02.unity.
Menú de Unity: El Último Faro > Etapa 2 > Abrir cuarzo y farol Android.
Se mantienen SampleScene y la escena del primer prototipo para comparar.

## Qué cambia

1. Cámara a 0,5167 unidades sobre los pies (antes 1,55), cápsula de 0,5667 de alto y radio 0,0667. Velocidad de 0,6667 unidades/segundo. La escala del escenario permanece intacta. Se conserva el ajuste de posición del árbol guardado por el usuario.
2. Recogida contextual de los objetos existentes cuarzo y Farol_de_mano. El jugador debe estar cerca y mirar hacia ellos; la geometría sólida impide recoger a través de paredes.
3. Ambos objetos pueden llevarse simultáneamente. Su representación en mano se adapta al tamaño del jugador, conservando sus mallas y materiales. El cuarzo conserva su emisión original. No se crean duplicados ni se activa la maquinaria todavía.
4. Farol con interruptor táctil, luz cálida local sin sombras y emisión del globo que cambia al encender/apagar. Los materiales compartidos no se editan: la emisión se controla por instancia durante la ejecución.
5. Sonido breve de recogida, sonido de interruptor, confirmaciones escritas e inventario visible. Los sonidos se sintetizan en código; no se descargaron assets.
6. Los objetos recogidos permanecen con el jugador al regresar al inicio o recuperarse de una caída. No se permite arrojarlos al mar. La recogida es por sesión; cerrar y volver a abrir la aplicación reinicia los objetos.
7. Las tablas y peldaños existentes pCube1 a pCube15 se reconocen como suelo del muelle. Se permite un escalón de 0,18 unidades para los peldaños originales; no se redujo este umbral mecánicamente a un tercio porque impediría subir de vuelta. pCube22 y pCube23 no se modificaron.
8. Accesos rotulados PRUEBA: IR AL FAROL y PRUEBA: ISLA CUARZO. Son teletransportes temporales para evaluar los objetos, no navegación del bote ni parte de la secuencia final.

## Probar en Samsung A56

- Instala ElUltimoFaro_02_Cuarzo_Farol.apk y abre el juego en horizontal.
- Acércate a la puerta del faro: comprueba la nueva altura y el movimiento simultáneo con dos dedos.
- Pulsa PRUEBA: IR AL FAROL; mira hacia abajo y hacia el farol. Cuando se habilite RECOGER FAROL, presiónalo.
- Enciende y apaga el farol; comprueba el objeto en mano, el cambio de luz, el texto y el sonido. La luz local puede resultar más sutil bajo la iluminación diurna del escenario.
- Pulsa PRUEBA: ISLA CUARZO; mira al cristal y pulsa RECOGER CUARZO.
- Pulsa VOLVER AL INICIO. Debes conservar ambos objetos y poder seguir encendiendo/apagando el farol.
- Prueba el muelle, los límites de las islas y volver a la aplicación después de enviarla a segundo plano.

En Editor: WASD, botón derecho para mirar, E para recoger, F para encender/apagar y R para volver. Los botones de pantalla también funcionan.

## Código y lógica

- MobilePlayerController: movimiento y recuperación; escala de recorrido configurable, preservando los valores del primer prototipo; TeleportTo limpia velocidad y controles.
- PortableItem: controla si el objeto está recogido, lo sujeta a su punto de mano y gestiona la luz del farol.
- MobileInteraction: alcance, dirección de mirada y obstrucciones; botones, inventario, sonidos y accesos de prueba.
- FaroStageTwoTools: prepara la escena de etapa 2 y compila Android.
- FaroStageTwoVerification: prueba automática en Play Mode, incluyendo movimiento, recogida bloqueada por obstáculos, ambas recogidas, luz y retención de inventario.

## Pendiente después de esta prueba

Bote de conducción libre, orientación por iluminación, socket del cuarzo, maquinaria y arranque suave, palanca, encendido/animación del faro, ambientación y demás interacciones de la pauta. Este bloque no representa todavía el juego final ni diez interacciones distintas completas.

El usuario ya confirmó que el primer APK funciona en el A56 y que movimiento, mirada simultánea y mapa se ven correctamente. Esta segunda versión requiere su propia prueba física.

## Verificación realizada

Prueba automatizada en Unity Play Mode: aprobada. Comprobó altura y cápsula a un tercio, apoyo inicial, movimiento táctil y dedos independientes, rechazo de recogida lejana o detrás de un obstáculo, recogida mediante el botón contextual de ambos objetos originales, rechazo de duplicados, luz encendida/apagada, puntos de prueba seguros, regreso y recuperación conservando objetos y luz.
Se inspeccionaron capturas del juego para revisar la altura relativa a la puerta y la presentación de los objetos/UI. El cuarzo en mano se desplazó hacia el centro para despejar los botones.
No se realizó una prueba física de esta versión en el A56; la validación del primer APK comunicada por el usuario no se sustituye por una validación ficticia del segundo.

## Resultado de compilación

Unity BuildPipeline terminó en Succeeded, con 0 errores. APK ARM64/IL2CPP, versión 0.2.0 (código 2), paquete com.elultimofaro.experiencia, mínimo API 26 y objetivo API 36.
La firma APK v2 es válida y el certificado coincide con el del primer APK, por lo que puede instalarse como actualización. Esta compilación completó también el empaquetado Gradle directamente desde Unity.
Tamaño del APK: 30818512 bytes.

La compilación generó advertencias heredadas de Unity y recomendaciones del auditor sobre meshes; no se presentan como errores de compilación ni se intentó una limpieza masiva de assets. La comprobación del proyecto original contra el respaldo confirma que SampleScene, la escena de etapa 1, los modelos y los materiales originales se conservaron.
