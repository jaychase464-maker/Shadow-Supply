# Shadow Supply v0.8.1 — Cable Floor-Clipping Hotfix

## Problem

The physical workbench plug and cable interaction worked, but the visual
cable sag could drop portions of the `LineRenderer` beneath the garage
floor.

## Fix

The cable now performs non-allocating downward floor probes for each
intermediate cable segment.

Each segment:

- keeps the existing slack and sag calculation
- detects upward-facing floor surfaces beneath it
- ignores the workbench and plug colliders
- rejects walls and surfaces above the cable endpoints
- clamps itself slightly above the detected floor
- retains a taut shape near maximum cable reach

This is still a controlled visual cable rather than full rope physics.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace:

   `Assets/_Project/Scripts/Runtime/Electrical/PowerCableVisual.cs`

4. Reopen Unity and wait for compilation.
5. Open `Dev_Playground`.
6. Enter Play Mode and test the workbench cable.

The Milestone 8A setup menu does not need to be rerun for this script-only
hotfix.

## Acceptance test

- Cable no longer disappears beneath the garage floor
- Cable still sags naturally while slack
- Cable still becomes taut near maximum reach
- Plug can still be grabbed, released, connected, and unplugged
- Cable does not jump onto the workbench tabletop
- No red Console errors appear

## Version

`v0.8.1-cable-floor-clipping-hotfix`
