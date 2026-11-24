# Reproducción de la compilación

Usar Unity 6000.6.2f1 con sus componentes Android, SDK, NDK y OpenJDK. Las versiones de paquetes se conservan en `Packages/manifest.json` y `packages-lock.json`. Abrir `Hall_Quest`, seleccionar Android y ejecutar el comando de compilación final del menú del juego.

La copia del repositorio modifica únicamente las rutas de salida de herramientas del editor: ahora quedan dentro de `Builds/`, que Git ignora. No se incorporó la caché de paquetes ni los archivos temporales del equipo de desarrollo.

## Incidencia del entorno de construcción

En el equipo de desarrollo, Gradle no confiaba en un certificado utilizado para descargar dependencias de Google. El código nativo y los recursos ya se habían generado; se completó la tarea Gradle `:launcher:assembleRelease` usando la confianza de certificados de Windows y dos trabajadores. No se desactivó TLS. Este ajuste responde al entorno de ese equipo y no es necesariamente necesario en otra red.

La invocación adicional a Gradle utilizó:

```text
:launcher:assembleRelease --max-workers=2 --console=plain
-Dorg.gradle.jvmargs=-Xmx4096M -Djavax.net.ssl.trustStoreType=Windows-ROOT -Djavax.net.ssl.trustStore=NONE
```

Es una opción de Java para Windows; no se debe copiar a macOS o Linux. Primero se recomienda usar el flujo normal de Unity. En caso de error, revisar el registro antes de modificar certificados o dependencias.

El APK entregado fue comprobado con `aapt` y `apksigner`: ARM64, categoría VR, compatibilidad declarada con Quest 3S y firma v2 válida. `build-result.txt` conserva el hash del archivo entregado.
