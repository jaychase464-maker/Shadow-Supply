# Shadow Supply v0.5.2 — Imported Furniture Scale Hotfix

## Problem

Imported furniture appeared at extreme and inconsistent sizes:

- Workbench and office desk could become nearly invisible
- CCTV camera could become enormous
- Placement previews did not match the intended furniture dimensions

## Root cause

The setup tool calculated a scale ratio from the rendered bounds, then replaced
the imported FBX root's `localScale`.

Some FBX importers already provide a root scale such as `0.01` or `100`. Replacing
that value discarded the model's required import conversion:

- Models requiring a large importer scale became tiny
- Models requiring a small importer scale became enormous

The old code also scaled each axis independently, which could distort the model.

## Fixed

- Preserves each FBX model's imported root scale
- Multiplies the imported scale by a calculated correction factor
- Uses one uniform correction factor to preserve model proportions
- Fits every model inside its intended placement dimensions
- Recalculates renderer bounds after scaling
- Realigns floor objects to ground level
- Realigns wall objects to the mounting surface
- Logs the original size, corrected size, and multiplier during setup

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace:

   `Assets/_Project/Scripts/Editor/Milestone5FurnitureOwnershipSetup.cs`

4. Reopen Unity and wait for compilation.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 5 Furniture Ownership`

Running the setup again is required because it rebuilds the generated furniture
prefabs with corrected scales.

## Acceptance test

- Workbench is approximately human workbench size
- Storage rack is approximately 2.25 meters tall
- Office desk is approximately waist height
- CCTV camera is small enough to mount near a ceiling
- Door keypad is approximately hand-sized
- Preview and final placed object use the same scale
- No model is stretched or flattened
- No red Console errors appear

## Version

`v0.5.2-imported-furniture-scale-hotfix`
