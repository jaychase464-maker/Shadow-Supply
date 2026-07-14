# Shadow Supply — Bug Tracker

Use one entry per bug.

## Active bugs

### BUG-004 — Dropped inventory items float instead of falling

- Status: Fix prepared; local verification pending
- Severity: Medium
- Reported version: `v0.2.0-inventory`
- Scene: `Assets/_Project/Scenes/Development/Dev_Playground.unity`
- Affected scripts:
  - `Assets/_Project/Scripts/Runtime/Inventory/WorldItemPickup.cs`
- Reproduction steps:
  1. Pick up an item.
  2. Select its hotbar slot.
  3. Press `Q` to drop one item.
- Expected:
  - The dropped item is released as a dynamic physics object.
  - Gravity pulls it to the ground.
  - Initial force and torque make it tumble naturally.
- Actual:
  - The item remains suspended near the drop position.
- Fix prepared:
  - Explicitly enable gravity and dynamic Rigidbody state.
  - Clear inherited Rigidbody constraints.
  - Enable collision detection and interpolation.
  - Add a solid collider when a display prefab has none.
  - Apply initial force and randomized torque.
  - Wake the Rigidbody immediately after configuration.

## Previously reported bugs

### BUG-001 — Wall outlets face the wrong direction

- Status: Historical legacy-project report; not present in the active clean rewrite
- Area: Electrical placement
- Likely affected assets:
  - Outlet prefab
  - `PowerOutlet.cs`
  - Scene-builder placement rotation

### BUG-002 — Electrical connectors do not insert correctly

- Status: Historical legacy-project report; not present in the active clean rewrite
- Area: Physical plug system
- Likely affected scripts:
  - `MachinePowerConnection.cs`
  - `PowerCableVisual.cs`
  - Outlet and connector snap transforms

### BUG-003 — Entrance door blocked by electrical panels

- Status: Historical legacy-project report; not present in the active clean rewrite
- Area: Starter property scene layout
- Likely affected assets:
  - Garage scene
  - Electrical-panel placement
  - Scene builder

## Resolved bugs

Move verified fixed bugs here and include:

- Fixed version
- Commit hash
- Files changed
- Summary of fix
- Regression tests
