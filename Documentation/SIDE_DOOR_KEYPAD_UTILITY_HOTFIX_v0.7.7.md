# Shadow Supply v0.7.7 — Side Door, Keypad, and Utility Hotfix

## Confirmed issues addressed

- Supplied electricity-meter model faced backward
- Imported switch-box model produced unusable geometry
- Side-door masonry opening was narrower than the complete door frame
- Only one side of the entry door had a keypad

## Changes

### Electricity meter

The supplied meter model remains in use, but its wrapper has been rotated
180 degrees and moved slightly away from the wall. Its front should now face
outward.

### Breaker panel

The imported switch-box mesh is no longer used for the installed breaker.
Its source hierarchy and axes produced a distorted wall-mounted result.

The garage now receives a deterministic panel assembly containing:

- metal cabinet
- closed front door
- handle
- warning-label plate
- four hinge pieces
- simple box collider

The original FBX remains in the project for later manual inspection, but it
is not instantiated by the garage setup.

### Side-door opening

The actual brick-wall opening has been widened from approximately `1.0 m`
to `1.5 m`.

This matches the full frame width instead of forcing the frame and door into
an undersized opening.

### Dual keypads

Two keypad instances are now installed:

- interior keypad
- exterior keypad

They are mounted on opposite faces of the same wall beside the entry door.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace the existing setup script.
4. Reopen Unity.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 7A Modular Starter Garage`

## Acceptance test

- Meter face points away from the exterior wall
- Breaker appears as a normal closed electrical panel
- Side-door frame fits inside the brick opening
- No brick overlaps either side of the frame
- Interior keypad appears beside the door
- Exterior keypad appears at the matching location outside
- Entry door still opens and closes
- No red Console errors appear

## Version

`v0.7.7-side-door-keypad-utility-hotfix`
