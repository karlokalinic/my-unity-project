# Third-Party Attributions

## The Rake — Sealife Fan 3

- Work: **The Rake**
- Creator: **Sealife Fan 3** (`@SealifeFan3`)
- Source: https://sketchfab.com/3d-models/the-rake-92898b04b24e4315b0673fcfe307f64e
- License: **Creative Commons Attribution 4.0 International (CC BY 4.0)**
- License text: https://creativecommons.org/licenses/by/4.0/

UNITYLAPTOP's `RakeEncounterController` supports this asset as the intended creature visual and animation source. The authorized Sketchfab download itself is not vendored in this repository because the protected Sketchfab binary download endpoint requires an authenticated download flow that is not available to the repository automation. When the authorized Unity-importable asset is supplied, expose its prefab/model through `Assets/Resources/ThirdParty/TheRake/TheRake` (resource path `ThirdParty/TheRake/TheRake`); the encounter will bind it automatically, fit it to the scene scale, disable conflicting imported physics, and use a run animation clip when one is present.

Until that binary is present, the encounter deliberately uses a procedural physical stand-in so the cinematic, room geometry, camera transition, lighting, audio, collision, and gameplay integration remain testable without misrepresenting the third-party asset as included.
