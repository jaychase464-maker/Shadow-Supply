# Shadow Supply v0.6.4 — Right Ankle Alignment Hotfix

## Problem

The rigged player character's right ankle and foot could appear rotated sideways,
creating an unnatural twisted-foot pose.

## Cause

The Mixamo rig is valid, but the right foot/toe chain has a different forward
orientation from the left foot in the imported humanoid bind pose.

The procedural locomotion system drives the major arm and leg muscles, but it
does not override the ankle's bind-pose yaw. That allows the imported sideways
orientation to remain visible.

## Fixed

A new `HumanoidFootAlignment` component now:

1. Reads the left and right humanoid foot and toe bones
2. Calculates each foot's forward direction from ankle to toes
3. Uses the correctly aligned left foot as the reference
4. Calculates a safe yaw-only correction for the right ankle
5. Applies the correction after procedural locomotion each frame
6. Prevents the correction from accumulating
7. Applies the same correction to the local and shadow bodies

The correction is limited to plausible ankle-orientation errors. Small natural
differences between the feet are ignored.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace all prompted files.
4. Reopen Unity and wait for compilation.
5. Open `Dev_Playground`.
6. Run:

   `Shadow Supply → Setup → Apply Milestone 6B Rigged Player Integration`

Rerunning setup is required so both `LocalBody` and `ShadowBody` receive the new
foot-alignment component.

## Acceptance test

- Stand still and look down at both feet
- Right foot points in approximately the same forward direction as the left foot
- Right ankle no longer appears twisted sideways
- Walking does not reintroduce the sideways foot
- Sprinting does not reintroduce the sideways foot
- Crouching keeps the ankle aligned
- Jumping keeps the ankle aligned
- Full-body shadow matches the corrected foot direction
- No red Console errors appear

The Console may print one informational line per body showing the calculated
right-ankle correction angle.

## Version

`v0.6.4-right-ankle-alignment`
