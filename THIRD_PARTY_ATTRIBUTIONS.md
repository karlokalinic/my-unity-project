# Third-Party Attributions

## The Rake — Sealife Fan 3

- Work: **The Rake**
- Creator: **Sealife Fan 3** (`@SealifeFan3`)
- Source: https://sketchfab.com/3d-models/the-rake-92898b04b24e4315b0673fcfe307f64e
- License: **Creative Commons Attribution 4.0 International (CC BY 4.0)**
- License text: https://creativecommons.org/licenses/by/4.0/
- Repository asset: `Assets/Resources/ThirdParty/TheRake/TheRake.fbx`

The authorized Sketchfab package was supplied directly by the project owner and is now vendored in UNITYLAPTOP. The source FBX contains the creature rig and animation takes including `metarig|run`, `metarig|walk`, idle and attack takes. The accompanying textures are also vendored under `Assets/Resources/ThirdParty/TheRake/`; texture resolution/compression is reduced for WebGL while preserving the authored material inputs.

`RakeEncounterController` loads the model through resource path `ThirdParty/TheRake/TheRake`, fits it to the established scene scale, disables conflicting imported physics, supplies gameplay-owned compound collision, and prefers the imported run animation for the cinematic charge.

## The Abominable Snowman (READ DESC) — toro ardido modelos 3d

- Work: **THE ABOMINABLE SNOWMAN (READ DESC)**
- Creator: **toro ardido modelos 3d** (`@toro_ardido_modelos_3d`)
- Source: https://sketchfab.com/3d-models/the-abominable-snowman-read-desc-75699f1d63e94e23b40a73df6a3d65b8
- License: **Creative Commons Attribution 4.0 International (CC BY 4.0)**
- License text: https://creativecommons.org/licenses/by/4.0/
- Repository asset: `Assets/Resources/ThirdParty/AbominableSnowman/AbominableSnowman.glb`

The authorized Sketchfab package was supplied directly by the project owner. Its embedded glTF asset metadata identifies the title, creator, source URL and `CC-BY-4.0` license. UNITYLAPTOP vendors a WebGL-focused derivative that preserves the complete rendered creature mesh, skin/skeleton and the authored `RUN (GORILLA)` animation required by this one-shot encounter while removing unrelated animation payload from this runtime asset.

The project pins Unity glTFast for editor-time `.glb` import. `RakeEncounterController` loads the resulting native prefab through resource path `ThirdParty/AbominableSnowman/AbominableSnowman`, fits it to the Snowman encounter scale, disables conflicting imported physics, supplies gameplay-owned compound collision, and selects the imported run animation for the charge.

## Build integrity

`CloudBuildGuard` blocks deployment if either creature fails to import as a loadable `GameObject`, lacks a `SkinnedMeshRenderer`, or lacks a run/sprint/charge/walk `AnimationClip` at the exact `Resources` path used by runtime gameplay. This prevents production WebGL builds from silently shipping the procedural stand-ins when the intended licensed assets are expected.
