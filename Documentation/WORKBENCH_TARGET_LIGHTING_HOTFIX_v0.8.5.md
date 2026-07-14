# Shadow Supply v0.8.5 — Workbench Target + Lighting Hotfix

## Confirmed foundation

Interactive production is working. This pass addresses three refinement
issues reported immediately afterward:

- the workbench is difficult to target
- garage lights do not cast correctly until toggled off and on
- illumination does not visually appear to originate from the fixture model

## Workbench targeting

The powered workbench now receives a large invisible trigger volume covering
the usable tabletop.

The trigger:

- is automatically generated from the workbench's actual rendered bounds
- does not block player movement
- works with the existing raycast interaction system
- excludes the assembly surface and output objects from its size calculation
- is recreated automatically if missing at runtime

## Lighting startup synchronization

The garage light switch now reapplies its electrical state during the first
several startup frames.

This resolves execution-order cases where the switch ran before the panel,
circuit, load system, or saved electrical state had fully settled.

The player should no longer need to toggle the lights off and on after
entering Play Mode or loading a save.

## Fixture-origin lighting

Each fluorescent fixture now contains three downward spot-light sources
distributed along the diffuser.

This replaces the single broad source and makes the illumination originate
from the visible light bar.

The diffuser now:

- uses an emissive material
- glows while its real light sources are enabled
- becomes dull when the fixture is off
- follows breaker and wall-switch state

## Installation

1. Exit Play Mode.
2. Close Unity.
3. Extract the ZIP into the project root.
4. Replace all prompted files.
5. Reopen Unity and wait for compilation.
6. Open `Dev_Playground`.
7. Run:

   `Shadow Supply → Setup → Apply v0.8.5 Workbench Target + Lighting Hotfix`

8. Run:

   `Shadow Supply → Validation → Validate v0.8.5 Workbench Target + Lighting`

Expected result:

   `WORKBENCH TARGET + LIGHTING READY`

## Acceptance test

- Workbench interaction prompt appears when aiming anywhere across most of
  the tabletop
- Invisible interaction trigger does not block movement
- Garage illumination works immediately when entering Play Mode
- Loading a save does not require an off/on toggle
- Each fixture visibly glows while on
- Light pools line up beneath the fixture bars
- Breaker and wall switch still control all fixtures
- Interactive production still opens and works
- No red Console errors appear

## Version

`v0.8.5-workbench-target-lighting-hotfix`
