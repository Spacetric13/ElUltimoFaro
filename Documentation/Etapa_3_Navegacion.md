# Etapa 3 — muelle y navegación libre

Escena: Assets/Scenes/SampleScene_Android_03.unity.
Menú: El Último Faro > Etapa 3 > Abrir navegación Android.
Dispositivo objetivo: Samsung A56 / Android 16; pantalla táctil horizontal.

## Funcionalidades y lógica

1. Acceso al muelle: una superficie de colisión continua suaviza el descenso entre los peldaños y las tablas. Se conservan las mallas visibles; se desactivan solo los colliders individuales de pCube1 a pCube7 en esta escena para evitar que compitan con esa superficie. Las tablas del muelle siguen siendo suelo transitable.
2. Bote existente: pCube22 y pCube23 conservan mallas, nombres y materiales; se agrupan bajo Bote_Navegable con un pivote centrado. El modelo importado original no se edita. La instancia de faro isla se desempaqueta solo en esta tercera escena para permitir mover esas dos piezas juntas.
3. Embarque contextual: cerca del bote y de un acceso costero, EMBARCAR suspende la cápsula de caminar y coloca la cámara en el puesto del bote. No se crean copias del personaje, cuarzo o farol.
4. Conducción manual: arriba en el control izquierdo avanza, abajo retrocede, izquierda/derecha giran. Aceleración y frenado graduales; puede girarse sin avanzar. La mirada de la derecha funciona independientemente del rumbo del bote. El juego no tiene piloto automático ni una ruta obligatoria.
5. Flotación: se consulta la altura del mar existente y se suaviza la altura del bote. El collider de consulta del agua no empuja al personaje. El casco comprueba colisiones antes de desplazarse o girar.
6. Límites de navegación: zona generosa alrededor de ambas islas; el avance hacia el exterior se reduce progresivamente y el casco se mantiene dentro del área. Es posible girar y regresar. No se añaden paredes visibles ni flechas de ruta.
7. Orientación por luz: luz cálida en un poste del muelle y luz fría sobre la costa accesible de la isla del cuarzo. Son luces y pequeños puntos emisivos nuevos; no se cambian los materiales originales ni se enciende el faro de la misión.
8. Desembarque: se habilita cerca de los dos accesos cuando el bote está detenido y se ha soltado el control. Se comprueba suelo y espacio para la cápsula antes de colocar al jugador en tierra. El bote permanece donde se dejó para poder volver a embarcar.
9. Viaje con objetos: los objetos recogidos permanecen bajo la cámara; el farol sigue encendido/apagado según su estado. Embarcar, navegar, desembarcar y volver no reinician el inventario.
10. Controles de prueba: se conservan los dos teletransportes previos y se añade PRUEBA: BOTE AL MUELLE. Este último devuelve el bote y al jugador al acceso principal conservando los objetos. Si se usa otro teletransporte a bordo, se detiene el bote, se separa al jugador y se restaura el movimiento a pie.

Esta lista describe lo construido; no afirma que ya esté completo el pack final de diez interacciones distintas del certamen. Maquinaria, inserción del cuarzo y encendido del faro siguen pendientes.

## Cómo probar en el A56

1. Instala ElUltimoFaro_03_Navegacion.apk sobre la versión anterior.
2. Baja al muelle caminando y vuelve a subir. Comprueba especialmente el tramo que antes se atascaba.
3. Recoge el farol y prueba encenderlo. Acércate al bote y pulsa EMBARCAR.
4. Prueba avanzar, retroceder, girar y mirar con el otro dedo al mismo tiempo. Al soltar el control, el bote frena.
5. Navega rodeando la costa hacia la isla del cuarzo; su iluminación fría señala el acceso. Puedes elegir tu recorrido.
6. Acércate a esa costa, suelta el control y espera a que se habilite DESEMBARCAR. No se permite desembarcar en mar abierto.
7. Recoge el cuarzo, vuelve a embarcar y regresa al muelle de luz cálida. Desembarca y verifica ambos objetos.
8. Prueba un límite del mar: debes poder girar y volver. Envía la app a segundo plano mientras navegas y comprueba que al volver el bote esté detenido.
9. Los accesos PRUEBA permiten repetir rápidamente. Si dejaste el bote lejos usando un teletransporte, PRUEBA: BOTE AL MUELLE recupera ambos en el muelle.

