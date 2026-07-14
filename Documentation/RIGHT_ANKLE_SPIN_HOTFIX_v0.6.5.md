# Shadow Supply v0.6.5 — Right Ankle Spin Hotfix

## Problem

The previous right-ankle correction applied the same stored rotation every frame.

Because the procedural humanoid pose did not fully reset the ankle's yaw before
the correction ran, the rotation accumulated continuously. The right foot then
spun around like a propeller.

## Fixed

The ankle correction is now non-accumulating.

Each `LateUpdate` it:

1. Reads the current left foot-to-toe direction.
2. Reads the current right foot-to-toe direction.
3. Calculates only the yaw difference still required.
4. Applies that remaining difference once.
5. Stops correcting when the feet are already aligned.

There is no stored rotation that gets multiplied into the ankle every frame.

The correction is also clamped to a safe maximum so malformed animation poses
cannot cause a full rotation.

## Installation

1. Close Unity.
2. Extract this ZIP into the Unity project root.
3. Replace:

   `Assets/_Project/Scripts/Runtime/Character/HumanoidFootAlignment.cs`

4. Reopen Unity and wait for compilation.
5. Enter Play mode.

The character setup does not need to be rerun as long as v0.6.4 already added
`HumanoidFootAlignment` to `LocalBody` and `ShadowBody`.

## Acceptance test

- Right ankle no longer spins.
- Right foot remains approximately aligned with the left foot.
- Standing does not create continuous rotation.
- Walking does not create continuous rotation.
- Sprinting does not create continuous rotation.
- Crouching does not create continuous rotation.
- Jumping does not create continuous rotation.
- Shadow foot remains synchronized.
- No red Console errors appear.

## Version

`v0.6.5-right-ankle-spin-hotfix`
