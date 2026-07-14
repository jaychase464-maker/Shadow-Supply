# Shadow Supply v0.5.5 — Furniture Collision Hotfix

## Problems

The imported floor furniture used one solid rectangular collider covering the
entire placement volume.

This caused two opposite problems:

- The player could not approach the workbench or office desk naturally because
  the empty space underneath was treated as solid.
- After visual-scale calibration, parts of the visible models extended beyond
  the old collider, allowing the player to clip through the sides.

## Fixed

### Workbench

The single solid box was replaced with compound colliders:

- One tabletop collider
- Four leg colliders
- Open space beneath the tabletop remains accessible

### Office desk

The single solid box was replaced with:

- One desktop collider
- One left drawer-pedestal collider
- One right cabinet-pedestal collider
- Open knee space beneath the center of the desk

### Placement validation

The placeable definition now derives its placement-validation bounds from the
combined generated colliders. The placement system therefore checks the same
physical footprint used during gameplay.

### Other furniture

- Storage rack remains a full-volume collider
- Utility cabinet remains a full-volume collider
- CCTV camera remains unchanged
- Door keypad remains unchanged

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace:

   `Assets/_Project/Scripts/Editor/Milestone5FurnitureOwnershipSetup.cs`

4. Reopen Unity and wait for compilation.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 5 Furniture Ownership`

Running setup again is required because it rebuilds the generated prefabs and
placeable collision bounds.

## Acceptance test

### Office desk

- Player can walk closer to the center of the desk
- Knee space is no longer treated as a solid block
- Player cannot walk through either cabinet pedestal
- Player cannot walk through the desktop or outer sides

### Workbench

- Player can approach the work surface naturally
- Empty space between the legs is accessible
- Player cannot pass through the tabletop
- Player cannot pass directly through the four legs

### Placement

- Preview collision matches the physical furniture footprint
- Furniture cannot overlap walls or other furniture
- Preview and final object use identical collision dimensions
- No red Console errors appear

## Version

`v0.5.5-furniture-collision-hotfix`
