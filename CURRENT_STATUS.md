# Shadow Supply — Current Status

Update this file after each meaningful development session.

## Current version

- Current clean rewrite: `v0.4.0-placement-foundation`
- Repository: `jaychase464-maker/Shadow-Supply`
- Source-of-truth branch: `main`
- Save schema version: `2`
- Legacy v0.10.x work: historical design reference only
- Experimental v0.11.x island rebuild: not the active project

## Unity version

- Unity Editor: `6000.0.58f2`
- Revision: `92dee566b325`

## Package versions

- Universal Render Pipeline: `17.0.4`
- Input System: `1.14.2`
- AI Navigation: `2.0.9`
- uGUI: `2.0.0`

## Completed milestones

### Milestone 1 — Playable Foundation

- First-person movement
- Sprint, jump, and crouch
- Cursor locking
- Interaction framework
- Development playground

### Milestone 2 — Items and Inventory

- Stable item definitions
- Stackable inventory
- Eight-slot hotbar
- World pickups
- Held-item display
- Dropping and physical item behavior

### Milestone 3 — Save and Load Foundation

- Versioned JSON save files
- Three save slots
- Player transform persistence
- Inventory persistence by stable item ID
- Hotbar persistence
- World-pickup persistence
- Dropped-item physics persistence
- Save-slot metadata

### Inventory UI production pass

- Industrial full-screen inventory
- Twenty-four inventory slots
- Compact gameplay hotbar
- Item details and inspection
- Slot mouse interaction
- Use, Drop, Split, and Inspect controls

## Current milestone

### Milestone 4 — Persistent Furniture Placement

- Stable placeable definitions
- Placeable database
- Floor placement
- Grid snapping
- Rotation controls
- Green and red placement previews
- Collision validation
- Persistent placed-object IDs
- Placed-object removal
- Save schema version `2`
- Backward-compatible schema `1` migration
- Workbench, shelf, and cabinet development prefabs

## Current compilation state

- Milestone 4 package awaiting local Unity compilation and acceptance testing

## Current known risks

- Placement currently supports floor objects only.
- Placeable development prefabs are primitive placeholders.
- Placement currently uses a development HUD.
- Furniture does not yet consume an inventory item.
- Wall placement and surface placement are future milestones.
- Production prefabs will need carefully authored placement bounds.
- The repository only reflects committed and pushed local changes.

## Next planned work

1. Validate placement preview, rotation, collision, and removal.
2. Verify placed furniture survives save and load.
3. Commit and push Milestone 4.
4. Add placeable inventory items and purchasing.
5. Build wall placement for outlets, lights, posters, and security devices.
6. Begin the physical electrical system after placement persistence is stable.

## Session log

### 2026-07-14

- Changes:
  - Added persistent furniture placement foundation.
  - Advanced save schema from version `1` to version `2`.
  - Added backward-compatible migration for schema version `1`.
  - Added three development placeables.
- Tests performed:
  - Local Unity compilation and acceptance testing pending.
