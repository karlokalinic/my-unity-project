# UnityLaptop: Unity Cloud + Version Control setup

This repository is already a healthy Unity text-serialization project. The remaining setup is mostly account linkage: Unity Hub/Editor must authenticate against the Unity organization that owns the Student entitlement.

## What this branch already configures

- Unity Authentication SDK `com.unity.services.authentication@3.5.2`
- Unity Cloud Code SDK `com.unity.services.cloudcode@2.10.2`
- Unity Deployment tooling `com.unity.services.deployment@1.6.2`
- `Assets/Scripts/Core/UnityCloudServices.cs` as the single runtime initialization/authentication/Cloud Code gateway
- `ignore.conf` for Unity Version Control, including an explicit `.git` exclusion during migration
- Git/UVCS rules that keep generated Unity `.csproj`/`.sln` files out of source control while preserving real Cloud Code module projects under `/CloudCode`

On the first Unity open after merging this branch, Package Manager will resolve the new dependencies and update `Packages/packages-lock.json`. Commit that generated lock file after Unity has resolved successfully.

## One-time account linkage: Unity Version Control

This step cannot be stored in Git because it authenticates your personal Unity account and creates/links a remote Unity DevOps repository.

1. Open Unity Hub while signed into the Unity ID that owns the Student plan.
2. Find the existing `my-unity-project` / UnityLaptop project.
3. Open the project context menu and choose **Use Unity Version Control**.
4. Use a repository name such as `UnityLaptop` and select the Unity organization that owns the Student entitlement.
5. Select **Connect to Version Control**.
6. Open the project. If the panel is not visible, use **Window > Unity Version Control**.
7. Before the first check-in, confirm that `Library`, `Temp`, `Logs`, `UserSettings`, `.git`, and other generated/local folders are ignored. `Assets`, `Packages`, `ProjectSettings`, and `ignore.conf` must remain versioned.
8. Perform the initial UVCS check-in only after the pending-change list is clean.

### Source-of-truth rule

Do not treat GitHub and UVCS as two independent primary repositories. During migration, GitHub is the safety copy and existing CI source. After the first UVCS check-in is verified, choose one primary workflow. If the goal is maximum Unity integration, use **UVCS as primary** and point Unity Build Automation at UVCS. Keep GitHub as a deliberate mirror/backup rather than manually developing two divergent histories.

## One-time account linkage: Unity Gaming Services

1. In Unity Editor open **Edit > Project Settings > Services**.
2. Link the Editor project to a Unity Cloud project in the same organization. Create a Unity Project ID if one does not exist.
3. Let Package Manager finish resolving the packages from `Packages/manifest.json`.
4. Verify that Authentication, Cloud Code, and Deployment appear installed without compile errors.
5. Open **Services > Deployment**.

Cloud Code requires an authenticated player before client calls. The provided `UnityCloudServices.InitializeAsync()` initializes UGS and signs in anonymously for development. Replace/link anonymous accounts with a durable identity provider before depending on cross-device progression or account recovery.

## Create the first Cloud Code C# module

Use the Editor-generated workflow instead of hand-authoring deployment metadata:

1. Create a root-level `CloudCode` directory for server projects.
2. In the Project window create **Services > Cloud Code C# Module Reference**.
3. Open **Services > Deployment** and generate a new module solution under `/CloudCode`.
4. Implement server-authoritative endpoints in that generated .NET solution. Cloud Code modules are normal .NET projects and must not reference `UnityEngine`.
5. Deploy first to a development environment.
6. Generate client bindings when the module contract stabilizes.
7. Call a deployed module from runtime through `UnityCloudServices.CallModuleAsync<TResult>(moduleName, functionName, args)`.

Example call shape:

```csharp
var result = await UnityCloudServices.CallModuleAsync<MyResponse>(
    "WorldState",
    "GetState",
    new Dictionary<string, object> { { "sceneId", sceneId } });
```

Keep authoritative or exploitable logic in Cloud Code: reward grants, persistent world-state mutations, validated random rolls, entitlement checks, anti-cheat-sensitive calculations, and later economy/progression rules. Do not move frame-by-frame gameplay, physics, AI navigation, animation, or latency-sensitive interactions to Cloud Code.

## Recommended next Unity services, in order

### 1. Build Automation

After UVCS is connected, configure a development build target first. Keep the existing GitHub EditMode test workflow until the cloud build is proven. Then decide whether GitHub CI is still useful or redundant.

### 2. Cloud Save

Add it when persistent player/world state is actually defined. Do not cloud-save arbitrary MonoBehaviour state. Create versioned DTOs and a persistence boundary first.

### 3. Remote Config / Game Overrides

Use these for values that benefit from live tuning without a client rebuild: difficulty coefficients, encounter frequencies, infection tuning, interaction timings, feature flags, and event parameters. Do not use Remote Config as a substitute for source-controlled game design data that never needs live changes.

### 4. Addressables + Asset Manager

The project already contains large binary art. Move toward Addressables before content growth makes builds/imports painful. Asset Manager is useful for organized cloud asset workflows, but it is not a reason to upload every local working file.

### 5. Cloud Diagnostics / Analytics

Add only when there is a concrete telemetry question. Instrument crashes, failed cloud calls, load failures, and a small number of gameplay funnels. Dumping every event into analytics creates noise and cost, not insight.

## Student-plan benefits worth using

The Student plan currently includes the Unity Pro Editor, Unity Cloud access, an Odin Inspector/Validator education license, a Synty asset bundle, Asset Store benefits, and Unity Version Control allocation. Use Odin primarily for editor validation/debugging and import Synty content selectively; importing entire packs into a production repository creates unnecessary binary weight and longer import/build cycles.

## Not recommended now

- Do not install every UGS package pre-emptively.
- Do not move physical interaction logic to the cloud.
- Do not make Analytics, Economy, IAP, Multiplayer, or Leaderboards dependencies until the game design requires them.
- Do not commit Unity `Library`, generated IDE project files, credentials, service-account keys, or `.git` metadata to UVCS.
- Do not put Unity service credentials or secret keys in `Assets` or client code. Client builds are not a secret store.
