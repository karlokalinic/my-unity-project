# my-unity-project

## Production path (canonical)

Use these folders/files as the production source of truth:

- **Unity project root**: `C:\Users\kalin\Documents\GitHub\my-unity-project` (must contain `Assets/`, `Packages/`, `ProjectSettings/`)
- **Gameplay scripts**: `Assets/Scripts/`
- **Editor tooling**: `Assets/Editor/`
- **Scenes for shipping/build**: `Assets/Scenes/` (as listed in `ProjectSettings/EditorBuildSettings.asset`)
- **Automated tests**: `Assets/Tests/EditMode/Editor/`
- **Art mapping pipeline**: `Assets/Data/Art/HolstinArtPack.asset`, `Assets/Art/`

Do not open a nested/cache folder as the Unity project root (for example `...\my-unity-project\my-unity-project`), because it can produce package namespace compile errors from `Library/PackageCache/*`.

## Unity Cloud / DevOps onboarding

See [`UNITY_CLOUD_SETUP.md`](UNITY_CLOUD_SETUP.md) for the Unity Version Control migration path, Unity Gaming Services project linkage, Cloud Code SDK bootstrap, and recommended cloud-service rollout order.

## Non-production archive

Non-production docs and scenes are intentionally excluded from GitHub.

- Local-only archive folder: `LOCAL/non_production_backup/`

`LOCAL/` is git-ignored so it can be moved out of the project without affecting production source control.

## CI gate

A GitHub Actions workflow is provided at:

- `.github/workflows/unity-editmode-tests.yml`

It runs Unity **EditMode** tests on push/PR and currently gates on `UNITY_LICENSE` secret availability.

## Notes

- Scene mutation/bootstrap changes should be applied manually in edit mode via the `VerticalSliceScenaBootstrap` context menu unless intentionally overridden.
- Keep production scene references deterministic through `ProjectSettings/EditorBuildSettings.asset`.

## Onboarding & Testing

- Open the project root containing `Assets/`, `Packages/`, and `ProjectSettings/`.
- Use scenes listed in `ProjectSettings/EditorBuildSettings.asset` as canonical production scenes.
- Run EditMode tests from Unity Test Runner or CI workflow `.github/workflows/unity-editmode-tests.yml`.
