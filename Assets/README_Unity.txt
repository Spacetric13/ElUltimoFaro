EL ULTIMO FARO - UNITY LOW-POLY ASSET

MODEL
-----
El_Ultimo_Faro_LowPoly.glb

Important named parts:
- LampGlow
- LightBeam
- Lantern_Glass
- Lantern_Roof
- Door
- Tower_Red_Lower
- Tower_White_Upper

IMPORT IN UNITY
---------------
1. Copy El_Ultimo_Faro_LowPoly.glb into Assets/Models/.
2. Import it using your GLB/glTF importer (for example glTFast).
3. Drag it into the Scene.
4. The model is built at approximately 18.7 Unity units/meters tall.

BLINKING LIGHT
--------------
The GLB intentionally keeps LampGlow and LightBeam as separate named objects
so Astra/Codex can animate them easily.

An optional LighthouseBlink.cs script is included.
To use it:
1. Create a child GameObject in the lantern called BeaconLight.
2. Add a Spot Light and aim it along the LightBeam direction.
3. Put LighthouseBlink.cs on the lighthouse root.
4. Assign BeaconLight, LampGlow renderer and LightBeam renderer.
5. Adjust onTime/offTime.

For a classic rotating lighthouse beam, parent BeaconLight + LightBeam under one
pivot and rotate that pivot around local Y.

NOTE
----
This is a lightweight low-poly reconstruction based on the supplied concept,
not a photogrammetric/exact replica.
