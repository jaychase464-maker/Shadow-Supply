# Shadow Supply v0.7.2 — Overhead Door Track Hotfix

## Problem

The temporary garage door used a vertical slide motion.

When opened, the entire segmented door moved above the roofline and remained
visible as a large floating black panel.

## Fixed

The temporary garage door now behaves like a tilt-up sectional door:

- The pivot is located at the top of the opening
- Closed panels extend downward into the doorway
- Opening rotates the complete door inward by 90 degrees
- The open door rests beneath the garage ceiling
- Nothing rises above the roofline
- Collision moves with the door
- The door still starts open
- The same interaction component remains in use

Static visual tracks were also added:

- Left vertical track
- Right vertical track
- Left ceiling track
- Right ceiling track

These tracks do not create unnecessary collision.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace:

   `Assets/_Project/Scripts/Editor/Milestone7StarterGarageSetup.cs`

4. Reopen Unity and wait for compilation.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 7A Modular Starter Garage`

Rerunning setup is required because the overhead-door hierarchy and pivot must be
rebuilt.

## Acceptance test

- No door panels appear above the roof
- Open door rests horizontally beneath the ceiling
- Opening remains clear enough for the player
- Closing rotates the door down into the opening
- Door collision follows the panels
- Door interaction prompt still appears
- Roof and wall geometry remain unchanged
- No red Console errors appear

## Version

`v0.7.2-overhead-door-track-hotfix`
