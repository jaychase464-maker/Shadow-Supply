# Shadow Supply v0.7.1 — Clean Modular Garage Rebuild

## Why the first build was rejected

The initial garage setup attempted to normalize and reuse unrelated imported FBX
pieces as structural modules.

Those source assets had inconsistent pivots, axes, dimensions, child hierarchy,
and UV layouts. The result included:

- Giant floating black meshes
- Distorted wall modules
- Incorrect wall materials
- A garage that was far too small relative to the lot
- Blown-out lighting
- Misaligned structural pieces
- Unreadable proportions

The entire generated property from v0.7.0 is discarded by this rebuild.

## New approach

The garage shell is now built from clean Unity modular prefabs with exact
dimensions:

- `4.0 m × 4.2 m` wall module
- `4.0 m × 4.0 m` roof module
- Exact steel opening columns
- Exact overhead-door header
- Exact entry-door opening
- Exact 12 m × 8 m floor slab

The supplied texture packs are still used for:

- Brick walls
- Cracked concrete
- Asphalt
- Painted roof
- Brick ceiling

Only previously verified furniture and security models are retained.

Unverified structural and street-prop FBX files are not instantiated.

## Lighting correction

The previous build created four lights with intensity `650`, which severely
overexposed the interior.

The clean rebuild uses controlled values:

- Interior light intensity: `1.8`
- Exterior street-light intensity: `1.6`
- Reduced ranges
- Soft shadows

## Installation

This hotfix requires the v0.7.0 starter-garage package because it uses the
textures already installed under:

`Assets/_Project/Art/StarterGarage/Textures`

Steps:

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace all prompted files.
4. Reopen Unity.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 7A Modular Starter Garage`

The setup deletes the entire previous `StarterGarage_Property` hierarchy before
building the clean version.

## Acceptance test

- No giant floating meshes exist
- Garage proportions are visibly 12 m × 8 m
- Walls use the brick material
- Interior is not blown out white
- Roof aligns with the wall perimeter
- Entry and overhead-door openings are correctly sized
- Player scale looks natural
- Existing furniture fits the interior
- Default walking paths remain clear
- Doors and light switch remain interactable
- No red Console errors appear

## Version

`v0.7.1-clean-modular-garage-rebuild`
