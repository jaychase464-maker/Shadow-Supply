# Shadow Supply v0.5.3 — Floor Furniture Calibration Hotfix

## Problem

The importer-scale correction fixed the CCTV camera and keypad, but two floor
models remained much smaller than their intended gameplay dimensions:

- Workbench
- Office desk

The storage rack and utility cabinet were already close to usable scale.

## Cause

The workbench and office-desk FBX files contain source bounds that do not map
cleanly to the visible furniture dimensions. Automatic bounds fitting therefore
included nonrepresentative source-space measurements and scaled the visible model
too far down.

A single automatic normalization rule cannot reliably calibrate every unrelated
third-party FBX asset.

## Fixed

The setup tool now supports a model-specific visual calibration multiplier after
preserving Unity's importer scale and applying automatic bounds normalization.

Current calibration:

- Workbench: `2.60x`
- Office Desk: `1.75x`
- Storage Rack: `1.00x`
- CCTV Camera: `1.00x`
- Door Keypad: `1.00x`

The placement colliders and saved placement bounds remain at their intended
real-world dimensions. Only the imported visible mesh is recalibrated.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace:

   `Assets/_Project/Scripts/Editor/Milestone5FurnitureOwnershipSetup.cs`

4. Reopen Unity and wait for compilation.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 5 Furniture Ownership`

Running setup again is required because the generated furniture prefabs must be
rebuilt.

## Acceptance test

- Workbench is visibly full-sized and usable
- Office desk is normal desk width and height
- Storage rack remains approximately the same size
- Utility cabinet remains approximately the same size
- CCTV camera remains correctly sized
- Door keypad remains correctly sized
- Placement previews match final placed objects
- Floor furniture still rests on the floor
- No red Console errors appear

## Version

`v0.5.3-floor-furniture-calibration`
