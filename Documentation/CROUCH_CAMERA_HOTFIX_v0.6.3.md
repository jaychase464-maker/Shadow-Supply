# Shadow Supply v0.6.3 — Crouch Recovery and Camera Hotfix

## Problems

Two issues were confirmed after the rigged first-person body was installed:

1. The player could crouch but could not stand back up.
2. The crouching camera entered the torso and exposed the inside of the
   character mesh.

## Cause: unable to stand

The standing-clearance test used a broad capsule query against every collider in
the obstruction mask.

That query could count the player's own `CharacterController`, character
hierarchy, or ground contact as an overhead obstruction. The controller then
forced the player back into the crouched state every time the user attempted to
stand.

## Fix: self-filtered upper clearance

The controller now checks only the additional volume needed above the crouched
character.

The new query:

- Ignores the player's own `CharacterController`
- Ignores colliders anywhere beneath the player hierarchy
- Ignores ground-level contacts
- Ignores triggers
- Still blocks standing underneath actual ceilings, shelves, or furniture

The crouch key remains a toggle using `Left Ctrl`, `C`, or the configured gamepad
button.

## Cause: seeing inside the body

The first-person camera lowers substantially while crouching, but the local
character body remained centered directly around the camera.

The procedural crouch pose did not move the torso far enough away from the
camera, allowing the near plane to enter the chest and neck geometry.

## Fix: first-person body alignment

A new local-only body alignment component keeps the visible first-person body
behind the camera:

- Standing offset: slightly down and behind the camera
- Crouching offset: farther behind the camera
- Additional retreat while looking sharply downward
- Smooth transitions between standing and crouching

Only `LocalBody` receives this offset. `ShadowBody` remains centered on the
player so the world shadow stays correctly aligned.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace all prompted files.
4. Reopen Unity and wait for compilation.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 6B Rigged Player Integration`

Rerunning setup is required so `LocalBody` receives the new
`FirstPersonBodyAlignment` component.

## Acceptance test

### Crouch recovery

- Press `C` or `Left Ctrl` to crouch
- Press the same control again in an open area
- Player returns to standing
- Crouching beneath a low obstacle correctly prevents standing
- Moving out from under the obstacle allows standing

### Camera/body separation

- Crouch in an open area
- Look straight ahead
- Look sharply downward
- No chest, neck, face, or inside-body geometry fills the camera
- Torso and legs remain visible when looking down
- Hands remain attached and animated

### Shadows

- Complete body shadow remains centered beneath the player
- Shadow does not receive the local camera offset
- No duplicate shadow appears

### Regression

- Walking, sprinting, jumping, and interaction still work
- Inventory and furniture systems still work
- Save/load still works
- No red Console errors appear

## Version

`v0.6.3-crouch-camera-hotfix`
