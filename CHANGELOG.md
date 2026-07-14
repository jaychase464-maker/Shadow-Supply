# Changelog

All meaningful project changes should be recorded here.

## Unreleased

### Added

#### Milestone 4 — Persistent Furniture Placement

- Stable `PlaceableDefinition` assets
- Placeable database and stable placeable-ID lookup
- Floor placement mode
- Camera-centered placement preview
- Grid snapping
- Fifteen-degree rotation
- Mouse-wheel rotation
- Green valid-placement preview
- Red invalid-placement preview
- Slope validation
- Physics overlap validation
- Persistent placed-object IDs
- Removal of targeted placed objects
- Placement development HUD
- Workbench development prefab
- Storage-shelf development prefab
- Utility-cabinet development prefab
- One-click Milestone 4 setup tool
- Save and load support for placed objects

### Changed

- Active project version advanced to `v0.4.0-placement-foundation`.
- Save schema advanced from version `1` to version `2`.
- Version `1` saves migrate to version `2` with an empty placed-object list.
- Existing inventory, world-item, player, and hotbar save fields remain unchanged.

### Fixed

- Inventory cursor ambiguity
- Inventory stylesheet rendering failure
- Inventory slot pointer interaction
- Inventory inspection modal render order
- Dropped inventory-item physics

### Verified

- Milestones 1 and 2 work locally.
- Inventory interface reached usable in-engine presentation and interaction.
- Milestone 4 awaits local acceptance testing.
