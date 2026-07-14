# Shadow Supply v0.7.3 — Clean Wall Material Hotfix

## Problem

The garage shell geometry is now correctly proportioned, but the wall surface
still looks broken.

The original `Brick_BaseColor` file was not a tileable brick texture. It was a
UV atlas created for a specific scanned modular-building FBX. Applying that
atlas to ordinary Unity cubes displayed unrelated doors, columns, plaster bands,
pipes, holes, and facade pieces across every wall module.

That is why the wall looked stitched together from random building fragments.

## Fixed

This hotfix replaces the atlas with a purpose-built seamless garage-brick
material:

- Clean repeating brick pattern
- Weathered red-brown brick color
- Neutral mortar
- Subtle grime and age variation
- No doors, windows, columns, holes, or facade elements baked into the texture
- Matching tangent-space normal map
- Two-by-two material tiling on every four-meter wall module
- Lower smoothness for a dry, worn masonry appearance

The wall geometry, dimensions, openings, roof, doors, furniture, and lot are not
changed by this hotfix.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace all prompted files, including:

   - `Assets/_Project/Art/StarterGarage/Textures/Brick_BaseColor.jpg`
   - `Assets/_Project/Art/StarterGarage/Textures/Brick_Normal.jpg`
   - `Assets/_Project/Scripts/Editor/Milestone7StarterGarageSetup.cs`

4. Reopen Unity and wait for texture import.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 7A Modular Starter Garage`

Rerunning setup refreshes the shared brick material and rebuilds the clean wall
modules.

## Acceptance test

- No doors, windows, holes, columns, or grey bands appear inside the wall texture
- Brick scale looks believable next to the player
- All modules use the same consistent masonry surface
- Module seams remain structural seams rather than texture-atlas seams
- Interior and exterior walls are readable
- Garage openings and collision remain unchanged
- No red Console errors appear

## Version

`v0.7.3-clean-wall-material`
