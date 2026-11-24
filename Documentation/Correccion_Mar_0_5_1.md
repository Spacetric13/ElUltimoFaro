# Corrección del mar — 0.5.1

El clima anterior fue validado por el usuario: lluvia inicial, tormenta tras recoger el cuarzo y calma al encender el faro. El mar no se percibía en movimiento en el A56.

## Cambio

LowPolyWater mueve únicamente la altura de los 7.372 vértices originales; conserva sus 2.601 triángulos y las caras low poly. Se usan dos ondas con distinta dirección y fase. La amplitud máxima por sentido pasa a 0,12 unidades al inicio, 0,18 en tormenta y 0,05 en calma. Las transiciones siguen controladas por WeatherDirector.

Se crea una malla temporal al abrir la escena. LowPolyWaterSurfaceData guarda las posiciones originales porque la malla importada tiene isReadable=false y una lectura directa podría fallar en Android. El modelo importado y su collider permanecen intactos. El shader ilumina los triángulos ya desplazados sin sumar otra animación.

La flotación interpola los mismos desplazamientos en las caras del collider original. No se reconstruye un collider en cada cuadro. Los arrays de animación se reutilizan; no hay subdivisiones, texturas nuevas ni paquetes adicionales.

## Archivos principales

- Assets/ElUltimoFaro/Runtime/LowPolyWater.cs: animación de vértices y cálculo de altura para el bote.
- Assets/ElUltimoFaro/Runtime/LowPolyWaterSurfaceData.cs y Generated/Weather5/Mar_vertices_originales.asset: posiciones y topología originales.
- Assets/ElUltimoFaro/Shaders/LowPolySea.shader: sombreado de caras planas.
- Assets/Scenes/SampleScene_Android_05.unity: amplitudes por estado.
- Editor/FaroWaterMotionVerification.cs: movimiento real de vértices, topología, conservación del modelo y coincidencia con flotación.

## Prueba en el A56

Instala ElUltimoFaro_05_1_Mar_Animado.apk sobre la anterior. Mira unos segundos las caras del mar desde el muelle, navega y compara el oleaje antes del cuarzo, durante la tormenta y después de encender el faro. Revisa la flotación, embarque, desembarque y fluidez durante una vuelta completa.

Las comprobaciones automatizadas son del Editor. Los resultados se entregan en Verificacion_Mar_0_5_1.txt; no sustituyen una prueba física del teléfono.

## Siguiente etapa propuesta: entorno vivo

1. Puerta existente Door: abrir y cerrar con el botón contextual, giro desde la bisagra, límites, colisión y sonido. Antes se comprobará que la geometría del acceso permita el movimiento; no se añadirá un interior nuevo por defecto.
2. gaviota_en_vuelo: recorrido suave alrededor de la isla o sobre el mar, orientación según el vuelo, pequeña variación de altura y sonidos 3D ocasionales. Se reutiliza el modelo; no se inventa un rig de alas.
3. Vegetación: movimiento leve en la parte superior, bases fijas y variación entre plantas; mayor viento en tormenta y menor al calmarse, sin girar árboles enteros como bloques rígidos.
4. Revisión integrada: claridad de la interacción de la puerta, mezcla de sonidos y rendimiento de una vuelta completa en Android.

Esta propuesta no está implementada en la revisión 0.5.1.
