# UnityLaptop

## Production source of truth

GitHub is the canonical source of truth for the project.

- Gameplay scripts: `Assets/Scripts/`
- Editor/build tooling: `Assets/Editor/`
- Shipping/build scenes: `Assets/Scenes/`
- Automated tests: `Assets/Tests/EditMode/Editor/`
- Art mapping pipeline: `Assets/Data/Art/HolstinArtPack.asset`, `Assets/Art/`
- Build scene list: `ProjectSettings/EditorBuildSettings.asset`

## Online ChatOps deployment

The production workflow is intentionally online-first:

1. Changes are implemented and versioned in GitHub.
2. Unity Build Automation watches `main`.
3. Unity Cloud compiles and validates the project.
4. The continuous preview target is WebGL.
5. Successful builds are automatically shared online.

See `ONLINE_CHATOPS.md` for the exact cloud configuration and deployment rules.

`Assets/Editor/CloudBuildGuard.cs` runs before Unity packaging and blocks builds with missing enabled scenes or missing MonoBehaviour script references.

## Repository rules

Do not introduce a second primary source-control repository. UVCS may be used only if a later workflow explicitly requires it; GitHub remains authoritative for chat-driven development and Unity Build Automation consumes GitHub directly.

Generated Unity folders (`Library`, `Temp`, `Logs`, `UserSettings`, build output, IDE state) remain excluded from version control.

Keep production scene references deterministic through `ProjectSettings/EditorBuildSettings.asset`.
