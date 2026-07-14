# Milestone 3 — Save and Load Foundation

## Included

- Versioned JSON save files
- Three save slots
- Atomic save writes with backup files
- Player position persistence
- Player yaw and camera-pitch persistence
- Exact inventory-slot persistence
- Stable item-ID lookup through an item database
- Hotbar-selection persistence
- Active world-pickup persistence
- Dropped-item position, rotation, velocity, and angular-velocity persistence
- Save-slot metadata
- Scene-aware loading foundation
- On-screen development save HUD
- One-click Milestone 3 setup tool

## Setup

1. Extract this package into the Unity project root.
2. Reopen Unity and wait for compilation.
3. Open:

   `Assets/_Project/Scenes/Development/Dev_Playground.unity`

4. Run:

   `Shadow Supply → Setup → Apply Milestone 3 Save System`

5. Enter Play Mode.

## Controls

- `F1`: Select save slot 1
- `F2`: Select save slot 2
- `F3`: Select save slot 3
- `F5`: Save the selected slot
- `F9`: Load the selected slot

## Save location

Save files are written outside the repository under Unity's
`Application.persistentDataPath`:

`ShadowSupply/Saves/slot_1.json`

Each overwritten save also produces a `.bak` backup.

## Acceptance test

1. Pick up several items.
2. Put different items into multiple hotbar slots.
3. Drop one or more items into the world.
4. Walk to a different location and look in a distinct direction.
5. Select slot 1 with `F1`.
6. Press `F5`.
7. Change the inventory, move elsewhere, and drop more items.
8. Press `F9`.

Expected:

- Player position returns to the saved position.
- Player view direction returns.
- Inventory contents and exact slot locations return.
- Selected hotbar slot returns.
- World pickups return to their saved state.
- Picked-up objects that were absent from the save remain absent.
- Dropped physics objects return at their saved positions.
- No red Console errors appear.

## Save compatibility

The first save schema is version `1`.

Future systems must extend the save model without silently changing or reusing existing fields. When the schema changes, add an explicit migration path and increment the save version.

## Next milestone

Milestone 4 will implement the first persistent furniture-placement foundation:

- Placeable-object definitions
- Placement preview
- Rotation
- Collision validation
- Floor placement
- Persistent placed-object IDs
- Save/load integration
