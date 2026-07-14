# Shadow Supply v0.2.1 — Dropped Item Physics Hotfix

## Problem

Items removed from the hotbar with `Q` spawned near the player but remained suspended instead of falling and tumbling naturally.

The project gravity setting is already correct at `Y = -9.81`. The hotfix therefore hardens the runtime Rigidbody setup applied to every dropped item.

## Changed

`Assets/_Project/Scripts/Runtime/Inventory/WorldItemPickup.cs`

The complete script now:

- Forces `useGravity = true`
- Forces `isKinematic = false`
- Enables collisions
- Clears Rigidbody constraints
- Enables interpolation
- Uses continuous dynamic collision detection
- Configures damping and maximum angular speed
- Ensures a solid collider exists
- Applies initial drop force through `ForceMode.VelocityChange`
- Applies randomized torque for visible tumbling
- Calls `WakeUp()` before applying motion

## Installation

1. Close Unity.
2. Extract the hotfix ZIP into the Unity project root.
3. Allow the files to merge and replace the existing script and documentation.
4. Reopen Unity and wait for compilation.
5. Enter Play Mode in `Dev_Playground`.
6. Pick up an item and press `Q` while its hotbar slot is selected.

No scene regeneration or Inspector reassignment is required.

## Acceptance test

- Dropped items move away from the player.
- Gravity immediately pulls dropped items downward.
- Items rotate and tumble while moving.
- Items collide with the floor and walls.
- Items eventually settle instead of floating.
- Settled items can still be picked up.
- No red Console errors appear.
