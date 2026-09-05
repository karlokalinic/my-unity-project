# Third-Party Attributions

## The Rake — Sealife Fan 3

- Work: **The Rake**
- Creator: **Sealife Fan 3** (`@SealifeFan3`)
- Source: https://sketchfab.com/3d-models/the-rake-92898b04b24e4315b0673fcfe307f64e
- License: **Creative Commons Attribution 4.0 International (CC BY 4.0)**
- License text: https://creativecommons.org/licenses/by/4.0/

UNITYLAPTOP's `RakeEncounterController` supports this asset as the intended creature visual and animation source. The authorized Sketchfab download itself is not vendored in this repository because the protected Sketchfab binary download endpoint requires an authenticated download flow that is not available to the repository automation. When the authorized Unity-importable asset is supplied, expose its prefab/model through `Assets/Resources/ThirdParty/TheRake/TheRake` (resource path `ThirdParty/TheRake/TheRake`); the encounter will bind it automatically, fit it to the scene scale, disable conflicting imported physics, and use a run animation clip when one is present.

Until that binary is present, the encounter deliberately uses a procedural physical stand-in so the cinematic, room geometry, camera transition, lighting, audio, collision, and gameplay integration remain testable without misrepresenting the third-party asset as included.

## The Abominable Snowman (READ DESC) — toro ardido modelos 3d

- Work: **THE ABOMINABLE SNOWMAN (READ DESC)**
- Creator: **toro ardido modelos 3d** (`@toro_ardido_modelos_3d`)
- Source: https://sketchfab.com/3d-models/the-abominable-snowman-read-desc-75699f1d63e94e23b40a73df6a3d65b8
- Sketchfab status verified from the public listing: **animated** and **downloadable**.
- License note: the exact model-license field was not reliably exposed through the unauthenticated page/search path used by repository automation, so this repository does not assert a license value that was not verified. Confirm the model's current Sketchfab license terms in the authenticated download flow before committing the binary.

The opposite-side pitch-black encounter binds this model from `Assets/Resources/ThirdParty/AbominableSnowman/AbominableSnowman` (resource path `ThirdParty/AbominableSnowman/AbominableSnowman`). The runtime fits the model to the encounter scale, disables conflicting imported physics, creates matching compound collision, and selects a charge animation by preferring clips containing `run`, `sprint`, `charge`, or `walk`, falling back to the first available animation clip.

Sketchfab's download API requires an authenticated user request and returns temporary archive links. Because repository automation does not possess the user's Sketchfab OAuth token, the binary is not falsely vendored. Until the authorized model file is present, the second encounter uses a larger physical procedural stand-in while preserving the complete door, darkness, first-person transition, flashlight, audio, charge, and blackout sequence.
