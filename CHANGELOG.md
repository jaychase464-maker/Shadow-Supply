# Changelog

All meaningful project changes should be recorded here.

## Unreleased

### Added

#### Milestone 3 — Save and Load Foundation

- Save schema version `1`
- Three independent save slots
- Atomic JSON save writes
- Backup save files
- Save-slot metadata
- Player-position persistence
- Player-yaw persistence
- Camera-pitch persistence
- Exact inventory-slot persistence
- Item quality and condition persistence
- Stable item-database lookup
- Hotbar-selection persistence
- World-pickup persistence
- Dropped-item physics-state persistence
- Scene-aware loading foundation
- Development save HUD
- One-click Milestone 3 setup tool

#### Milestone 2 — Items and Inventory

- Stable serialized item IDs
- Item categories
- Item quality tiers
- Item condition values
- Stackable inventory slots
- Twenty-four-slot player inventory
- Eight-slot hotbar
- World item pickups
- Partial pickup handling
- Held-item visual display
- Hotbar selection with number keys and mouse wheel
- Item dropping and repickup
- Temporary inventory and hotbar HUD
- Four development item definitions
- One-click Milestone 2 playground updater
- Milestone 2 setup and acceptance documentation

#### Milestone 1 — Playable Foundation

- Runtime and Editor assembly definitions
- Persistent game bootstrap
- First-person walking, sprinting, jumping, crouching, and looking
- Cursor lock and unlock support
- Interaction raycast and `IInteractable` contract
- Temporary interaction prompt HUD
- Test interactable
- One-click development playground generator
- Milestone 1 setup and acceptance documentation

### Fixed

#### Milestone 3 Package Revision 2

- Resolved `CS0104` in `WorldItemPickup.cs` by explicitly using `UnityEngine.Random.rotation`.
- The save-system package now compiles without conflicting with `System.Random`.

#### v0.2.1 — Dropped Item Physics Hotfix

- Dropped inventory items explicitly use gravity.
- Dropped Rigidbody components are forced into a dynamic state.
- Inherited Rigidbody constraints are cleared.
- Continuous dynamic collision detection and interpolation are enabled.
- Dropped items receive randomized torque.
- A solid fallback collider is added when needed.

### Changed

- Active project version advanced to `v0.3.0-save-foundation`.
- World pickups now carry persistent runtime IDs.
- Player inventory exposes a controlled full-restore operation.
- First-person view yaw and pitch can be restored by the save system.

### Verified

- Milestone 1 playable foundation works locally.
- Milestone 2 inventory, hotbar, pickup, held-item, and drop controls work locally.
- Dropped-item physics hotfix is committed.
- Milestone 3 is awaiting local acceptance testing.
