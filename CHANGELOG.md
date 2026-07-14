# Changelog

## v0.6.2 — Rigged Player Integration

### Added

- Mixamo-rigged player FBX
- Unity Humanoid importer automation
- Valid Avatar enforcement
- Rigged player prefab generator
- Full-body first-person character
- Local head-bone hiding
- Local face-accessory renderer hiding
- Complete shadows-only body
- Procedural locomotion placeholder
- Bone-bound equipment sockets
- Backpack attachment foundation
- Runtime character coordinator
- Character appearance save data
- Save schema 4 migration
- Rigged-player validator

### Updated

- First-person controller now exposes read-only locomotion state.
- SaveManager now captures and restores character appearance.
- Existing schema-3 saves migrate to schema 4.

### Preserved

- First-person movement
- Inventory
- Furniture placement
- Furniture ownership
- Physical deliveries
- Existing world persistence

### Deferred

- Production animation clips
- Clothing catalog
- Backpack visual asset
- Character customization interface
