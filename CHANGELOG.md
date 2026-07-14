# Changelog

All meaningful project changes should be recorded here.

## Unreleased

### Added

#### v0.3.1 — Inventory UI Redesign

- Full runtime UI Toolkit inventory
- Industrial UXML layout and USS styling
- Twenty-four dynamic inventory slots
- Eight-slot in-menu hotbar
- Compact gameplay hotbar
- Selected and equipped slot states
- Item detail panel
- Equipment paper-doll presentation
- Cash, dirty cash, heat, and temporary weight presentation
- Item icon resource pipeline
- Development item icon artwork
- Functional Use, Drop, Split, and Inspect actions
- Large item inspection modal
- Inventory-to-hotbar swapping
- Stack splitting into empty slots
- Inventory keyboard and mouse-wheel controls
- One-click inventory UI setup tool

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

### Changed

- Active project version advanced to `v0.3.1-inventory-ui`.
- The temporary `OnGUI` inventory has been replaced.
- Inventory slots now support safe swapping and splitting.
- Hotbar dropping can target any inventory slot through the UI.
- Save schema remains version `1`.

### Fixed

- Resolved `CS0104` in `WorldItemPickup.cs` by explicitly using `UnityEngine.Random.rotation`.
- Dropped inventory items use dynamic physics and gravity.

### Verified

- Milestone 1 playable foundation works locally.
- Milestone 2 inventory behavior works locally.
- Milestone 3 and the inventory redesign await local acceptance testing.
