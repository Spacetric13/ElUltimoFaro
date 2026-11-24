MAQUINARIA ANTIGUA DE CUARZO - UNITY

ARCHIVOS
--------
Maquinaria_Antigua_Cuarzo.glb
QuartzMachineController.cs
QuartzSocketTrigger.cs
MachineLever.cs

OBJETOS IMPORTANTES DEL MODELO
------------------------------
QuartzSocket
QuartzSocket_Inner
Flywheel
MainGear
SecondaryGear
IndicatorLamp
EnergyCore
Lever
Lever_Handle
PressureGauge_Needle

IDEA DE USO
-----------
La máquina permanece apagada hasta que el GameObject existente llamado
exactamente "cuarzo" entra en el receptáculo.

Al insertarse:
1. El cuarzo se alinea con un SnapPoint.
2. Se bloquea en la máquina.
3. Se activa QuartzMachineController.
4. Flywheel y engranajes comienzan a rotar.
5. EnergyCore pulsa.
6. IndicatorLamp puede encenderse.
7. La máquina queda en estado IsPowered = true.

IMPORTACIÓN
-----------
1. Importa Maquinaria_Antigua_Cuarzo.glb a Assets/Models.
2. Arrástrala a la escena.
3. Añade QuartzMachineController al objeto raíz de la máquina.
4. Asigna desde la jerarquía:
   Flywheel
   MainGear
   SecondaryGear
   EnergyCore
   IndicatorLamp Renderer
5. Crea un Point Light pequeño dentro de IndicatorLamp y asígnalo a Indicator Light.
6. En QuartzSocket crea un hijo vacío llamado QuartzSnapPoint.
7. Añade un Box Collider o Sphere Collider con Is Trigger = true al área de inserción.
8. Añade QuartzSocketTrigger al mismo objeto.
9. Asigna Machine y QuartzSnapPoint.
10. Asegúrate de que el cuarzo tenga Collider y Rigidbody.

NOTA XR
-------
El script busca el objeto por nombre "cuarzo". Si detecta XRGrabInteractable
después de insertarlo, intenta desactivarlo sin hacer que el proyecto dependa
obligatoriamente del paquete XR.

ESCALA
------
La máquina mide aproximadamente 2.8 m de ancho, 1.7 m de profundidad y
2.7 m de alto. Ajusta Transform Scale en Unity si lo necesitas.

ESTILO
------
Low-poly, maquinaria antigua / industrial, hierro oscuro, latón envejecido,
cobre, madera y cámara de energía. Está diseñada para encajar visualmente
con un faro antiguo y una misión centrada en cuarzo.
