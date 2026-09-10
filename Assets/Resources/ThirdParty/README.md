# Pitch-black encounter creature assets

These assets are runtime-critical and are loaded by `RakeEncounterController` through Unity `Resources` paths.

- `TheRake/TheRake.fbx` -> `ThirdParty/TheRake/TheRake`
- `AbominableSnowman/AbominableSnowman.glb` -> `ThirdParty/AbominableSnowman/AbominableSnowman`

Do not rename or relocate these files without updating the controller presets, `CloudBuildGuard`, tests, and attribution documentation together.

The Snowman `.glb` depends on the pinned `com.unity.cloud.gltfast` package for editor-time import to a native Unity prefab and animation sub-assets. Production builds are blocked if either resource cannot be loaded as a rigged `GameObject` with a charge/run animation.
