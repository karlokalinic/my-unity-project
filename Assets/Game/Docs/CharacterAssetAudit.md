# Game Asset Audit and Organization

Generated: 2026-03-28

## Canonical Folder Layout

All production character/art assets are now under one root:

- `Assets/Game/Art/Characters`
- `Assets/Game/Art/Animations`
- `Assets/Game/Art/Materials`
- `Assets/Game/Art/Prefabs`
- `Assets/Game/Art/Textures`

Mixamo source actors used by runtime bootstraps:

- `Assets/Game/Art/Characters/Source/Ch01_nonPBR@Double Dagger Stab.fbx`
- `Assets/Game/Art/Characters/Source/Ch02_nonPBR@Double Dagger Stab.fbx`
- `Assets/Game/Art/Characters/Source/Ch11_nonPBR@Double Dagger Stab.fbx`

## Character File Stats (Key Runtime Assets)

- `Ch01_nonPBR@Double Dagger Stab.fbx`: 55,206,848 bytes
- `Ch02_nonPBR@Double Dagger Stab.fbx`: 50,376,480 bytes
- `Ch11_nonPBR@Double Dagger Stab.fbx`: 62,914,304 bytes
- `Antonia_2K.fbx`: 5,896,924 bytes
- `MB-Lab_Male_2K.fbx`: 3,646,092 bytes
- `Vitruvian_2K.fbx`: 8,429,132 bytes
- `UAL1_Standard.fbx`: 24,838,044 bytes
- `UAL2_Standard.fbx`: 24,365,116 bytes

Texture DB stats (`Assets/Game/Art/Characters/Textures/CharMorphDb`):

- texture files: 25
- total size: 29,452,130 bytes

Prefab count in repository: 0 (`.prefab` files)

## Connections / What Is Wired

- Runtime player/friendly/enemy model loading paths updated in:
  - `Assets/Scripts/World/SampleSceneTestingBootstrap.cs`
  - `Assets/Scripts/World/VerticalSliceScenaBootstrap.cs`
- CharMorph texture root path updated to:
  - `Assets/Game/Art/Characters/Textures/CharMorphDb`
- Fallback underwear source switched to 2K Antonia texture:
  - `Assets/Game/Art/Characters/CharMorph/2K/Antonia/Antonia_2K.fbm/underwear.png`
- Art pack editor tool paths updated to `Assets/Game/Art/...` in:
  - `Assets/Editor/HolstinArtPackTools.cs`

## Quality / Risk Classification

### Production-usable now

- Ch01/Ch02/Ch11 Mixamo models (runtime visual actors)
- CharMorphDb texture set (for procedural/fallback mapping)
- UAL locomotion FBX files (need clip setup in Unity importer/controller)

### Moved out of production project (problematic)

- `Reom.fbx` moved to:
  - `C:\Users\kalin\Documents\GitHub\extra\Quarantine\Reom`
- Reason: previously reported Unity importer warning: self-intersecting polygon(s) discarded.

## Extra Folder Analysis

Analyzed: `C:\Users\kalin\Documents\GitHub\extra`

Findings:

- `extra/Assets` mostly contains scene snapshots/navmesh assets (no robust character pipeline files).
- External sci-fi kits are raw DCC exports (large import surface):
  - `Modular SciFi MegaKit[Standard]`: 378 FBX, 191 OBJ, 190 GLTF (+ 24 PNG)
  - `Sci-Fi Essentials Kit[Standard]`: 74 FBX, 37 OBJ, 37 GLTF (+ 68 PNG, 1 JPG)

Decision:

- Not moved into production by default, because they are not validated against this scene/pipeline yet and would add heavy unverified import surface.
- Only safe reference docs copied into game directory:
  - `Assets/Game/Docs/ExtraReference/UE_Reference_MeshMap.txt`
  - `Assets/Game/Docs/ExtraReference/extra_art_readme.txt`

## Validation

- `dotnet build Assembly-CSharp.csproj` succeeds.
- `dotnet build Assembly-CSharp-Editor.csproj` succeeds.

