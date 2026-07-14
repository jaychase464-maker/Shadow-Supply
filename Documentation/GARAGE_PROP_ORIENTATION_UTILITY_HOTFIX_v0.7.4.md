# Shadow Supply v0.7.4 — Garage Prop Orientation + Utility Hotfix

## User-reported issues fixed

- Ceiling lights were upside down / visually wrong
- The furniture vendor and loose dev helper objects could end up jammed into the included props
- Outlets were flat cubes
- Light switch was a flat cube
- Breaker panel was a flat cube
- Office monitor was a flat cube
- CCTV camera faced the wrong direction and was too close to the front door

## What changed

### Lighting
Ceiling fixtures are now built as simple fluorescent assemblies with:
- metal housing
- diffuser panel
- correct downward-facing shape

### Monitor
The office desk now gets a modeled monitor with:
- monitor body
- screen face
- neck
- base

### Utilities
The garage now uses modeled primitive assemblies for:
- breaker panel
- electric meter
- wall outlets
- light switch

This keeps the project self-contained and avoids depending on uncertain import
paths for third-party FBX utility props.

### CCTV
The CCTV camera has been moved to the back-right wall and rotated to watch the
interior toward the overhead opening.

### Loose development objects
A new pass repositions likely scene helpers whose names contain:
- `pickup`
- `test cube`
- `vendor`
- `supplier terminal`
- `furniture vendor`

These are staged out near the front of the garage instead of overlapping the
starter props.

### Supplier terminal point
The supplier terminal anchor was moved to a clearer front-right position.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace:

   `Assets/_Project/Scripts/Editor/Milestone7StarterGarageSetup.cs`

4. Reopen Unity and let it compile.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 7A Modular Starter Garage`

## Acceptance test

- Ceiling fixtures visually face downward
- Furniture vendor / helper objects are no longer buried inside the included props
- Outlets look like outlets instead of cubes
- Light switch looks like a switch instead of a cube
- Breaker looks like a mounted utility panel
- Office monitor looks like a monitor
- CCTV is moved to the rear wall and faces inward toward the room
- No red Console errors appear

## Version

`v0.7.4-garage-prop-orientation-utility-hotfix`
