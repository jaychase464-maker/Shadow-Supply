# Changelog

All meaningful project changes should be recorded here.

## Unreleased

### Fixed

#### v0.2.1 — Dropped Item Physics Hotfix

- Dropped inventory items now explicitly use gravity.
- Dropped Rigidbody components are forced into a non-kinematic dynamic state.
- Inherited Rigidbody constraints are cleared when an item is dropped.
- Continuous dynamic collision detection and interpolation are enabled.
- Dropped items receive randomized torque so they tumble naturally.
- A solid fallback collider is added when an item display has no usable collider.
- Rigidbody wake-up is forced after drop initialization.

### Added

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

### Changed

- Active project version advanced to `v0.2.1-inventory-hotfix`.
- The active project remains a clean rewrite using Unity `6000.0.58f2`.
- Legacy and experimental versions remain design references rather than active codebases.

### Verified

- Milestone 1 playable foundation works locally.
- Milestone 2 inventory, hotbar, pickup, held-item, and drop controls work locally.
- Dropped-item physics hotfix is awaiting local verification.
