# Shadow Supply v0.5.4 — Office Desk Scale Hotfix

## Problem

The office desk remained visibly undersized after the first floor-furniture
calibration pass.

The previous calibration was not large enough, and the placement bounds still
described the smaller desk dimensions.

## Fixed

The office desk now uses larger explicit gameplay dimensions:

- Width: `2.50 m`
- Height: `1.05 m`
- Depth: `1.00 m`
- Visual calibration: `2.30x`

The placement center and collision bounds were updated to match the new visible
size. This prevents the larger desk from visually overlapping walls or other
furniture while the placement system still considers the area empty.

No other furniture scales were changed.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace:

   `Assets/_Project/Scripts/Editor/Milestone5FurnitureOwnershipSetup.cs`

4. Reopen Unity and wait for compilation.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 5 Furniture Ownership`

Running setup again is required because the office-desk prefab and placeable
bounds must be rebuilt.

## Acceptance test

- Office desk is clearly larger than before
- Desk reads as full-size commercial furniture
- Workbench, rack, cabinet, CCTV, and keypad remain unchanged
- Desk preview and placed desk use the same scale
- Desk collision checks match the visible model
- Desk rests on the floor
- No red Console errors appear

## Version

`v0.5.4-office-desk-scale`
