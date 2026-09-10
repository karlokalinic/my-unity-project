# UNITYLAPTOP Player Humanoid Contract

The player is one physical actor with two coordinated representations:

- gameplay / collision authority: `CharacterController` plus `ProceduralHumanoidRig` physical bodies and joints;
- rendered body: the canonical `Assets/Ch01_nonPBR@Double Dagger Stab.fbx` Humanoid skin.

The rendered mesh is never allowed to introduce competing colliders or rigidbodies. Imported model physics is disabled. World collision, doors, chests, combat and navigation continue to interact with the gameplay-owned physical representation.

## Production model injection

`PlayerHumanoidSceneBuildProcessor` injects or repairs `StoreModelVisual` while production scenes are processed for build. It applies the canonical Ch01 model, validates that it imports as a usable Unity Humanoid, fits the rendered body to 1.78 m and aligns its feet to the `CharacterController` ground plane.

Production scenes currently covered are `Scena`, `INTERAKCIJA` and `VerticalSlice_Consolidated`.

Do not rely on editor-only AssetDatabase lookup at runtime. Do not move the 55 MB player source FBX into `Resources` merely to solve scene binding. The build processor is the canonical build-time binding path.

## Animation ownership

`PlayerAnimationController` owns the major procedural locomotion pose: gait phase, legs, knees, ankles, arms, pelvis, spine, chest, head, reach and recoil.

`PlayerHumanoidVisualDriver` calibrates the actual Humanoid skeleton against the procedural target/physical rigs and transfers the pose to the skinned mesh. Required mappings are Hips, Spine, Head, both upper/lower arms and hands, and both upper/lower legs and feet. Chest can fall back to UpperChest where needed.

On death, the physical ragdoll remains collision-authoritative while the real skinned player follows the physical bones. Primitive ragdoll renderers are a fallback only when the Humanoid skin cannot be bound.

## Micro-detail layers

Execution order is intentional and must be preserved:

1. `PlayerMicroMotionDetailDriver` (`-50`) removes its previous target-rig additives before the main animation Update and applies fresh breathing/inertial target motion early in LateUpdate.
2. `PlayerHumanoidVisualDriver` (default) transfers the calibrated target/physical pose to the real skin.
3. `PlayerAnatomicalDetailDriver` (`80`) adds optional neck, shoulder and toe-off motion directly to exposed Humanoid bones.
4. `PlayerFootGroundingDetailDriver` (`120`) adds stance-aware ankle compliance to the actual ground normal using cached `RaycastNonAlloc` probes.
5. `PlayerEyeGazeDetailDriver` (`140`) adds tiny optional eye saccades when LeftEye/RightEye mappings exist.
6. `PlayerFacialMicroMotion` (`160`) applies opportunistic blink/death-eye blendshapes when compatible morphs exist.

Optional Humanoid bones or facial blendshapes must degrade to a no-op. Their absence must not invalidate the player. Essential locomotion bones are enforced by the build guard.

## Motion rules

- gait phase is distance/stride based, not a fixed animation clock;
- pelvis, chest and head respond to acceleration, strafing and turning instead of only speed;
- breathing/exertion must use authoritative `PlayerMover` movement values where possible;
- secondary motion must be additive and must remove its previous frame correction before the next base pose is evaluated;
- imported FBX local bone axes are not assumed for optional anatomical corrections; world/actor axes are preferred where a robust calibrated mapping is unavailable;
- first-person `HeadAnchor` prefers the real rendered Humanoid Head but uses actor-space eye offset to avoid imported head-axis ambiguity;
- ragdoll mode disables or bypasses cosmetic motion layers that would fight physical simulation.

## WebGL budget

Frame-critical character behavior remains local. Avoid LINQ, per-frame hierarchy scans, material instantiation, repeated `GetComponent` calls and allocations in Update/LateUpdate.

Grounding uses cached non-allocating raycast storage. Missing visual references are resolved on throttled retry intervals rather than every frame. Material tuning uses `MaterialPropertyBlock`.

Full positional leg IK is deliberately not part of the current pass. The current foot system only applies stance-aware ankle orientation so it cannot stretch legs, pull the body through geometry or introduce an IK package dependency. Add full IK only with explicit contact/step tests and WebGL profiling.

## Validation

`CloudBuildGuard` blocks builds when the canonical player model does not import as a valid Humanoid with a skinned renderer and essential bone mappings.

`PlayerHumanoidIntegrationTests` checks importer settings, Avatar validity, essential bones, actual skin binding, build-time injection, physical ownership and 1.78 m world scale. `PlayerHumanoidMicroDetailTests` protects the detail-component ownership contract.

`.github/workflows/validate-player-humanoid.yml` performs source-level integrity checks without claiming Unity compilation. Unity Build Automation remains the authoritative compile/build gate for WebGL.
