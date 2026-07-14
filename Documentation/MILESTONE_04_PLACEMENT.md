# Milestone 4 — Persistent Furniture Placement

## Included

- Stable placeable definitions and IDs
- Placeable database
- Floor-based placement mode
- Camera-centered preview
- Grid snapping
- Rotation controls
- Slope checks
- Collision validation
- Valid and invalid preview colors
- Persistent placed-object IDs
- Object removal
- Save schema version `2`
- Backward-compatible migration from schema version `1`
- Three generated development furniture prefabs
- One-click scene and asset setup

## Installation

1. Close Unity.
2. Extract this package into the Unity project root.
3. Reopen Unity and wait for compilation.
4. Open:

   `Assets/_Project/Scenes/Development/Dev_Playground.unity`

5. Run:

   `Shadow Supply → Setup → Apply Milestone 4 Placement`

6. Enter Play Mode.

## Controls

- `B`: Enter or exit build mode
- `1`: Workbench
- `2`: Storage Shelf
- `3`: Utility Cabinet
- `R`: Rotate clockwise
- `Shift + R`: Rotate counterclockwise
- `Mouse Wheel`: Rotate
- `Left Mouse`: Place
- `Delete` or `Backspace`: Remove targeted placed object
- `Right Mouse`: Exit build mode

## Acceptance test

### Preview

- Pressing `B` opens build mode.
- A furniture preview follows the center camera ray.
- The preview is green over a clear floor area.
- The preview becomes red when overlapping a wall, platform edge, player, or furniture.
- Looking too far away shows an invalid-placement message.

### Selection and rotation

- Keys `1`, `2`, and `3` switch furniture definitions.
- `R` rotates by fifteen degrees.
- Holding Shift while pressing `R` rotates the opposite direction.
- The mouse wheel rotates the preview.
- Rotation does not change the preview's pivot height.

### Placement and removal

- Left click places a real furniture object.
- Several objects can be placed without leaving build mode.
- Placed objects have functional colliders.
- Placement cannot overlap a previously placed object.
- Aim at a placed object and press Delete to remove it.
- Right click exits build mode and restores the hotbar and interaction system.

### Save and load

1. Place at least three furniture objects.
2. Rotate them differently.
3. Save with `F5`.
4. Remove or place additional furniture.
5. Load with `F9`.

Expected:

- Furniture returns to its saved positions and rotations.
- Furniture created after saving disappears.
- Furniture removed after saving returns.
- Existing inventory and world-item state still restores.
- Version `1` saves load without an error and contain no placed furniture.
- No red Console errors appear.

## Save migration

Schema version `2` adds:

```json
"placedObjects": []
```

Older version `1` saves are accepted. The missing list is initialized during load, preserving all previous player, inventory, hotbar, and world-item data.

## Next milestone

Milestone 5 will connect placement to ownership and acquisition:

- Furniture inventory items
- Purchase and delivery flow
- Consuming a furniture item when placed
- Returning an item when furniture is picked up
- Wall placement foundations