Editor: WASD para caminar o conducir; botón derecho para mirar; B para embarcar/desembarcar; E para recoger; F para el farol; R para volver al inicio. También funcionan los botones visibles.

## Archivos principales

- BoatController.cs: embarque, movimiento, flotación, colisiones, límites, desembarque y recuperación de prueba.
- MobilePlayerController.cs: mantiene el control de mirada y envía el control izquierdo al bote mientras está embarcado; suspende el movimiento a pie durante ese estado.
- FaroStageThreeTools.cs: preparación y compilación de la tercera escena. BuildAndroid selecciona únicamente esta escena, versión 0.3.0/código 3.
- FaroStageThreeVerification.cs: pruebas de recorrido. La conducción automatizada de ese archivo es exclusiva de pruebas del Editor y no se incluye en el juego.
- Generated/PasoMuelle.asset: superficie física del acceso. Los materiales Luz_guia_* corresponden solo a los pequeños emisores nuevos.

## Continuidad del proyecto

Se conservan SampleScene, SampleScene_Android_01 y SampleScene_Android_02. Se respeta el nuevo asset back.jpg encontrado en el proyecto del usuario. No se importaron paquetes ni se conectaron los scripts de la maquinaria.

El usuario confirmó que la etapa 2 funcionaba en su A56, salvo la dificultad de bajar al muelle. La etapa 3 necesita una nueva validación en el teléfono; las pruebas automatizadas del Editor no sustituyen esa comprobación.

## Verificación automática y observaciones

La prueba final de Play Mode aprobó tres descensos y ascensos completos del muelle con control táctil y distintos pasos de tiempo; embarque; envío real del joystick al bote; mirada que no altera su rumbo; avance y retroceso; navegación hasta la isla secundaria; desembarque; recogida del cuarzo; reembarque y regreso al muelle conservando cuarzo y farol encendido. También aprobó la detención ante un obstáculo sólido, límites del mar, salida desde esos límites, pérdida de foco y uso de los teletransportes a bordo.

Se corrigió una diferencia de alcance que permitía desembarcar pero no volver a embarcar desde la isla secundaria. Ambos usan ahora el mismo alcance de 1,7 unidades, con comprobación de obstáculos al embarcar.

En un ensayo previo hubo una detención aislada cerca de la unión entre rampa y tablas. No se reprodujo en el ensayo final de tres ciclos y distintos pasos de tiempo. El acceso al muelle debe revisarse especialmente en la prueba física del A56. La prueba también distingue la liberación legítima de entradas al cambiar el foco del Editor de un bloqueo del recorrido.

Se revisaron capturas para ajustar el puesto de cámara del bote y apoyar los puntos de luz en el muelle/costa. La apariencia y rendimiento en el teléfono requieren validación del usuario.

## Resultado de compilación

Unity BuildPipeline: Succeeded, 0 errores. APK 0.3.0 (código 3), ARM64/IL2CPP, mínimo API 26, objetivo API 36, identificador com.elultimofaro.experiencia. Firma APK v2 válida y certificado igual al de la etapa 2: puede instalarse como actualización.
Tamaño: 30837012 bytes. Se completó el empaquetado con Gradle desde Unity.

Unity advierte que en futuras versiones será necesario habilitar explícitamente la colisión precalculada en algunos meshes importados; esta versión aún la prepara automáticamente durante la compilación. No se cambiaron masivamente los importadores de modelos para eliminar esa advertencia.

La comparación del original con el respaldo confirma que los únicos archivos existentes modificados fueron MobilePlayerController.cs, TagManager.asset (nombre de capa FaroWater) y este índice histórico de documentación. Las escenas anteriores, modelos y materiales originales se conservaron; el resto son archivos nuevos de la etapa 3.
