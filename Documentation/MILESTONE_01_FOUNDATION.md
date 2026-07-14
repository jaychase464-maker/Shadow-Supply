# Milestone 1 — Playable Foundation

## Included

- Runtime and Editor assembly definitions
- Persistent `GameBootstrap`
- CharacterController-based first-person movement
- Mouse and gamepad look
- Sprint, jump, and toggle crouch
- Standing-clearance check
- Cursor lock/unlock
- Camera-centered interaction raycast
- `IInteractable` contract
- Minimal interaction prompt HUD
- Test interactable
- One-click development playground builder

## Setup

1. Extract this package into the Unity project root.
2. Open the project in Unity `6000.0.58f2`.
3. Wait for script compilation.
4. Run `Shadow Supply → Setup → Create Milestone 1 Playground`.
5. Open `Assets/_Project/Scenes/Development/Dev_Playground.unity`.
6. Enter Play Mode.

## Controls

- WASD or arrow keys: Move
- Mouse: Look
- Left Shift: Sprint
- Left Ctrl or C: Toggle crouch
- Space: Jump
- E: Interact
- Escape: Lock/unlock cursor

## Acceptance checklist

- No red Console errors
- Player moves and looks correctly
- Sprint increases movement speed
- Crouch lowers the controller and camera
- Space jumps only while grounded
- Looking at the test cube displays an interaction prompt
- Pressing E changes the test cube state
- Escape releases and restores the cursor

## Next milestone

Milestone 2 will create item definitions, pickups, stackable inventory, a hotbar, held-item representation, and inventory UI.
