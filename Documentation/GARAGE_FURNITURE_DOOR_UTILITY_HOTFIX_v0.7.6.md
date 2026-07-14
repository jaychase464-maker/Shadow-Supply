# Shadow Supply v0.7.6 — Furniture, Door, and Utility Cleanup

## Reported issues addressed

- Office desk clipping into the right wall
- Monitor floating above the desk
- `Fallback Visual` block clipping the desk
- Side-entry door showing a visible gap
- Workbench/table clipping into the left wall
- Remaining `Test_Interactable` development object
- Temporary breaker panel not using the supplied model
- Electricity meter placement and orientation

## Changes

### Furniture
- Moved the workbench farther away from the left wall
- Moved the office desk farther into the room
- Removed the placeholder utility-cabinet fallback block
- Monitor placement now uses the actual rendered desk bounds and sits on the tabletop

### Development blockers
The setup now deletes scene objects named:
- `Test_Interactable`
- `Fallback Visual`
- underscore/case variants of those names

These objects are development helpers and should not remain in the starter garage.

### Side door
- Increased door height
- Extended the frame uprights
- Lowered/extended the frame header into the wall lintel
- Added left, right, and top weather seals

### Electrical panel
The setup now uses the supplied `SwitchBox.fbx` model when available.

It is:
- normalized to a garage-appropriate size
- positioned against the inside face of the right wall
- given a generated collider
- assigned the supplied switch-box textures

A modeled primitive remains available as a fallback if Unity cannot load the FBX.

### Electricity meter
The supplied `ElectricityMeter.fbx` is now used when available and mounted on the exterior face of the right wall.

## Install

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace all prompted files.
4. Reopen Unity and allow the FBX files to import.
5. Open `Dev_Playground`.
6. Run:

`Shadow Supply → Setup → Apply Milestone 7A Modular Starter Garage`

## Confirmation gate

Do not post the Discord update until these changes are confirmed working in Unity.
