# UnityLaptop Online ChatOps

This repository uses one deployment path for chat-driven development:

1. GitHub is the source of truth.
2. Unity Build Automation watches the production branch.
3. Every accepted change is compiled in Unity Cloud as WebGL.
4. Build validation runs automatically before packaging.
5. Successful WebGL builds are shared online.

No local Unity Hub step is part of this workflow.

## Unity Dashboard configuration

Configure the existing Unity Cloud project in the web dashboard:

- DevOps > Build Automation > connect source control to `karlokalinic/my-unity-project`.
- Build configuration name: `UnityLaptop-WebGL`.
- Branch: `main`.
- Project subdirectory: leave empty because `Assets/`, `Packages/`, and `ProjectSettings/` are at repository root.
- Unity version: auto-detect from `ProjectSettings/ProjectVersion.txt` (currently Unity 6000.4.0f1).
- Platform: WebGL.
- Auto-build: enabled.
- Build Automation > Settings > General > Automatic build sharing: enabled.
- Unit/EditMode tests: enable as part of the cloud build gate when available for the configuration.
- Caching: workspace caching after the first successful build for fastest iteration.

## Chat workflow

A request such as `ADD XYZ` or `IMPLEMENT XYZ` is treated as a production change request.

The implementation flow is:

- inspect current production code and scene-generation code;
- implement the change on a dedicated branch;
- add/update tests where the behavior is testable without scene rendering;
- preserve physical-world rules, serialized references, and deterministic scene setup;
- run repository-level validation where possible;
- merge only when the change is internally coherent;
- Unity Build Automation picks up `main`, compiles, validates, and produces the WebGL build;
- the successful cloud build becomes the playable preview for that commit.

## Build safety

`Assets/Editor/CloudBuildGuard.cs` executes before every Unity build and blocks deployment when:

- no enabled build scenes exist;
- an enabled scene file is missing;
- an enabled scene contains missing MonoBehaviour script references.

The guard is deliberately platform-agnostic so the same repository remains buildable for native targets later, while WebGL remains the continuous preview target.

## Source control rule

Do not create a second primary repository in UVCS. GitHub remains authoritative because it is the control plane used by chat-driven implementation, review, history, and automation. Unity Build Automation consumes GitHub directly.

## Cloud services

UGS features such as Cloud Code, Remote Config, Cloud Save, Economy, or other server-side configuration should be deployed through file-based UGS CLI/REST workflows from versioned repository content once required by gameplay. They must not become a second source of truth maintained manually in the Dashboard.
