# Shadow Supply v0.7.5 — Garage Placement + Orientation Hotfix

## Targeted fixes
This pass addresses the issues reported after v0.7.4:

- outlets not oriented correctly
- Unity light source sitting above the fixture instead of below it
- light switch orientation incorrect
- monitor intersecting the desk
- tables/furniture oriented poorly
- `Test_Interactable` still ending up inside the garage
- `FallBack Visual` still ending up inside the garage
- breaker panel orientation wrong
- overhead opening still showing a visible gap
- meter body embedded/oriented wrong

## What changed

### Lighting
- Replaced fixture-attached lights with a child light anchor positioned below the fixture
- Rotated the light anchors downward so the spot lights actually cast toward the floor

### Furniture layout
- Repositioned and reoriented the workbench
- Repositioned and reoriented the office desk
- Raised / repositioned the monitor to sit on the desk more cleanly

### Utility orientation
- Rebuilt outlet, switch, breaker, and meter primitive assemblies so their thickness faces outward correctly
- Corrected wall-facing rotations for:
  - rear wall outlets
  - side wall outlets
  - light switch
  - breaker panel
  - electric meter

### Development helper cleanup
The loose-object cleanup pass now also catches:
- `Test_Interactable`
- `FallBack Visual`
- `fallback_visual`

### Overhead door gap
Added simple weather-seal style filler strips around the opening and widened the temporary overhead door panels slightly.

## Install
1. Close Unity
2. Extract this ZIP into the Unity project root
3. Replace the existing file when prompted
4. Open Unity
5. Open `Dev_Playground`
6. Run:

`Shadow Supply → Setup → Apply Milestone 7A Modular Starter Garage`

## Hold
Do **not** post a Discord update until this is confirmed working by the user.
