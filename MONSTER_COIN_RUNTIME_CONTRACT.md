# Monster and Coin Runtime Contract

Production scene: `Assets/Scenes/INTERAKCIJA.unity`.

The archive gameplay enemy `INT_ArchiveEnforcer` keeps its existing `EnemyController`, NavMesh patrol/chase/attack behavior, damage, XP and death systems. At runtime `HorrorCreaturePresenceInstaller` binds the canonical Resources-backed Rake asset to that actor instead of adding a second disconnected monster.

The visible Rake is scaled to 2.3 m and grounded against the NavMesh actor base. Imported FBX physics is disabled. `HorrorCreatureRuntimeUtility` replaces the old generic root capsule with a compound torso/head/limb collision hierarchy named `__CreatureCollision`. This keeps the visible creature and physical blocker describing the same actor. `HorrorCreatureLocomotionDriver` samples the creature locomotion clip while NavMeshAgent remains authoritative for translation. Imported root motion is stripped after each sample.

On enemy death, the real creature skin and its compound live collision are disabled while the existing physical death-ragdoll system becomes authoritative. On revive, the real creature skin and live compound collision return and the generic root capsule remains disabled.

`CoinCollectionRuntimeInstaller` installs only in `INTERAKCIJA`. It creates a 12-coin route from the opening play area through the existing interior/archive lane. Each coin is a visible cylinder with a convex trigger MeshCollider built from that same visible mesh, a kinematic Rigidbody, local spin/bob presentation, and local collection through `CoinCollectible`. Collection credits the player's existing `CurrencyWallet` under the `coins` currency ID and never waits on a cloud call. A lightweight HUD shows collection progress and completion raises `coin_collection_complete`.

`CloudBuildGuard` blocks production if the boot scene loses the player CurrencyWallet or the existing archive enemy. `.github/workflows/validate-monster-coins.yml` is the GitHub-side static contract gate; it does not substitute for a Unity compile or WebGL runtime verification.
